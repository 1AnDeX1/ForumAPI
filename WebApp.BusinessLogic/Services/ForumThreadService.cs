using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.ThreadModels;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;

namespace WebApp.BusinessLogic.Services
{
    /// <summary>
    /// Provides services for managing forum threads.
    /// </summary>
    public class ForumThreadService : IForumThreadService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IForumThreadRepository threadRepository;
        private readonly IPostRepository postRepository;
        private readonly IPostService postService;
        private readonly IMapper mapper;
        private readonly ILogger<ForumThreadService> logger;

#pragma warning disable SA1204 // Static elements should appear before instance elements
        private static readonly Action<ILogger, string, Exception?> LogWarning =
#pragma warning restore SA1204 // Static elements should appear before instance elements
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(0, nameof(LogWarning)),
                "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogError =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(1, nameof(LogError)),
                "{Message}");

        private static readonly Action<ILogger, string, Exception?> LogInformation =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(2, nameof(LogInformation)),
                "{Message}");

        /// <summary>
        /// Initializes a new instance of the <see cref="ForumThreadService"/> class.
        /// </summary>
        /// <param name="threadRepository">The repository for managing forum threads.</param>
        /// <param name="mapper">The AutoMapper instance for mapping models.</param>
        /// <param name="userManager">The UserManager instance for managing user information.</param>
        /// <param name="logger">The logger instance for logging operations.</param>
        public ForumThreadService(
            IForumThreadRepository threadRepository,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ILogger<ForumThreadService> logger,
            IPostRepository postRepository,
            IPostService postService)
        {
            this.threadRepository = threadRepository;
            this.mapper = mapper;
            this.userManager = userManager;
            this.logger = logger;
            this.postRepository = postRepository;
            this.postService = postService;
        }

        /// <summary>
        /// Retrieves all forum threads asynchronously.
        /// </summary>
        /// <param name="title">Value of Forum thread.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A task that represents the asynchronous operation, containing a collection of <see cref="ForumThreadModel"/>.</returns>
        public async Task<(IEnumerable<ForumThreadModel> threads, int threadsCount)> GetAllThreadsAsync(string? title, int page, int pageSize)
        {
            var threadsObject = string.IsNullOrEmpty(title)
                ? await this.threadRepository.GetAllAsync(page, pageSize)
                : await this.threadRepository.GetAllByTitleAsync(title, page, pageSize);

            return (this.mapper.Map<IEnumerable<ForumThreadModel>>(threadsObject.threads), threadsObject.threadsCount);
        }

        /// <summary>
        /// Retrieves a specific forum thread by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the forum thread.</param>
        /// <returns>A task that represents the asynchronous operation, containing the <see cref="ForumThreadModel"/> if found; otherwise, <c>null</c>.</returns>
        /// <exception cref="AppException">Thrown when the thread is not found.</exception>
        public async Task<ForumThreadModel?> GetThreadByIdAsync(int id)
        {
            var thread = await this.threadRepository.GetByIdAsync(id);
            if (thread == null)
            {
                LogWarning(this.logger, $"Thread with ID {id} not found.", null);
                throw new AppException($"Thread with ID {id} not found.");
            }

            return this.mapper.Map<ForumThreadModel>(thread);
        }

        /// <summary>
        /// Creates a new forum thread asynchronously.
        /// </summary>
        /// <param name="threadCreateModel">The model containing the information for the new thread.</param>
        /// <param name="userId">The identifier of the user creating the thread.</param>
        /// <returns>A task that represents the asynchronous operation, containing the created <see cref="ForumThreadModel"/>.</returns>
        public async Task<ForumThreadModel> CreateThreadAsync(ForumThreadCreateModel threadCreateModel, string userId)
        {
            var thread = this.mapper.Map<ForumThread>(threadCreateModel);
            ValidateThread(thread);

            thread.UserId = userId;

            var createdThread = await this.threadRepository.AddAsync(thread);
            LogInformation(this.logger, $"Thread created with ID {createdThread.Id}.", null);
            return this.mapper.Map<ForumThreadModel>(createdThread);
        }

        /// <summary>
        /// Updates an existing forum thread asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the thread to update.</param>
        /// <param name="threadModel">The model containing the updated information for the thread.</param>
        /// <returns>A task that represents the asynchronous operation, containing the updated <see cref="ForumThreadModel"/>.</returns>
        /// <exception cref="AppException">Thrown when the thread is not found.</exception>
        public async Task<ForumThreadModel> UpdateThreadAsync(int id, ForumThreadCreateModel threadModel)
        {
            var existingThread = await this.threadRepository.GetByIdAsync(id);
            if (existingThread == null)
            {
                LogWarning(this.logger, $"Thread with ID {id} not found.", null);
                throw new AppException($"Thread with ID {id} not found.");
            }

            var updatedThread = this.mapper.Map(threadModel, existingThread);
            ValidateThread(updatedThread);

            updatedThread.Id = id;
            updatedThread.UserId = existingThread.UserId;

            _ = await this.threadRepository.UpdateAsync(updatedThread);
            LogInformation(this.logger, $"Thread updated with ID {updatedThread.Id}.", null);
            return this.mapper.Map<ForumThreadModel>(updatedThread);
        }

        /// <summary>
        /// Deletes a forum thread by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the forum thread to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="AppException">Thrown when the thread is not found.</exception>
        public async Task DeleteThreadAsync(int id)
        {
            var thread = await this.threadRepository.GetByIdAsync(id);
            if (thread == null)
            {
                LogWarning(this.logger, $"Thread with ID {id} not found.", null);
                throw new AppException($"Thread with ID {id} not found.");
            }

            // Get all posts associated with the thread
            var posts = await this.postRepository.GetPostsByThreadIdNoPaginationAsync(id);

            // Delete each post and its replies
            foreach (var post in posts)
            {
                await this.postService.DeletePostAsync(post.Id);
            }

            await this.threadRepository.DeleteAsync(id);
            LogInformation(this.logger, $"Thread deleted with ID {id}.", null);
        }

        /// <summary>
        /// Determines whether a user can modify a specific forum thread asynchronously.
        /// </summary>
        /// <param name="user">The user attempting to modify the thread.</param>
        /// <param name="threadId">The identifier of the thread to check.</param>
        /// <returns>A task that represents the asynchronous operation, containing <c>true</c> if the user can modify the thread; otherwise, <c>false</c>.</returns>
        /// <exception cref="AppException">Thrown when the thread is not found.</exception>
        public async Task<bool> CanUserModifyThread(ApplicationUser user, int threadId)
        {
            var thread = await this.threadRepository.GetByIdAsync(threadId);

            if (thread == null)
            {
                LogWarning(this.logger, $"Thread with ID {threadId} not found.", null);
                throw new AppException($"Thread with ID {threadId} not found.");
            }

            ArgumentNullException.ThrowIfNull(user);

            var isAdmin = await this.userManager.IsInRoleAsync(user, "Admin");
            return isAdmin || thread.UserId == user.Id;
        }

        /// <summary>
        /// Validates a forum thread for required fields and length constraints.
        /// </summary>
        /// <param name="thread">The thread to validate.</param>
        /// <exception cref="AppException">Thrown when validation fails.</exception>
        private static void ValidateThread(ForumThread? thread)
        {
            if (string.IsNullOrWhiteSpace(thread?.Title))
            {
                throw new AppException("Thread title cannot be empty.");
            }

            if (thread.Title.Length > 100)
            {
                throw new AppException("Thread title cannot exceed 100 characters.");
            }

            if (thread.Content?.Length > 5000)
            {
                throw new AppException("Thread description cannot exceed 5000 characters.");
            }
        }
    }
}
