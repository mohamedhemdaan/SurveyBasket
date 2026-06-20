namespace SurveyBasket.Api.Errors
{
    public class UserErrors
    {
        public static readonly Error InvalidCredentials =
            new Error("User.InvalidCredentials", "Invalid Email / Password",StatusCodes.Status401Unauthorized);
        
        public static readonly Error InvalidJwtToken = 
            new Error("User.InvalidJwtToken", "Invalid Jwt Token", StatusCodes.Status401Unauthorized);
        
        public static readonly Error InvalidRefreshToken =
            new Error("User.InvalidRefreshToken", "Invalid refresh Token", StatusCodes.Status401Unauthorized);

        public static readonly Error DuplicatedEmail =
            new Error("User.DuplicatedEmail", "Another user with the same email is already exists", StatusCodes.Status409Conflict);

        public static readonly Error EmailNotConfirmed =
            new Error("User.EmailNotConfirmed", "Email Not Confirmed", StatusCodes.Status401Unauthorized);

        public static readonly Error InvalidCode =
            new Error("User.InvalidCode", "Invalid Code", StatusCodes.Status401Unauthorized);

        public static readonly Error DuplicatedConfirmation =
            new Error("User.DuplicatedConfirmation", "Email already Confirmed", StatusCodes.Status400BadRequest);

    }

}
