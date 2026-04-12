using EduCheck.Application.Behaviors;
using EduCheck.Application.Interfaces;
using EduCheck.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EduCheck.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISubmissionService, SubmissionService>();

        return services;
    }
}
