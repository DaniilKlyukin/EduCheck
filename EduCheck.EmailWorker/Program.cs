using EduCheck.Core.Interfaces;
using EduCheck.EmailWorker;
using EduCheck.Infrastructure.Data;
using EduCheck.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Minio;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var storageSettings = builder.Configuration.GetSection("StorageSettings");
builder.Services.AddMinio(configureSource => configureSource
    .WithEndpoint(storageSettings["Endpoint"])
    .WithCredentials(storageSettings["AccessKey"], storageSettings["SecretKey"])
    .WithSSL(false));

builder.Services.AddScoped<IEmailParser, EmailParser>();
builder.Services.AddScoped<IFileStorage, MinioFileStorage>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();