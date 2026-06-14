namespace SurveyBasket.Api.Abstractions
{
    public static class ResultExtensions
    {
        public static ObjectResult ToProblem(this Result result)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException("Canont convert success result to problem");

            var problem = Results.Problem(statusCode: result.Error.StatusCode);
            var problemDetails = problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails; //Reflection
            
            problemDetails!.Extensions["errors"] = new[]
            {
               new { result.Error.Code,result.Error.Description }
            };

            ///problemDetails!.Extensions = new Dictionary<string, object?>()
            ///{
            ///    {
            ///        "errors",
            ///        new[]
            ///        {
            ///            new {result.Error.Code,result.Error.Description}
            ///        }
            ///    }
            ///};
            

            return new ObjectResult(problemDetails);
        }
    }
}
