using SurveyBasket.Api.Contracts.Answers;
using SurveyBasket.Api.Contracts.Questions;

namespace SurveyBasket.Api.Mapping
{
    public class MappingConfigurations : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            //config.NewConfig<QuestionsRequest, Question>()
            //    .Ignore(nameof(Question.Answers));

            config.NewConfig<QuestionsRequest, Question>()
                .Map(dest => dest.Answers, src => src.Answers.Select(AnswerContent => new Answer { Content = AnswerContent }));

            config.NewConfig<Question, QuestionResponse>()
                .Map(dest => dest.Answers, src => src.Answers.Where(a => a.IsActive)
                .Select(a => new AnswerResponse(a.Id, a.Content)));
        }
    }
}
