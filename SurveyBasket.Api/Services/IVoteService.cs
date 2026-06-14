using SurveyBasket.Api.Contracts.Votes;

namespace SurveyBasket.Api.Services
{
    public interface IVoteService
    {
        public Task<Result> AddAsync(int pollId,string userId,VoteRequest request , CancellationToken cancellationToken = default);
    }
}
