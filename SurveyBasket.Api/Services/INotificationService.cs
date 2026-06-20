namespace SurveyBasket.Api.Services
{
    public interface INotificationService
    {
        Task SendNewPollsNotification(int? pollId = null);
    }
}
