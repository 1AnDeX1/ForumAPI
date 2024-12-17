using AutoMapper;
using Microsoft.AspNetCore.Identity;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;

namespace WebApp.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public UserService(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            this.userRepository = userRepository;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        /// <summary>
        /// Deletes a user asynchronously by ID.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteUserAsync(string id)
        {
            var user = await this.userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            await this.userRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Retrieves all users with optional filtering by username, paginated.
        /// </summary>
        /// <param name="userName">Optional username filter.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Tuple containing users and total user count.</returns>
        public async Task<(IEnumerable<UserModel> users, int usersCount)> GetAllUsersAsync(string? userName, int page, int pageSize)
        {
            var result = string.IsNullOrEmpty(userName)
                ? await this.userRepository.GetUsersAsync(page, pageSize)
                : await this.userRepository.GetUsersByNameAsync(userName, page, pageSize);

            var users = this.mapper.Map<IEnumerable<UserModel>>(result.users);
            return (users, result.usersCount);
        }

        /// <summary>
        /// Retrieves a user by ID asynchronously.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The user model if found; otherwise, null.</returns>
        public async Task<UserModel?> GetUserByIdAsync(string id)
        {
            var user = await this.userRepository.GetByIdAsync(id);
            return user != null ? this.mapper.Map<UserModel>(user) : null;
        }

        /// <summary>
        /// Updates a user's information asynchronously.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <param name="registrationModel">The updated user information.</param>
        /// <returns>The updated user model.</returns>
        public async Task<RegistrationModel> UpdateUserAsync(string id, RegistrationModel registrationModel)
        {
            var user = await this.userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            // Map non-password fields
            _ = this.mapper.Map(registrationModel, user);

            // Update password if provided
            if (!string.IsNullOrEmpty(registrationModel?.Password))
            {
                var token = await this.userManager.GeneratePasswordResetTokenAsync(user);
                var result = await this.userManager.ResetPasswordAsync(user, token, registrationModel.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new AppException($"Failed to update password: {errors}");
                }
            }

            var updatedUser = await this.userRepository.UpdateAsync(user);

            return this.mapper.Map<RegistrationModel>(updatedUser);
        }
    }
}
