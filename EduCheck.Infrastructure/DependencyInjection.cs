using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EduCheck.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IFileStorage, MinioFileStorage>();
        services.AddSingleton<ISubmissionStatusLabelProvider, SubmissionStatusLabelProvider>();
        services.AddHttpClient<IAiCodeReviewer, OllamaCodeReviewer>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434/");
            client.Timeout = TimeSpan.FromMinutes(10); 
        });
    }
}
