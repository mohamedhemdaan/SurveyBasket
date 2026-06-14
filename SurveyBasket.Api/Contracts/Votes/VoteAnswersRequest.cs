namespace SurveyBasket.Api.Contracts.Votes
{
    public record VoteAnswersRequest(
        int QuestionId,
        int AnswerId
        );
}
