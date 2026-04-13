using EduCheck.Application;
using EduCheck.EmailWorker;
using EduCheck.Infrastructure;
using EduCheck.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Minio;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());


var storageSettings = builder.Configuration.GetSection("StorageSettings");
builder.Services.AddMinio(configureSource => configureSource
    .WithEndpoint(storageSettings["Endpoint"])
    .WithCredentials(storageSettings["AccessKey"], storageSettings["SecretKey"])
    .WithSSL(false));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEmailWorkerServices(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();