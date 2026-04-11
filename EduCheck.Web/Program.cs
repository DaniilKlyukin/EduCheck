using EduCheck.Infrastructure;
using EduCheck.Infrastructure.Data;
using EduCheck.Web;
using EduCheck.Web.Components;
using Microsoft.EntityFrameworkCore;
using Minio;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

var storageSettings = builder.Configuration.GetSection("StorageSettings");
builder.Services.AddMinio(configureSource => configureSource
    .WithEndpoint(storageSettings["Endpoint"])
    .WithCredentials(storageSettings["AccessKey"], storageSettings["SecretKey"])
    .WithSSL(false));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddInfrastructure();
builder.Services.AddWebServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
