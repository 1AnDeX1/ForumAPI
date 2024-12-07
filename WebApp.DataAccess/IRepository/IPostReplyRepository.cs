using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.IRepository;

/// <summary>
/// Interface for managing post replies in the database.
/// </summary>
public interface IPostReplyRepository
{
    /// <summary>
    /// Retrieves all replies associated with a specific post.
    /// </summary>
    Task<IEnumerable<PostReply>> GetRepliesByPostIdAsync(int postId);

    /// <summary>
    /// Retrieves a specific post reply by its identifier.
    /// </summary>
    Task<PostReply?> GetByIdAsync(int id);

    /// <summary>
    /// Adds a new reply to a post in the database.
    /// </summary>
    Task<PostReply> AddAsync(PostReply reply);

    /// <summary>
    /// Updates an existing post reply in the database.
    /// </summary>
    Task<PostReply> UpdateAsync(PostReply reply);

    /// <summary>
    /// Deletes a post reply by its identifier from the database.
    /// </summary>
    Task DeleteAsync(int id);
}
