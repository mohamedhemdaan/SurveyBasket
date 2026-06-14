namespace SurveyBasket.Api.Contracts.Authorization
{
    public record RefreshTokenRequest(
        string Token,
        string RefreshToken
        );
    
}
