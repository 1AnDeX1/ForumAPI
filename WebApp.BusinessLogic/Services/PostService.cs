using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.BusinessLogic.Models.PostReplyModels;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;

namespace WebApp.BusinessLogic.Services
{
    public class PostService : IPostService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPostRepository postRepository;
        private readonly IPostReplyRepository postReplyRepository;
        private readonly IMapper mapper;
        private readonly ILogger<PostService> logger;

#pragma warning disable SA1204 // Static elements should appear before instance elements
        private static readonly Action<ILogger, string, Exception?> LogInformation =
#pragma warning restore SA1204 // Static elements should appear before instance elements
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(0, nameof(LogInformation)),
                "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogWarning =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(1, nameof(LogWarning)),
                "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogError =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(2, nameof(LogError)),
                "{Message}");

        /// <summary>
        /// Initializes a new instance of the <see cref="PostService"/> class.
        /// </summary>
        /// <param name="postRepository">The repository for managing posts.</param>
        /// <param name="postReplyRepository">The repository for managing post replies.</param>
        /// <param name="mapper">The AutoMapper instance for mapping between entities and models.</param>
        /// <param name="userManager">The UserManager for managing application users.</param>
        /// <param name="logger">The logger for logging information, warnings, and errors.</param>
        public PostService(
            IPostRepository postRepository,
            IPostReplyRepository postReplyRepository,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ILogger<PostService> logger)
        {
            this.postRepository = postRepository;
            this.postReplyRepository = postReplyRepository;
            this.mapper = mapper;
            this.userManager = userManager;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all posts associated with a specific thread asynchronously.
        /// </summary>
        /// <param name="threadId">The identifier of the thread for which to retrieve posts.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A task that represents the asynchronous operation, containing a collection of <see cref="PostModel"/>.</returns>
        public async Task<(IEnumerable<PostModel> posts, int postsCount)> GetPostsByThreadIdAsync(int threadId, int page, int pageSize)
        {
            LogInformation(this.logger, $"Getting posts for thread ID: {threadId}", null);
            var postsObject = await this.postRepository.GetPostsByThreadIdAsync(threadId, page, pageSize);

            if (postsObject.posts == null || !postsObject.posts.Any())
            {
                LogWarning(this.logger, $"No posts found for thread ID: {threadId}", null);
                throw new AppException($"Posts with this thread ID {threadId} not found.");
            }

            LogInformation(this.logger, $"Retrieved {postsObject.posts.Count()} posts for thread ID: {threadId}", null);
            return (this.mapper.Map<IEnumerable<PostModel>>(postsObject.posts), postsObject.postsCount);
        }

        /// <summary>
        /// Adds a new post to a specific thread asynchronously.
        /// </summary>
        /// <param name="threadId">The identifier of the thread to which the post will be added.</param>
        /// <param name="userId">The identifier of the user creating the post.</param>
        /// <param name="postModel">The model containing the information for the new post.</param>
        /// <returns>A task that represents the asynchronous operation, containing the created <see cref="PostModel"/>.</returns>
        public async Task<PostModel> AddPostAsync(int threadId, string userId, PostCreateModel postModel)
        {
            LogInformation(this.logger, $"Adding new post to thread ID: {threadId} by user ID: {userId}", null);

            var newPost = this.mapper.Map<Post>(postModel);
            ValidatePost(newPost);

            newPost.ThreadId = threadId;
            newPost.UserId = userId;

            var createdPost = await this.postRepository.AddAsync(newPost);
            LogInformation(this.logger, $"Post created with ID: {createdPost.Id}", null);

            return this.mapper.Map<PostModel>(createdPost);
        }

        /// <summary>
        /// Updates an existing post asynchronously.
        /// </summary>
        /// <param name="postId">The identifier of the post to update.</param>
        /// <param name="userId">The identifier of the user updating the post.</param>
        /// <param name="postModel">The model containing the updated information for the post.</param>
        /// <returns>A task that represents the asynchronous operation, containing the updated <see cref="PostModel"/>.</returns>
        public async Task<PostModel> UpdatePostAsync(int postId, string userId, PostCreateModel postModel)
        {
            LogInformation(this.logger, $"Updating post with ID: {postId}", null);
            var existingPost = await this.postRepository.GetByIdAsync(postId);
            if (existingPost == null)
            {
                LogWarning(this.logger, $"Post with ID: {postId} not found.", null);
                throw new AppException($"Post with ID {postId} not found.");
            }

            var updatedPost = this.mapper.Map(postModel, existingPost);
            ValidatePost(updatedPost);

            updatedPost.Id = postId;
            updatedPost.UserId = userId;

            _ = await this.postRepository.UpdateAsync(existingPost);
            LogInformation(this.logger, $"Post with ID: {postId} updated.", null);
            return this.mapper.Map<PostModel>(updatedPost);
        }

        /// <summary>
        /// Deletes a post by its identifier asynchronously.
        /// </summary>
        /// <param name="postId">The identifier of the post to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeletePostAsync(int postId)
        {
            var post = await this.postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                LogWarning(this.logger, $"Post with ID {postId} not found.", null);
                throw new AppException($"Post with ID {postId} not found.");
            }

            // Get all replies associated with the post
            var replies = await this.postReplyRepository.GetRepliesByPostIdAsync(postId);

            // Delete each reply
            foreach (var reply in replies)
            {
                await this.postReplyRepository.DeleteAsync(reply.Id);
            }

            await this.postRepository.DeleteAsync(postId);
            LogInformation(this.logger, $"Post deleted with ID {postId}.", null);
        }

        /// <summary>
        /// Retrieves all replies associated with a specific post asynchronously.
        /// </summary>
        /// <param name="postId">The identifier of the post for which to retrieve replies.</param>
        /// <returns>A task that represents the asynchronous operation, containing a collection of <see cref="PostReplyModel"/>.</returns>
        public async Task<IEnumerable<PostReplyModel>> GetRepliesByPostIdAsync(int postId)
        {
            var replies = await this.postReplyRepository.GetRepliesByPostIdAsync(postId);

            if (replies == null || !replies.Any())
            {
                LogWarning(this.logger, $"Replies with this post ID {postId} not found.", null);
                return this.mapper.Map<IEnumerable<PostReplyModel>>(replies);
            }

            LogInformation(this.logger, $"Retrieved {replies.Count()} replies for post ID: {postId}", null);
            return this.mapper.Map<IEnumerable<PostReplyModel>>(replies);
        }

        /// <summary>
        /// Adds a new reply to a specific post asynchronously.
        /// </summary>
        /// <param name="postId">The identifier of the post to which the reply will be added.</param>
        /// <param name="userId">The identifier of the user creating the reply.</param>
        /// <param name="replyModel">The model containing the information for the new reply.</param>
        /// <returns>A task that represents the asynchronous operation, containing the created <see cref="PostReplyModel"/>.</returns>
        public async Task<PostReplyModel> AddReplyAsync(int postId, string userId, PostReplyCreateModel replyModel)
        {
            var newReply = this.mapper.Map<PostReply>(replyModel);
            ValidateReply(newReply);

            newReply.PostId = postId;
            newReply.UserId = userId;

            var createdReply = await this.postReplyRepository.AddAsync(newReply);
            LogInformation(this.logger, $"Reply created for post ID: {postId} by user ID: {userId}", null);
            return this.mapper.Map<PostReplyModel>(createdReply);
        }

        /// <summary>
        /// Updates an existing reply asynchronously.
        /// </summary>
        /// <param name="replyId">The identifier of the reply to update.</param>
        /// <param name="userId">The identifier of the user updating the reply.</param>
        /// <param name="replyModel">The model containing the updated information for the reply.</param>
        /// <returns>A task that represents the asynchronous operation, containing the updated <see cref="PostReplyModel"/>.</returns>
        public async Task<PostReplyModel> UpdateReplyAsync(int replyId, string userId, PostReplyCreateModel replyModel)
        {
            var existingReply = await this.postReplyRepository.GetByIdAsync(replyId);
            if (existingReply == null)
            {
                LogWarning(this.logger, $"Reply with ID: {replyId} not found.", null);
                throw new AppException($"Reply with ID {replyId} not found.");
            }

            var updatedReply = this.mapper.Map(replyModel, existingReply);
            ValidateReply(updatedReply);

            updatedReply.Id = replyId;
            updatedReply.UserId = userId;

            _ = await this.postReplyRepository.UpdateAsync(updatedReply);
            LogInformation(this.logger, $"Reply with ID: {replyId} updated.", null);
            return this.mapper.Map<PostReplyModel>(updatedReply);
        }

        /// <summary>
        /// Deletes a reply by its identifier asynchronously.
        /// </summary>
        /// <param name="replyId">The identifier of the reply to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteReplyAsync(int replyId)
        {
            var reply = await this.postReplyRepository.GetByIdAsync(replyId);
            if (reply == null)
            {
                LogWarning(this.logger, $"Reply with ID: {replyId} not found.", null);
                throw new AppException($"Reply with ID {replyId} not found.");
            }

            await this.postReplyRepository.DeleteAsync(replyId);
            LogInformation(this.logger, $"Reply with ID: {replyId} deleted.", null);
        }

        public async Task<bool> CanUserModifyPost(ApplicationUser user, int postId)
        {
            var post = await this.postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                LogWarning(this.logger, $"Post with ID {postId} not found.", null);
                throw new AppException($"Post with ID {postId} not found.");
            }

            ArgumentNullException.ThrowIfNull(user);

            var isAdmin = await this.userManager.IsInRoleAsync(user, "Admin");
            return isAdmin || post.UserId == user.Id;
        }

        public async Task<bool> CanUserModifyReply(ApplicationUser user, int replyId)
        {
            var reply = await this.postReplyRepository.GetByIdAsync(replyId);
            if (reply == null)
            {
                LogWarning(this.logger, $"Reply with ID {replyId} not found.", null);
                throw new AppException($"Reply with ID {replyId} not found.");
            }

            ArgumentNullException.ThrowIfNull(user);

            var isAdmin = await this.userManager.IsInRoleAsync(user, "Admin");
            return isAdmin || reply.UserId == user.Id;
        }

        private static void ValidatePost(Post? post)
        {
            if (string.IsNullOrWhiteSpace(post?.Content))
            {
                throw new AppException("Post content cannot be empty.");
            }

            if (post.Content?.Length > 5000)
            {
                throw new AppException("Thread description cannot exceed 5000 characters.");
            }
        }

        private static void ValidateReply(PostReply? reply)
        {
            if (string.IsNullOrWhiteSpace(reply?.Content))
            {
                throw new AppException("Reply content cannot be empty.");
            }

            if (reply.Content?.Length > 5000)
            {
                throw new AppException("Thread description cannot exceed 5000 characters.");
            }
        }
    }
}
