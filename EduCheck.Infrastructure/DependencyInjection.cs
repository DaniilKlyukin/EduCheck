using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Interfaces;
using EduCheck.Infrastructure.Data.Repositories;
using EduCheck.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EduCheck.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
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
            client.BaseAddress = new Uri("http://localhost:11434/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        return services;
    }
}