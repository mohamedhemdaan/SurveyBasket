namespace SurveyBasket.Api.Contracts.Votes
{
    public class VoteAnswersRequestValidator : AbstractValidator<VoteAnswersRequest>
    {
        public VoteAnswersRequestValidator()
        {
            RuleFor(VAnswers => VAnswers.QuestionId)
                .GreaterThan(0);

            RuleFor(VAnswers => VAnswers.AnswerId)
                .GreaterThan(0);
        }
    }
}
