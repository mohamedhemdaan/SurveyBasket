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
    }
    
}
