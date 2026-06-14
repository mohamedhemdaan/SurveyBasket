namespace SurveyBasket.Api.Errors
{
    public class PollErrors
    {
    
        public static readonly Error PollNotFound = new Error("Poll.NotFound", "No poll was found with given Id",StatusCodes.Status404NotFound);
        public static readonly Error DuplicatedPollTitle = 
            new Error("Poll.DuplicatedTitle", "Another poll with the same title already existed",StatusCodes.Status409Conflict);
    }
}
