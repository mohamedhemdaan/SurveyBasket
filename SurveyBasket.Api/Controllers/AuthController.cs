using Microsoft.Extensions.Options;
using SurveyBasket.Api.Authentication;

namespace SurveyBasket.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, IOptions<JwtOptions> jwtOptions, ILogger<AuthController> logger ) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;

        [HttpPost("")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("hello from you email : {Email} and your password :{Password}",request.Email,request.Password);

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
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest Request, CancellationToken cancellationToken)
        {
            var result = await _authService.GetRefreshTokenAsync(Request.Token, Request.RefreshToken, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem();
                //Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);

            //return authResult is null ? BadRequest("Invalid Token") : Ok(authResult);
        }
        [HttpPut("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest Request, CancellationToken cancellationToken)
        {
            var result = await _authService.RevokeRefreshTokenAsync(Request.Token, Request.RefreshToken, cancellationToken);
            return result.IsSuccess
                ? Ok()
                : result.ToProblem(); 
            //Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);

            //return isRevoked ? Ok() : BadRequest("Operation failed");
        }


        //[HttpGet]
        //public IActionResult Test()
        //{
        //    return Ok(_jwtOptions.Audience);
        //}





    }



}
