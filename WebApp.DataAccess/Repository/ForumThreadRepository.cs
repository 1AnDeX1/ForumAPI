using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;

namespace WebApp.DataAccess.Repository;

/// <summary>
/// Repository for managing forum thread data in the database.
/// </summary>
public class ForumThreadRepository : IForumThreadRepository
{
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForumThreadRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for accessing forum threads.</param>
    public ForumThreadRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Retrieves all forum threads with their associated posts.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <returns>
    /// An asynchronous operation that returns an <see cref="IEnumerable{ForumThread}"/> containing all threads.
    /// </returns>
    public async Task<(IEnumerable<ForumThread> threads, int threadsCount)> GetAllAsync(int page, int pageSize)
    {
        var threads = await this.context.Threads
            .Include(t => t.Posts)
            .Include(t => t.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var threadsCount = await this.context.Threads.CountAsync();

        return (threads, threadsCount);
    }

    /// <summary>
    /// Retrieves all forum threads by title with their associated posts.
    /// </summary>
    /// <param name="title">Value of Forum thread.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <returns>
    /// An asynchronous operation that returns an <see cref="IEnumerable{ForumThread}"/> containing all threads.
    /// </returns>
    public async Task<(IEnumerable<ForumThread> threads, int threadsCount)> GetAllByTitleAsync(string title, int page, int pageSize)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var threads = await this.context.Threads
            .Where(t => t.Title.Contains(title))
            .Include(t => t.Posts)
            .Include(t => t.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var threadsCount = await this.context.Threads.Where(t => t.Title.Contains(title)).CountAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        return (threads, threadsCount);
    }

    /// <summary>
    /// Retrieves a specific forum thread by its identifier, including its posts.
    /// </summary>
    /// <param name="id">The unique identifier of the thread.</param>
    /// <returns>
    /// An asynchronous operation that returns the <see cref="ForumThread"/> with the specified identifier,
    /// or <c>null</c> if no matching thread is found.
    /// </returns>
    public async Task<ForumThread?> GetByIdAsync(int id)
    {
        return await this.context.Threads
            .Include(t => t.Posts)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// Adds a new forum thread to the database.
    /// </summary>
    /// <param name="thread">The <see cref="ForumThread"/> to add.</param>
    /// <returns>
    /// An asynchronous operation that returns the added <see cref="ForumThread"/>.
    /// </returns>
    public async Task<ForumThread> AddAsync(ForumThread thread)
    {
        _ = await this.context.Threads.AddAsync(thread);
        _ = await this.context.SaveChangesAsync();
        return thread;
    }

    /// <summary>
    /// Updates an existing forum thread in the database.
    /// </summary>
    /// <param name="thread">The <see cref="ForumThread"/> containing updated data.</param>
    /// <returns>
    /// An asynchronous operation that returns the updated <see cref="ForumThread"/>.
    /// </returns>
    public async Task<ForumThread> UpdateAsync(ForumThread thread)
    {
        _ = this.context.Threads.Update(thread);
        _ = await this.context.SaveChangesAsync();
        return thread;
    }

    /// <summary>
    /// Deletes a forum thread by its identifier from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the thread to delete.</param>
    /// <returns>An asynchronous operation.</returns>
    public async Task DeleteAsync(int id)
    {
        var thread = await this.context.Threads.FindAsync(id);
        if (thread != null)
        {
            _ = this.context.Threads.Remove(thread);
            _ = await this.context.SaveChangesAsync();
        }
    }
}
