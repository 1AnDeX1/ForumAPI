using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.BusinessLogic.Models.PostReplyModels;
using WebApp.DataAccess.Entities;

namespace WebApp.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/threads/{threadId}/posts")]
#pragma warning disable SA1404 // Code analysis suppression should have justification
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
#pragma warning restore SA1404 // Code analysis suppression should have justification

    /// <summary>
    /// Controller for managing posts in a forum thread.
    /// </summary>
    public class PostsController : ControllerBase
    {
        private readonly IPostService postService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<PostsController> logger;

#pragma warning disable SA1204 // Static elements should appear before instance elements
        private static readonly Action<ILogger, string, Exception?> LogInfo =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(LogInfo)), "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogWarning =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(2, nameof(LogWarning)), "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogError =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(3, nameof(LogError)), "{Message}");
#pragma warning restore SA1204 // Static elements should appear before instance elements

        /// <summary>
        /// Initializes a new instance of the <see cref="PostsController"/> class.
        /// </summary>
        /// <param name="postService">The service for managing posts.</param>
        /// <param name="userManager">The user manager for retrieving user information.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
        public PostsController(
            IPostService postService,
            UserManager<ApplicationUser> userManager,
            ILogger<PostsController> logger)
        {
            this.postService = postService;
            this.userManager = userManager;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the list of posts for a specified thread.
        /// </summary>
        /// <param name="threadId">The ID of the thread.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A list of posts for the specified thread.</returns>
        // GET: api/threads/{threadId}/posts
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<(IEnumerable<PostModel> posts, int postsCount)>> GetPosts(int threadId, int page = 1, int pageSize = 10)
        {
            try
            {
                LogInfo(this.logger, $"Getting posts for thread ID: {threadId}", null);
                var postsObject = await this.postService.GetPostsByThreadIdAsync(threadId, page, pageSize);
                if (postsObject.posts == null)
                {
                    LogWarning(this.logger, $"No posts found for thread ID: {threadId}", null);
                    return this.NotFound();
                }

                LogInfo(this.logger, $"Retrieved {postsObject.posts.Count()} posts for thread ID: {threadId}", null);
                return this.Ok(new { postsObject.posts, postsObject.postsCount });
            }
            catch (AppException ex)
            {
                LogError(this.logger, $"AppException: {ex.Message}", ex);
                return this.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                LogError(this.logger, $"Internal server error: {ex.Message}", ex);
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new post to a specified thread.
        /// </summary>
        /// <param name="threadId">The ID of the thread to which the post is being added.</param>
        /// <param name="postModel">The model containing the data for the new post.</param>
        /// <returns>The created post.</returns>
        // POST: api/threads/{threadId}/posts
        [HttpPost]
        public async Task<ActionResult<PostModel>> AddPost(int threadId, [FromBody] PostCreateModel postModel)
        {
            if (!this.ModelState.IsValid)
            {
                LogWarning(this.logger, $"ModelState is invalid when adding post to thread ID: {threadId}", null);
                return this.BadRequest(this.ModelState);
            }

            var currentUser = await this.userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                LogWarning(this.logger, "User not found while adding post", null);
                return this.Unauthorized("User not found.");
            }

            try
            {
                LogInfo(this.logger, $"Adding new post to thread ID: {threadId} by user ID: {currentUser.Id}", null);
                var createdPost = await this.postService.AddPostAsync(threadId, currentUser.Id, postModel);
                if (createdPost == null)
                {
                    LogWarning(this.logger, "Failed to add post.", null);
                    return this.BadRequest("Failed to add post.");
                }

                LogInfo(this.logger, $"Post created with ID: {createdPost.Id}", null);
                return this.CreatedAtAction(nameof(this.GetPosts), new { threadId }, createdPost);
            }
            catch (AppException ex)
            {
                LogError(this.logger, $"AppException: {ex.Message}", ex);
                return this.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                LogError(this.logger, $"Internal server error: {ex.Message}", ex);
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing post.
        /// </summary>
        /// <param name="postId">The ID of the post to update.</param>
        /// <param name="postModel">The model containing the updated data for the post.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        // PUT: api/threads/{threadId}/posts/{postId}
        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] PostCreateModel postModel)
        {
            if (!this.ModelState.IsValid)
            {
                LogWarning(this.logger, $"ModelState is invalid when updating post ID: {postId}", null);
                return this.BadRequest(this.ModelState);
            }

            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (!await this.postService.CanUserModifyPost(currentUser, postId))
            {
                LogWarning(this.logger, $"User {currentUser.Id} is unauthorized to modify post ID: {postId}", null);
                return this.Forbid();
            }

            try
            {
                LogInfo(this.logger, $"Updating post with ID: {postId}", null);
                _ = await this.postService.UpdatePostAsync(postId, currentUser.Id, postModel);
                LogInfo(this.logger, $"Post with ID: {postId} updated successfully.", null);
                return this.NoContent();
            }
            catch (AppException ex)
            {
                LogError(this.logger, $"AppException: {ex.Message}", ex);
                return this.BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                LogWarning(this.logger, $"Post with ID: {postId} not found.", null);
                return this.NotFound($"Post with ID {postId} not found.");
            }
            catch (Exception ex)
            {
                LogError(this.logger, $"Internal server error: {ex.Message}", ex);
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a specified post.
        /// </summary>
        /// <param name="postId">The ID of the post to delete.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        // DELETE: api/threads/{threadId}/posts/{postId}
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (!await this.postService.CanUserModifyPost(currentUser, postId))
            {
                LogWarning(this.logger, $"User {currentUser.Id} is unauthorized to delete post ID: {postId}", null);
                return this.Forbid();
            }

            try
            {
                LogInfo(this.logger, $"Deleting post with ID: {postId}", null);
                await this.postService.DeletePostAsync(postId);
                LogInfo(this.logger, $"Post with ID: {postId} deleted successfully.", null);
                return this.NoContent();
            }
            catch (AppException ex)
            {
                LogError(this.logger, $"AppException: {ex.Message}", ex);
                return this.BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                LogWarning(this.logger, $"Post with ID: {postId} not found for deletion.", null);
                return this.NotFound($"Post with ID {postId} not found.");
            }
            catch (Exception ex)
            {
                LogError(this.logger, $"Internal server error: {ex.Message}", ex);
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the list of replies for a specified post.
        /// </summary>
        /// <param name="postId">The ID of the post.</param>
        /// <returns>A list of replies for the specified post.</returns>
        // GET: api/threads/{threadId}/posts/{postId}/replies
        [AllowAnonymous]
        [HttpGet("{postId}/replies")]
        public async Task<ActionResult<IEnumerable<PostReplyModel>>> GetReplies(int postId)
        {
            try
            {
                LogInfo(this.logger, $"Getting replies for post ID: {postId}", null);
                var replies = await this.postService.GetRepliesByPostIdAsync(postId);
                if (replies == null)
                {
                    LogWarning(this.logger, $"No replies found for post ID: {postId}", null);
                    return this.NotFound();
                }

                LogInfo(this.logger, $"Retrieved {replies.Count()} replies for post ID: {postId}", null);
                return this.Ok(replies);
            }
            catch (AppException ex)
            {
                LogError(this.logger, $"AppException: {ex.Message}", ex);
                return this.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                LogError(this.logger, $"Internal server error: {ex.Message}", ex);
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a reply to a specified post.
        /// </summary>\
        /// <param name="threadId">The ID of the thread to which the post is being added.</param>
        /// <param name="postId">The ID of the post to which the reply is being added.</param>
        /// <param name="replyModel">The model containing the data for the new reply.</param>
        /// <returns>The created reply.</returns>
        // POST: api/threads/{threadId}/posts/{postId}/replies
        [HttpPost("{postId}/replies")]
        public async Task<ActionResult<PostReplyModel>> AddReply(int threadId, int postId, [FromBody] PostReplyCreateModel replyModel)
        {
            if (!this.ModelState.IsValid)
            {
                LogWarning(this.logger, $"ModelState is invalid when adding reply to post ID: {postId}", null);
                return this.BadRequest(this.ModelState);
            }

            var currentUser = await this.userManager.GetUserAsync(this.User);
            if (currentUser == null)
            {
                LogWarning(this.logger, "User not found while adding reply", null);
                return this.Unauthorized("User not found.");
            }

            try
            {
                LogInfo(this.logger, $"Adding new reply to post ID: {postId} by user ID: {currentUser.Id}", null);
                var createdReply = await this.postService.AddReplyAsync(postId, currentUser.Id, replyModel);
                if (createdReply == null)
                {
                    LogWarning(this.logger, "Failed to add reply.", null);
                    return this.BadRequest("Failed to add reply.");
                }

                LogInfo(this.logger, $"Reply created with ID: {createdReply.Id}", null);
                return this.CreatedAtAction(nameof(this.GetReplies), new { threadId = threadId, postId = postId }, createdReply);
            }
            catch (AppException ex)
            {
                LogError(this.logger, $"AppException: {ex.Message}", ex);
                return this.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                LogError(this.logger, $"Internal server error: {ex.Message}", ex);
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing reply for a specified post.
        /// </summary>
        /// <param name="postId">The ID of the post to which the reply belongs.</param>
        /// <param name="replyId">The ID of the reply to update.</param>
        /// <param name="replyModel">The model containing the updated data for the reply.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        // PUT: api/threads/{threadId}/posts/{postId}/replies/{replyId}
        [HttpPut("{postId}/replies/{replyId}")]
        public async Task<IActionResult> UpdateReply(int postId, int replyId, [FromBody] PostReplyCreateModel replyModel)
        {
            if (!this.ModelState.IsValid)
            {
                LogWarning(this.logger, $"ModelState is invalid when updating reply ID: {replyId} for post ID: {postId}", null);
                return this.BadRequest(this.ModelState);
            }

            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (!await this.postService.CanUserModifyReply(currentUser, replyId))
            {
                LogWarning(this.logger, $"User {currentUser.Id} is unauthorized to modify reply ID: {replyId}", null);
                return this.Forbid();
            }

            try
            {
                LogInfo(this.logger, $"Updating reply with ID: {replyId} for post ID: {postId}", null);
                _ = await this.postService.UpdateReplyAsync(replyId, currentUser.Id, replyModel);
                LogInfo(this.logger, $"Reply with ID: {replyId} updated successfully.", null);
                return this.NoContent();
            }
            catch (AppException ex)
            {
                LogError(this.logger, $"AppException: {ex.Message}", ex);
                return this.BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                LogWarning(this.logger, $"Reply with ID: {replyId} not found.", null);
                return this.NotFound($"Reply with ID {replyId} not found.");
            }
            catch (Exception ex)
            {
                LogError(this.logger, $"Internal server error: {ex.Message}", ex);
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a specified reply from a post.
        /// </summary>
        /// <param name="postId">The ID of the post from which the reply will be deleted.</param>
        /// <param name="replyId">The ID of the reply to delete.</param>
        /// <returns>An IActionResult indicating the result of the operation.</returns>
        // DELETE: api/threads/{threadId}/posts/{postId}/replies/{replyId}
        [HttpDelete("{postId}/replies/{replyId}")]
        public async Task<IActionResult> DeleteReply(int postId, int replyId)
        {
            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (!await this.postService.CanUserModifyReply(currentUser, replyId))
            {
                LogWarning(this.logger, $"User {currentUser.Id} is unauthorized to delete reply ID: {replyId}", null);
                return this.Forbid();
            }

            try
            {
                LogInfo(this.logger, $"Deleting reply with ID: {replyId} for post ID: {postId}", null);
                await this.postService.DeleteReplyAsync(replyId);
                LogInfo(this.logger, $"Reply with ID: {replyId} deleted successfully.", null);
                return this.NoContent();
            }
            catch (AppException ex)
            {
                LogError(this.logger, $"AppException: {ex.Message}", ex);
                return this.BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                LogWarning(this.logger, $"Reply with ID: {replyId} not found for deletion.", null);
                return this.NotFound($"Reply with ID {replyId} not found.");
            }
            catch (Exception ex)
            {
                LogError(this.logger, $"Internal server error: {ex.Message}", ex);
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
