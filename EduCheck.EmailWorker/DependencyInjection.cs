using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Services;

namespace EduCheck.EmailWorker;

public static class DependencyInjection
{
    public static void AddEmailWorkerServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailParser, EmailParser>();
        services.AddScoped<IFileStorage, MinioFileStorage>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICodeAnalyzer, RoslynCodeAnalyzer>();
        services.AddHttpClient<IAiCodeReviewer, OllamaCodeReviewer>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });
    }
}
