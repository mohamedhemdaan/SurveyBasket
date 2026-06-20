
using Microsoft.AspNetCore.Identity.UI.Services;
using SurveyBasket.Api.Helpers;

namespace SurveyBasket.Api.Services
{
    public class NotificationService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IHttpContextAccessor httpContextAccessor
        ) : INotificationService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task SendNewPollsNotification(int? pollId = null)
        {
            IEnumerable<Poll> polls = [];
            if (pollId.HasValue)
            {
                var poll = await _dbContext.Polls.SingleOrDefaultAsync(p => p.Id == pollId && p.IsPublished);
                polls = [poll!];
            }
            else
            {
                polls = await _dbContext.Polls
                     .Where(p => p.IsPublished && p.StartsAt == DateOnly.FromDateTime(DateTime.Now))
                     .AsNoTracking()
                     .ToListAsync();
            }


            var users = await _userManager.Users.ToListAsync();

            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

            foreach (var poll in polls)
            {
                foreach (var user in users)
                {
                    var placeholders = new Dictionary<string, string>()
                    {
                        {"{{name}}",user.FirstName },
                        {"{{pollTill}}",poll.Title },
                        {"{{endDate}}",poll.EndsAt.ToString() },
                        {"{{url}}",$"{origin}/poll/start/{poll.Id}" }
                    };
                    var body =  EmailBodyBuilder.GenerateEmailBody("PollNotification", placeholders);

                    //send email
                    await _emailSender.SendEmailAsync(user.Email!, $"Survey Basket: New Poll - {poll.Title} ", body);
                }
            }

        }
    }
}
