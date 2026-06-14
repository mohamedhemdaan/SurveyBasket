namespace SurveyBasket.Api.Errors
{
    public class VoteErrors
    {
        public static readonly Error DublicatedVote =
            new("Vote.Dublicated", "This User already voted before on this poll",StatusCodes.Status409Conflict);

        public static readonly Error InvalidQuestions =
            new("Vote.InvalidQuestions", "Invalid Operations", StatusCodes.Status400BadRequest);
    }
}
