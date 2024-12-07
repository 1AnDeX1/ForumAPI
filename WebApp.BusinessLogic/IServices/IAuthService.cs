using WebApp.BusinessLogic.Models.UserModels;

namespace WebApp.BusinessLogic.IServices
{
    /// <summary>
    /// Service interface for handling user authentication operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the specified role.
        /// </summary>
        /// <param name="model">The registration data for the new user.</param>
        /// <param name="role">The role to assign to the user.</param>
        /// <returns>
        /// A tuple containing the result code and a message:
        /// <list type="bullet">
        /// <item>
        /// <term>Item1</term>
        /// <description>An integer representing the result code.</description>
        /// </item>
        /// <item>
        /// <term>Item2</term>
        /// <description>A string containing the result message.</description>
        /// </item>
        /// </list>
        /// </returns>
        Task<(int, string)> Registeration(RegistrationModel model, string role);

        /// <summary>
        /// Logs in a user with the specified login credentials.
        /// </summary>
        /// <param name="model">The login data for the user.</param>
        /// <returns>
        /// A tuple containing the result code and a message:
        /// <list type="bullet">
        /// <item>
        /// <term>Item1</term>
        /// <description>An integer representing the result code.</description>
        /// </item>
        /// <item>
        /// <term>Item2</term>
        /// <description>A string containing the result message or authentication token.</description>
        /// </item>
        /// </list>
        /// </returns>
        Task<(int, string, string?)> Login(LoginModel model);
    }
}
