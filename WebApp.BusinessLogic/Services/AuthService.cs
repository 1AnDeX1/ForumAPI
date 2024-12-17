using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.DataAccess.Entities;

namespace WebApp.BusinessLogic.Services
{
    /// <summary>
    /// Service class that handles authentication operations such as registration and login.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;
        private readonly ILogger<AuthService> logger;
        private readonly IMapper mapper;

        // Define logging delegates
#pragma warning disable SA1204 // Static elements should appear before instance elements
        private static readonly Action<ILogger, string, Exception?> LogWarning =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(0, nameof(LogWarning)),
                "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogError =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(1, nameof(LogError)),
                "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogInformation =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(2, nameof(LogInformation)),
                "{Message}");
#pragma warning restore SA1204 // Static elements should appear before instance elements

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="userManager">The user manager for handling user-related operations.</param>
        /// <param name="roleManager">The role manager for handling role-related operations.</param>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="logger">The logger for logging events and errors.</param>
        /// <param name="mapper">The mapper for object mapping between models.</param>
        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <summary>
        /// Registers a new user with the specified role.
        /// </summary>
        /// <param name="model">The registration data for the new user.</param>
        /// <param name="role">The role to assign to the user.</param>
        /// <returns>A tuple containing a status code and message indicating the result of the registration process.</returns>
        public async Task<(int, string)> Registeration(RegistrationModel model, string role)
        {
            if (model == null)
            {
                LogWarning(this.logger, "User registration attempted with empty model.", null);
                return (0, "User is empty");
            }

            var userExists = await this.userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                LogWarning(this.logger, $"User registration failed: User '{model.UserName}' already exists.", null);
                return (0, "User with this name already exists");
            }

            ApplicationUser user = this.mapper.Map<ApplicationUser>(model);

            var createUserResult = await this.userManager.CreateAsync(user, model.Password);
            if (!createUserResult.Succeeded)
            {
                LogError(this.logger, $"User registration failed: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}", null);
                return (0, $"User registration failed: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
            }

            if (!await this.roleManager.RoleExistsAsync(role))
            {
                _ = await this.roleManager.CreateAsync(new IdentityRole(role));
            }

            _ = await this.userManager.AddToRoleAsync(user, role);
            LogInformation(this.logger, $"User '{model.UserName}' created successfully and assigned to role '{role}'.", null);
            return (1, "User created successfully!");
        }

        /// <summary>
        /// Logs in a user with the specified login credentials.
        /// </summary>
        /// <param name="model">The login data for the user.</param>
        /// <returns>A tuple containing a status code and a token or message indicating the result of the login process.</returns>
        public async Task<(int, string, string?)> Login(LoginModel model)
        {
            if (model == null)
            {
                LogWarning(this.logger, "Login attempted with empty model.", null);
                return (0, "User is empty", null);
            }

            var user = await this.userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                LogWarning(this.logger, $"Login failed: No such username '{model.UserName}'.", null);
                return (0, "No such username", null);
            }

            if (!await this.userManager.CheckPasswordAsync(user, model.Password))
            {
                LogWarning(this.logger, $"Login failed for user '{model.UserName}': Invalid password.", null);
                return (0, "Invalid password", null);
            }

            var userRoles = await this.userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            string token = this.GenerateToken(authClaims);
            LogInformation(this.logger, $"User '{model.UserName}' logged in successfully.", null);
            return (1, token, model.UserName);
        }

        /// <summary>
        /// Generates a JWT token for the specified claims.
        /// </summary>
        /// <param name="claims">The claims to include in the token.</param>
        /// <returns>The generated JWT token as a string.</returns>
        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["JWT:Secret"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = this.configuration["JWT:ValidIssuer"],
                Audience = this.configuration["JWT:ValidAudience"],
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(claims),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
