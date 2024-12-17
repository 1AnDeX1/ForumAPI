using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.IRepository;
public interface IUserRepository
{
    Task<(IEnumerable<ApplicationUser> users, int usersCount)> GetUsersAsync(int page, int pageSize);

    Task<(IEnumerable<ApplicationUser> users, int usersCount)> GetUsersByNameAsync(string username, int page, int pageSize);

    Task<ApplicationUser> GetByIdAsync(string id);

    Task<ApplicationUser> UpdateAsync(ApplicationUser user);

    Task DeleteAsync(string id);
}
