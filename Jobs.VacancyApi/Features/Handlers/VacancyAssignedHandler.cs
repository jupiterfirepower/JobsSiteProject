using Jobs.VacancyApi.Features.Notifications;
using MediatR;

namespace Jobs.VacancyApi.Features.Handlers;

public class VacancyAssignedHandler(ILogger<VacancyAssignedHandler> logger) : INotificationHandler<VacancyCreatedNotification>
{
    public Task Handle(VacancyCreatedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"handling notification for vacancy creation with id : {notification.Id}. assigning.");
        return Task.CompletedTask;
    }
}