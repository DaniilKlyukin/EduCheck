using EduCheck.Core.Interfaces;
using EduCheck.EmailWorker.Consumers;
using EduCheck.Infrastructure.Services;
using MassTransit;

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

        services.AddMassTransit(x =>
        {
            x.AddConsumer<SubmissionAnalysisConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("submission-analysis-queue", e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromMinutes(2)));
                    e.ConfigureConsumer<SubmissionAnalysisConsumer>(context);
                });
            });
        });
    }
}
