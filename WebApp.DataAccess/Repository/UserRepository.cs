using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;

namespace WebApp.DataAccess.Repository;
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext context;

    public UserRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<(IEnumerable<ApplicationUser> users, int usersCount)> GetUsersAsync(int page, int pageSize)
    {
        var users = await this.context.Users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var usersCount = await this.context.Users.CountAsync();

        return (users, usersCount);
    }

    public async Task<(IEnumerable<ApplicationUser> users, int usersCount)> GetUsersByNameAsync(string username, int page, int pageSize)
    {
        var users = await this.context.Users
            .Where(t => t.UserName.Contains(username))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var usersCount = await this.context.Users.Where(t => t.UserName.Contains(username)).CountAsync();

        return (users, usersCount);
    }

    public async Task<ApplicationUser> GetByIdAsync(string id)
    {
        return await this.context.Users
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<ApplicationUser> UpdateAsync(ApplicationUser user)
    {
        _ = this.context.Users.Update(user);
        _ = await this.context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(string id)
    {
        var user = await this.context.Users.FindAsync(id);
        if (user != null)
        {
            _ = this.context.Users.Remove(user);
            _ = await this.context.SaveChangesAsync();
        }
    }
}
