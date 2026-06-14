using SurveyBasket.Api.Contracts.Results;
using SurveyBasket.Api.Errors;

namespace SurveyBasket.Api.Services
{
    public class ResultService(ApplicationDbContext dbContext) : IResultService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
        {

           var pollVotes = await  _dbContext.Polls
                .Where(p => p.Id == pollId)
                .Select(p => new PollVotesResponse(
                    p.Title,
                    p.Votes.Select(v => new VoteResponse(
                        $"{v.User.FirstName} {v.User.LastName}",
                        v.SubmittedOn,
                        v.VoteAnswers.Select(VA => new QuestionAnswerResponse(
                            VA.Question.Content,
                            VA.Answer.Content
                        ))
                    ))
                 ))
                .SingleOrDefaultAsync(cancellationToken);

            return pollVotes is null 
                ? Result.Failure<PollVotesResponse>(PollErrors.PollNotFound) 
                : Result.Success(pollVotes);
        }
   
        public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default )
        {
            var pollIsExists =  await _dbContext.Polls.AnyAsync(v => v.Id == pollId);
            if (!pollIsExists)
                return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);

            var votesPerDay = await _dbContext.Votes
                .Where(v => v.PollId == pollId)
                .GroupBy(v => new { Date = DateOnly.FromDateTime(v.SubmittedOn) })
                .Select(g => new VotesPerDayResponse(
                    g.Key.Date,
                    g.Count()
                ))
                .ToListAsync(cancellationToken: cancellationToken);

            return Result.Success<IEnumerable<VotesPerDayResponse>>(votesPerDay);
        }

        public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default)
        {
            var pollIsExists = await _dbContext.Polls.AnyAsync(p => p.Id == pollId, cancellationToken: cancellationToken);
            if (!pollIsExists)
                return Result.Failure<IEnumerable<VotesPerQuestionResponse>>(PollErrors.PollNotFound);

            var votesPerQuestion = await _dbContext.VoteAnswers
                .Where(va => va.Vote.PollId == pollId)
                .Select(va => new VotesPerQuestionResponse(
                    va.Question.Content,
                    va.Question.VoteAnswers
                        .GroupBy(va => new { AnswerId = va.Answer.Id, AnswerContent = va.Answer.Content })
                        .Select(g => new VotesPerAnswerResponse(
                            g.Key.AnswerContent,
                            g.Count()
                        ))
                ))
                .ToListAsync(cancellationToken: cancellationToken);

            return Result.Success<IEnumerable<VotesPerQuestionResponse>>(votesPerQuestion);
        }
    }
}
