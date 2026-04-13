using MassTransit;

namespace EduCheck.EmailWorker;

public static class DependencyInjection
{
    public static IServiceCollection AddEmailWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<SubmissionAnalysisConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitSettings = configuration.GetSection("RabbitMq");

                cfg.Host(rabbitSettings["Host"] ?? "localhost", "/", h =>
                {
                    h.Username(rabbitSettings["Username"] ?? "guest");
                    h.Password(rabbitSettings["Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("submission-analysis-queue", e =>
                {
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromMinutes(2)));
                    e.ConfigureConsumer<SubmissionAnalysisConsumer>(context);
                });
            });
        });

        return services;
    }
}