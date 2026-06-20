using Azure.Core;
using Hangfire;
using SurveyBasket.Api.Errors;

namespace SurveyBasket.Api.Services
{
    public class PollService(ApplicationDbContext context,INotificationService notificationService) : IPollService
    {
        private readonly ApplicationDbContext _dbContext = context;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<IEnumerable<PollResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
           await _dbContext.Polls
            .AsNoTracking()
            //.Select(p => new PollResponse(p.Id,p.Title,p.Summary,p.IsPublished,p.StartsAt,p.EndsAt))
            .ProjectToType<PollResponse>()
            .ToListAsync();

        public async Task<Result<PollResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _dbContext.Polls.FindAsync(id, cancellationToken);
            return poll is null
                ? Result.Failure<PollResponse>((PollErrors.PollNotFound))
                : Result.Success(poll.Adapt<PollResponse>());
        }

        public async Task<Result<PollResponse>> AddAsync(PollRequest request, CancellationToken cancellationToken = default)
        {
            var existingPoll = await _dbContext.Polls.AnyAsync(p => p.Title == request.Title, cancellationToken: cancellationToken);
            if (existingPoll)
                return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTitle);
            var poll = request.Adapt<Poll>();

            await _dbContext.Polls.AddAsync(poll, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(poll.Adapt<PollResponse>());
        }

        public async Task<Result> UpdateAsync(int id, PollRequest request, CancellationToken cancellationToken = default)
        {

            var existingPoll = await _dbContext.Polls.AnyAsync(p => p.Title == request.Title && p.Id != id, cancellationToken: cancellationToken);
            if (existingPoll)
                return Result.Failure(PollErrors.DuplicatedPollTitle);

            var currentPoll = await _dbContext.Polls.FindAsync(id, cancellationToken);

            if (currentPoll is null)
                return Result.Failure(PollErrors.PollNotFound);

            currentPoll.Title = request.Title;
            currentPoll.Summary = request.Summary;
            currentPoll.StartsAt = request.StartsAt;
            currentPoll.EndsAt = request.EndsAt;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _dbContext.Polls.FindAsync(id, cancellationToken);

            if (poll is null)
                return Result.Failure(PollErrors.PollNotFound);

            _dbContext.Polls.Remove(poll);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _dbContext.Polls.FindAsync(id, cancellationToken);

            if (poll is null)
                return Result.Failure(PollErrors.PollNotFound);

            poll.IsPublished = !poll.IsPublished;

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (poll.IsPublished && poll.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
                BackgroundJob.Enqueue(() => _notificationService.SendNewPollsNotification(poll.Id));

            return Result.Success();


        }

        public async Task<IEnumerable<PollResponse>> GetCurrentAsync(CancellationToken cancellationToken = default) =>
         await _dbContext.Polls.
                Where(p => p.IsPublished && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                                         && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow))
                .AsNoTracking()
                .ProjectToType<PollResponse>()
                .ToListAsync();

    }
}
