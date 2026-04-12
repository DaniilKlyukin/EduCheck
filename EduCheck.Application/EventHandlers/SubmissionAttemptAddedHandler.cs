using EduCheck.Application.Contracts;
using EduCheck.Core.Domain.Events;
using MassTransit;
using MediatR;

namespace EduCheck.Application.EventHandlers;

public class SubmissionAttemptAddedHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<SubmissionAttemptAddedEvent>
{
    public async Task Handle(SubmissionAttemptAddedEvent notification, CancellationToken ct)
    {
        await publishEndpoint.Publish(new AnalyzeSubmissionTask(notification.HistoryId), ct);
    }
}
