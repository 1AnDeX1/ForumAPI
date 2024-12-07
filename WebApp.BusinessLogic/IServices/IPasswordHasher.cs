namespace WebApp.BusinessLogic.IServices
{
    /// <summary>
    /// Interface for password hashing functionality.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Generates a hash for the specified password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password as a string.</returns>
        string Generate(string password);

        /// <summary>
        /// Verifies a password against its hashed version.
        /// </summary>
        /// <param name="password">The plaintext password to verify.</param>
        /// <param name="hashedPassword">The hashed password to compare against.</param>
        /// <returns><c>true</c> if the password matches the hashed password; otherwise, <c>false</c>.</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}
