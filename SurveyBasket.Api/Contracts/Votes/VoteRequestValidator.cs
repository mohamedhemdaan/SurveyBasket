namespace SurveyBasket.Api.Contracts.Votes
{
    public class VoteRequestValidator : AbstractValidator<VoteRequest>
    {
        public VoteRequestValidator()
        {
            RuleFor(v => v.VoteAnswers)
                .NotEmpty();

            //To Apply the validations  (VoteAnswersRequestValidator) of child (VoteAnswersRequest)
            RuleForEach(v => v.VoteAnswers)
                .SetInheritanceValidator(v =>
                    v.Add(new VoteAnswersRequestValidator())
                );
        }
    }
}
