using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeWorkFlowHub.Common;
using EmployeeWorkFlowHub.Common.Helper;
using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Controllers.Api
{
    /// <summary>
    /// RESTful Web API Controller handling user authentication and JWT token generation.
    /// Injects IAuthenticateService via dependency injection.
    /// </summary>
    [Produces(AuthAPIController.InputType.ApplicationJson)]
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthenticateService _authenticateService;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            IAuthenticateService authenticateService,
            ILogger<AuthApiController> logger)
        {
            _authenticateService = authenticateService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates user credentials and issues a signed JWT token with role claims.
        /// </summary>
        /// <param name="request">The login credentials (Username, Password).</param>
        /// <returns>JWT token and user role information on success; 401 on invalid credentials.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _authenticateService.AuthenticateUserAsync(request);
                if (response.StatusCode != 200 || response.ResultData == null)
                {
                    _logger.LogWarning("Authentication failed for user '{Username}'. Message: {Message}", request.Username, response.StatusMessage);
                    return Unauthorized(new { message = response.StatusMessage });
                }

                var loginData = (LoginResponse)response.ResultData;

                // Store in ASP.NET Core session (session state)
                HttpContext.Session.SetString(CommonVariable.SessionField.Token, loginData.Token);
                HttpContext.Session.SetString(CommonVariable.SessionField.UserName, loginData.Username);
                HttpContext.Session.SetString(CommonVariable.SessionField.FullName, loginData.FullName);
                HttpContext.Session.SetString(CommonVariable.SessionField.Role, loginData.Role);
                if (loginData.EmployeeId.HasValue)
                {
                    HttpContext.Session.SetInt32(CommonVariable.SessionField.EmployeeId, loginData.EmployeeId.Value);
                }

                _logger.LogInformation("User '{Username}' authenticated successfully with role '{Role}'.", loginData.Username, loginData.Role);

                return Ok(loginData);
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                _logger.LogError(ex, "Error during authentication for user '{Username}'.", request.Username);
                return StatusCode(500, new { message = "An internal error occurred during authentication." });
            }
        }

        /// <summary>
        /// Validates current JWT token and returns claims of the authenticated user.
        /// </summary>
        /// <returns>User identity and roles.</returns>
        [HttpGet("GetCurrentUser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var response = await _authenticateService.GetCurrentUserAsync(username);
            if (response.StatusCode != 200)
            {
                return Unauthorized(new { message = response.StatusMessage });
            }

            return Ok(response.ResultData);
        }
    }
}
