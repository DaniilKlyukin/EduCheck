using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Services;

namespace EduCheck.EmailWorker;

public static class DependencyInjection
{
    public static void AddEmailWorkerServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailParser, EmailParser>();
        services.AddScoped<IFileStorage, MinioFileStorage>();
        services.AddScoped<ICodeAnalyzer, RoslynCodeAnalyzer>();
    }
}
