using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.ThreadModels;
using WebApp.DataAccess.Entities;

namespace WebApp.WebApi.Controllers
{
    /// <summary>
    /// Provides endpoints for managing forum threads, including creating, retrieving, updating, and deleting threads.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
#pragma warning disable SA1404 // Code analysis suppression should have justification
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
#pragma warning restore SA1404 // Code analysis suppression should have justification
    public class ForumThreadsController : ControllerBase
    {
        private readonly IForumThreadService forumThreadService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<ForumThreadsController> logger;

#pragma warning disable SA1204 // Static elements should appear before instance elements
        private static readonly Action<ILogger, string, Exception?> LogInfo =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(LogInfo)), "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogWarning =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(2, nameof(LogWarning)), "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogError =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(3, nameof(LogError)), "{Message}");
#pragma warning restore SA1204 // Static elements should appear before instance elements

        /// <summary>
        /// Initializes a new instance of the <see cref="ForumThreadsController"/> class.
        /// </summary>
        /// <param name="forumThreadService">The service for managing forum threads.</param>
        /// <param name="userManager">The user manager for handling user-related operations.</param>
        /// <param name="logger">The logger for logging thread-related information and errors.</param>
        public ForumThreadsController(
            IForumThreadService forumThreadService,
            UserManager<ApplicationUser> userManager,
            ILogger<ForumThreadsController> logger)
        {
            this.forumThreadService = forumThreadService;
            this.userManager = userManager;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all forum threads.
        /// </summary>
        /// <param name="title">Value of Forum thread.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="ForumThreadModel"/>.</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<(IEnumerable<ForumThreadModel> thread, int threadsCount)>> GetAllThreads(string? title, int page = 1, int pageSize = 20)
        {
            try
            {
                var threadsObject = await this.forumThreadService.GetAllThreadsAsync(title, page, pageSize);
                if (threadsObject.threads == null)
                {
                    LogWarning(this.logger, "No threads found.", null);
                    return this.NotFound();
                }

                LogInfo(this.logger, "Retrieved all forum threads successfully.", null);
                return this.Ok(new { threadsObject.threads, threadsObject.threadsCount });
            }
            catch (Exception ex)
            {
                LogError(this.logger, "Error retrieving threads: {Message}", ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving threads.");
            }
        }

        /// <summary>
        /// Retrieves a forum thread by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the thread.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ForumThreadModel"/> if found, otherwise a NotFound result.</returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ForumThreadModel>> GetThreadById(int id)
        {
            try
            {
                var thread = await this.forumThreadService.GetThreadByIdAsync(id);
                if (thread == null)
                {
                    LogWarning(this.logger, "Thread with ID {Id} not found.", null);
                    return this.NotFound();
                }

                LogInfo(this.logger, "Retrieved thread with ID {Id} successfully.", null);
                return this.Ok(thread);
            }
            catch (AppException ex)
            {
                LogWarning(this.logger, "AppException: {Message}", ex);
                return this.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                LogError(this.logger, "Error retrieving thread with ID {Id}: {Message}", ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the thread.");
            }
        }

        /// <summary>
        /// Creates a new forum thread.
        /// </summary>
        /// <param name="threadCreateModel">The model containing the details of the thread to create.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="ForumThreadModel"/>.</returns>
        [HttpPost]
        public async Task<ActionResult<ForumThreadModel>> CreateThread([FromBody] ForumThreadCreateModel threadCreateModel)
        {
            if (!this.ModelState.IsValid)
            {
                LogWarning(this.logger, "Invalid thread creation payload.", null);
                return this.BadRequest(this.ModelState);
            }

            var currentUser = await this.userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                LogWarning(this.logger, "User not found during thread creation.", null);
                return this.Unauthorized("User not found.");
            }

            try
            {
                var createdThread = await this.forumThreadService.CreateThreadAsync(threadCreateModel, currentUser.Id);
                LogInfo(this.logger, "Thread created successfully by user {UserId}.", null);
                return this.CreatedAtAction(nameof(this.GetThreadById), new { id = createdThread.Id }, createdThread);
            }
            catch (AppException ex)
            {
                LogWarning(this.logger, "AppException during thread creation: {Message}", ex);
                return this.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                LogError(this.logger, "Error creating thread: {Message}", ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing forum thread.
        /// </summary>
        /// <param name="id">The identifier of the thread to update.</param>
        /// <param name="threadModel">The model containing the updated details of the thread.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> indicating the result of the update operation.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateThread(int id, [FromBody] ForumThreadCreateModel threadModel)
        {
            if (!this.ModelState.IsValid)
            {
                LogWarning(this.logger, "Invalid thread update payload for ID {Id}.", null);
                return this.BadRequest(this.ModelState);
            }

            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (!await this.forumThreadService.CanUserModifyThread(currentUser, id))
            {
                LogWarning(this.logger, "User {UserId} attempted to modify a thread they do not own: {ThreadId}.", null);
                return this.Forbid();
            }

            try
            {
                _ = await this.forumThreadService.UpdateThreadAsync(id, threadModel);
                LogInfo(this.logger, "Thread with ID {Id} updated successfully by user {UserId}.", null);
                return this.NoContent();
            }
            catch (AppException ex)
            {
                LogWarning(this.logger, "AppException during thread update: {Message}", ex);
                return this.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                LogError(this.logger, "Error updating thread with ID {Id}: {Message}", ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a forum thread by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the thread to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> indicating the result of the delete operation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteThread(int id)
        {
            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (!await this.forumThreadService.CanUserModifyThread(currentUser, id))
            {
                LogWarning(this.logger, "User {UserId} attempted to delete a thread they do not own: {ThreadId}.", null);
                return this.Forbid();
            }

            try
            {
                await this.forumThreadService.DeleteThreadAsync(id);
                LogInfo(this.logger, "Thread with ID {Id} deleted successfully by user {UserId}.", null);
                return this.NoContent();
            }
            catch (AppException ex)
            {
                LogWarning(this.logger, "AppException during thread deletion: {Message}", ex);
                return this.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                LogError(this.logger, "Error deleting thread with ID {Id}: {Message}", ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
