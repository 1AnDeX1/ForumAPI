using WebApp.BusinessLogic.Models.ThreadModels;
using WebApp.DataAccess.Entities;

namespace WebApp.BusinessLogic.IServices
{
    /// <summary>
    /// Defines the contract for forum thread services.
    /// </summary>
    public interface IForumThreadService
    {
        /// <summary>
        /// Retrieves all forum threads by title.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a collection of <see cref="ForumThreadModel"/>.</returns>
        Task<(IEnumerable<ForumThreadModel> threads, int threadsCount)> GetAllThreadsAsync(string? title, int page, int pageSize);

        /// <summary>
        /// Retrieves a specific forum thread by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the forum thread.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="ForumThreadModel"/> if found; otherwise, <c>null</c>.</returns>
        Task<ForumThreadModel?> GetThreadByIdAsync(int id);

        /// <summary>
        /// Creates a new forum thread.
        /// </summary>
        /// <param name="threadCreateModel">The model containing the information for the new thread.</param>
        /// <param name="userId">The identifier of the user creating the thread.</param>
        /// <returns>A task that represents the asynchronous operation, containing the created <see cref="ForumThreadModel"/>.</returns>
        Task<ForumThreadModel> CreateThreadAsync(ForumThreadCreateModel threadCreateModel, string userId);

        /// <summary>
        /// Updates an existing forum thread.
        /// </summary>
        /// <param name="id">The identifier of the thread to update.</param>
        /// <param name="threadModel">The model containing the updated information for the thread.</param>
        /// <returns>A task that represents the asynchronous operation, containing the updated <see cref="ForumThreadModel"/>.</returns>
        Task<ForumThreadModel> UpdateThreadAsync(int id, ForumThreadCreateModel threadModel);

        /// <summary>
        /// Deletes a forum thread by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the forum thread to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteThreadAsync(int id);

        /// <summary>
        /// Determines whether a user can modify a specific forum thread.
        /// </summary>
        /// <param name="user">The user attempting to modify the thread.</param>
        /// <param name="threadId">The identifier of the thread to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing <c>true</c> if the user can modify the thread; otherwise, <c>false</c>.</returns>
        Task<bool> CanUserModifyThread(ApplicationUser user, int threadId);
    }
}
