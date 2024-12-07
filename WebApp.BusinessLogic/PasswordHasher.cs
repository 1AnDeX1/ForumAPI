using WebApp.BusinessLogic.IServices;

namespace WebApp.BusinessLogic
{
    /// <summary>
    /// Provides methods for hashing passwords and verifying hashed passwords.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Generates a hashed version of the specified password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A hashed password string.</returns>
        public string Generate(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password);

        /// <summary>
        /// Verifies a password against a given hashed password.
        /// </summary>
        /// <param name="password">The plaintext password to verify.</param>
        /// <param name="hashedPassword">The hashed password to compare against.</param>
        /// <returns><c>true</c> if the password matches the hashed password; otherwise, <c>false</c>.</returns>
        public bool VerifyPassword(string password, string hashedPassword) =>
            BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    }
}
