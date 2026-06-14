namespace SurveyBasket.Api.Errors
{
    public class QuestionErrors
    {
        public static readonly Error QuestionNotFound = 
            new Error("Question.NotFound", "No Question was found with given Id", StatusCodes.Status404NotFound);

        public static readonly Error DuplicatedQuestionContent =
            new Error("Question.DuplicatedContent", "Another Question with the same Content already existed", StatusCodes.Status409Conflict);


    }
}
