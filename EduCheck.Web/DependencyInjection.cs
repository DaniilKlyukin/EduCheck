using EduCheck.Web.Interfaces;
using EduCheck.Web.Services;
using MassTransit;

namespace EduCheck.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddSingleton<ISubmissionStatusStyleProvider, SubmissionStatusStyleProvider>();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });
        });

        return services;
    }
}