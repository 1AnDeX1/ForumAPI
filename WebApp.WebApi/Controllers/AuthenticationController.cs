using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.DataAccess.Entities;

namespace WebApp.WebApi.Controllers
{
    /// <summary>
    /// Provides endpoints for user authentication, including login and registration.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
#pragma warning disable SA1404 // Code analysis suppression should have justification
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
#pragma warning restore SA1404 // Code analysis suppression should have justification
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly ILogger<AuthenticationController> logger;

#pragma warning disable SA1204 // Static elements should appear before instance elements
        private static readonly Action<ILogger, string, Exception?> LogInfo =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(LogInfo)), "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogWarning =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(2, nameof(LogWarning)), "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogError =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(3, nameof(LogError)), "{Message}");
#pragma warning restore SA1204 // Static elements should appear before instance elements

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service for handling login and registration.</param>
        /// <param name="logger">The logger for logging authentication-related information and errors.</param>
        public AuthenticationController(IAuthService authService, ILogger<AuthenticationController> logger)
        {
            this.authService = authService;
            this.logger = logger;
        }

        /// <summary>
        /// Authenticates a user and returns a token if successful.
        /// </summary>
        /// <param name="model">The login model containing the user's credentials.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> indicating the result of the login attempt.</returns>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    LogWarning(this.logger, "Invalid payload in login attempt.", null);
                    return this.BadRequest("Invalid payload");
                }

                var (status, token, userName) = await this.authService.Login(model);
                if (status == 0)
                {
                    LogWarning(this.logger, $"Login failed: {token}", null);
                    return this.BadRequest(token);
                }

                LogInfo(this.logger, "User logged in successfully: {Message}", null);
                return this.Ok(new { token, userName });
            }
            catch (Exception ex)
            {
                LogError(this.logger, "An error occurred during login: {Message}", ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="model">The registration model containing the user's registration information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> indicating the result of the registration attempt.</returns>
        [HttpPost]
        [Route("registration")]
        public async Task<IActionResult> Register(RegistrationModel model)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    LogWarning(this.logger, "Invalid payload in registration attempt.", null);
                    return this.BadRequest("Invalid payload");
                }

                var (status, token) = await this.authService.Registeration(model, UserRoles.User);
                if (status == 0)
                {
                    LogWarning(this.logger, $"Registration failed: {token}", null);
                    return this.BadRequest(token);
                }

                LogInfo(this.logger, "User registered successfully: {Message}", null);
                return this.CreatedAtAction(nameof(this.Register), new { succeeded = true, message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                LogError(this.logger, "An error occurred during registration: {Message}", ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, new { succeeded = false, message = ex.Message });
            }
        }
    }
}
