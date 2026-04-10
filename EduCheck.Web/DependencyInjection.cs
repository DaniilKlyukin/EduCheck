using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Services;

namespace EduCheck.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IServiceCollection services)
    {
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
    }
}
