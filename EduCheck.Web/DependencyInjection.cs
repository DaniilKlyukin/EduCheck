using EduCheck.Web.Interfaces;
using EduCheck.Web.Services;

namespace EduCheck.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IServiceCollection services)
    {
        services.AddSingleton<ISubmissionStatusStyleProvider, SubmissionStatusStyleProvider>();
    }
}