using Microsoft.Extensions.Options;
using SurveyBasket.Api.Authentication;

namespace SurveyBasket.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("hello from you email : {Email} and your password :{Password}", request.Email, request.Password);

            var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);
            return authResult.IsSuccess
                ? Ok(authResult.Value)
                : authResult.ToProblem();
        }
        #region ByOneOf
        //public async Task<IActionResult> LoginAsync([FromBody] LoginRequest Request, CancellationToken cancellationToken)
        //{
        //    var authResult = await _authService.GetTokenAsync(Request.Email, Request.Password, cancellationToken);
        //    return authResult.Match(
        //        authResponse => Ok(authResponse),
        //        error => Problem(statusCode: StatusCodes.Status400BadRequest, title: error.Code, detail: error.Description)
        //        );
        //} 
        #endregion

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest Request, CancellationToken cancellationToken)
        {
            var result = await _authService.GetRefreshTokenAsync(Request.Token, Request.RefreshToken, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
            //Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);

            //return authResult is null ? BadRequest("Invalid Token") : Ok(authResult);
        }
        [HttpPut("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest Request, CancellationToken cancellationToken)
        {
            var result = await _authService.RevokeRefreshTokenAsync(Request.Token, Request.RefreshToken, cancellationToken);
            return result.IsSuccess
                ? Ok()
                : result.ToProblem();
            //Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);

            //return isRevoked ? Ok() : BadRequest("Operation failed");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            return result.IsSuccess
                ? Ok()
                : result.ToProblem();

        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request)
        {
            var result = await _authService.ConfirmEmailAsync(request);

            return result.IsSuccess
                ? Ok()
                : result.ToProblem();
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
        {
            var result = await _authService.ResendConfirmationEmailAsync(request);

            return result.IsSuccess
                ? Ok()
                : result.ToProblem();
        }


    }



}
