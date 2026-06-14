namespace SurveyBasket.Api.Contracts.Questions
{
    public class QuestionsRequestValidator : AbstractValidator<QuestionsRequest>
    {
        public QuestionsRequestValidator()
        {
            RuleFor(Q => Q.Content)
                .NotEmpty()
                .Length(3, 1000);

            RuleFor(Q => Q.Answers)
                .NotNull();

            RuleFor(Q => Q.Answers)
                .Must(A => A.Count > 1)
                .WithMessage("Questions should has at least 2 answers")
                .When(Q=>Q.Answers != null);

            RuleFor(Q => Q.Answers)
                .Must(A => A.Distinct().Count() == A.Count)
                .WithMessage("You can't Add dublicated answers to the same Question")
                .When(Q => Q.Answers != null);
        }
    }
}
