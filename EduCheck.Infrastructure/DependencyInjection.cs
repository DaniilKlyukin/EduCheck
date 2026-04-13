using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Interfaces;
using EduCheck.Infrastructure.Data.Repositories;
using EduCheck.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace EduCheck.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();

        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IFileStorage, MinioFileStorage>();
        services.AddScoped<IEmailParser, EmailParser>();
        services.AddScoped<ICodeAnalyzer, RoslynCodeAnalyzer>();
        services.AddSingleton<ISubmissionStatusLabelProvider, SubmissionStatusLabelProvider>();

        services.AddHttpClient<IAiCodeReviewer, OllamaCodeReviewer>(client =>
        {
            var url = configuration["AiSettings:Url"] ?? "http://localhost:11434/";
            client.BaseAddress = new Uri(url);
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        return services;
    }
}