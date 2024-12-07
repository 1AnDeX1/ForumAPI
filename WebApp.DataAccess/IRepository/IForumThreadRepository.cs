using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.IRepository;

/// <summary>
/// Interface for managing forum thread data.
/// </summary>
public interface IForumThreadRepository
{
    /// <summary>
    /// Retrieves all forum threads.
    /// </summary>
    Task<(IEnumerable<ForumThread> threads, int threadsCount)> GetAllAsync(int page, int pageSize);

    /// <summary>
    /// Retrieves all forum threads by title.
    /// </summary>
    Task<(IEnumerable<ForumThread> threads, int threadsCount)> GetAllByTitleAsync(string title, int page, int pageSize);

    /// <summary>
    /// Retrieves a specific forum thread by its identifier.
    /// </summary>
    Task<ForumThread?> GetByIdAsync(int id);

    /// <summary>
    /// Adds a new forum thread to the repository.
    /// </summary>
    Task<ForumThread> AddAsync(ForumThread thread);

    /// <summary>
    /// Updates an existing forum thread in the repository.
    /// </summary>
    Task<ForumThread> UpdateAsync(ForumThread thread);

    /// <summary>
    /// Deletes a forum thread by its identifier.
    /// </summary>
    Task DeleteAsync(int id);
}
