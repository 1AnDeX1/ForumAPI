using WebApp.BusinessLogic.Models.UserModels;

namespace WebApp.BusinessLogic.IServices;
public interface IUserService
{
    Task<(IEnumerable<UserModel> users, int usersCount)> GetAllUsersAsync(string? userName, int page, int pageSize);

    Task<UserModel?> GetUserByIdAsync(string id);

    Task<RegistrationModel> UpdateUserAsync(string id, RegistrationModel registrationModel);

    Task DeleteUserAsync(string id);
}
