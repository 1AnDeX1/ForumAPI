using WebApp.BusinessLogic.Models.PostModels;
using WebApp.BusinessLogic.Models.PostReplyModels;
using WebApp.DataAccess.Entities;

namespace WebApp.BusinessLogic.IServices;

/// <summary>
/// Defines the contract for post-related services in the forum.
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Retrieves all posts associated with a specific thread asynchronously.
    /// </summary>
    Task<(IEnumerable<PostModel> posts, int postsCount)> GetPostsByThreadIdAsync(int threadId, int page, int pageSize);

    /// <summary>
    /// Adds a new post to a specific thread asynchronously.
    /// </summary>
    Task<PostModel> AddPostAsync(int threadId, string userId, PostCreateModel postModel);

    /// <summary>
    /// Retrieves all replies associated with a specific post asynchronously.
    /// </summary>
    Task<IEnumerable<PostReplyModel>> GetRepliesByPostIdAsync(int postId);

    /// <summary>
    /// Adds a new reply to a specific post asynchronously.
    /// </summary>
    Task<PostReplyModel> AddReplyAsync(int postId, string userId, PostReplyCreateModel replyModel);

    /// <summary>
    /// Updates an existing post asynchronously.
    /// </summary>
    Task<PostModel> UpdatePostAsync(int postId, string userId, PostCreateModel postModel);

    /// <summary>
    /// Deletes a post by its identifier asynchronously.
    /// </summary>
    Task DeletePostAsync(int postId);

    /// <summary>
    /// Updates an existing reply asynchronously.
    /// </summary>
    Task<PostReplyModel> UpdateReplyAsync(int replyId, string userId, PostReplyCreateModel replyModel);

    /// <summary>
    /// Deletes a reply by its identifier asynchronously.
    /// </summary>
    Task DeleteReplyAsync(int replyId);

    /// <summary>
    /// Determines whether a user can modify a specific post asynchronously.
    /// </summary>
    Task<bool> CanUserModifyPost(ApplicationUser user, int postId);

    /// <summary>
    /// Determines whether a user can modify a specific reply asynchronously.
    /// </summary>
    Task<bool> CanUserModifyReply(ApplicationUser user, int replyId);
}
