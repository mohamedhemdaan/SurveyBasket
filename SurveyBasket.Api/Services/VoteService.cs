using Microsoft.AspNetCore.OutputCaching;
using SurveyBasket.Api.Contracts.Votes;
using SurveyBasket.Api.Errors;

namespace SurveyBasket.Api.Services
{
    public class VoteService(ApplicationDbContext dbContext ) : IVoteService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken = default)
        {
            var hasVote = await _dbContext.Votes.AnyAsync(v => v.UserId == userId && v.PollId == pollId, cancellationToken: cancellationToken);

            if (hasVote)
                return Result.Failure(VoteErrors.DublicatedVote);

            var pollIsExists = await _dbContext.Polls
                .AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow)
                                                               && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow),cancellationToken);

            if (!pollIsExists)
                return Result.Failure(PollErrors.PollNotFound);

            var availableQuestionsIds  = await  _dbContext.Questions
                .Where(q => q.PollId == pollId && q.IsActive)
                .Select(q => q.Id)
                .ToListAsync(cancellationToken);

            if (!request.VoteAnswers.Select(VAnswer => VAnswer.QuestionId).SequenceEqual(availableQuestionsIds))
                return Result.Failure(VoteErrors.InvalidQuestions);

            var vote = new Vote()
            {
                PollId = pollId,
                UserId = userId,
                VoteAnswers = request.VoteAnswers.Adapt<IEnumerable<VoteAnswer>>().ToList()
            };

            await _dbContext.Votes.AddAsync(vote, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);


            return Result.Success();
        }
    }
}
