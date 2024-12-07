using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.IRepository;

/// <summary>
/// Interface for managing post data in the database.
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Retrieves all posts associated with a specific thread.
    /// </summary>
    Task<(IEnumerable<Post> posts, int postsCount)> GetPostsByThreadIdAsync(int threadId, int page, int pageSize);

    /// <summary>
    /// Retrieves all posts associated with a specific thread without pagination.
    /// </summary>
    Task<IEnumerable<Post>> GetPostsByThreadIdNoPaginationAsync(int threadId);

    /// <summary>
    /// Retrieves a specific post by its identifier.
    /// </summary>
    Task<Post?> GetByIdAsync(int id);

    /// <summary>
    /// Adds a new post to the database.
    /// </summary>
    Task<Post> AddAsync(Post post);

    /// <summary>
    /// Updates an existing post in the database.
    /// </summary>
    Task<Post> UpdateAsync(Post post);

    /// <summary>
    /// Deletes a post by its identifier from the database.
    /// </summary>
    Task DeleteAsync(int id);
}
