using MediatR;
using Microsoft.Extensions.Logging;

namespace EduCheck.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Starting request {Name}", requestName);

        var response = await next();

        logger.LogInformation("Finished request {Name}", requestName);

        return response;
    }
}
