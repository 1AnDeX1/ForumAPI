using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;

namespace WebApp.DataAccess.Repository
{
    /// <summary>
    /// Repository class for managing post data in the database.
    /// </summary>
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostRepository"/> class.
        /// </summary>
        /// <param name="context">The database context to access post data.</param>
        public PostRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Retrieves all posts associated with a specific thread, including user and thread details.
        /// </summary>
        /// <param name="threadId">The unique identifier of the thread.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>
        /// An asynchronous operation that returns an <see cref="IEnumerable{Post}"/> containing all posts in the specified thread.
        /// </returns>
        public async Task<(IEnumerable<Post> posts, int postsCount)> GetPostsByThreadIdAsync(int threadId, int page, int pageSize)
        {
            var posts = await this.context.Posts
                                 .Where(p => p.ThreadId == threadId)
                                 .Include(p => p.User)
                                 .Include(p => p.Thread)
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

            var postsCount = await this.context.Posts
                                 .Where(p => p.ThreadId == threadId)
                                 .CountAsync();

            return (posts, postsCount);
        }

        /// <summary>
        /// Retrieves all posts associated with a specific thread, including user and thread details.
        /// </summary>
        /// <param name="threadId">The unique identifier of the thread.</param>
        /// <returns>
        /// An asynchronous operation that returns an <see cref="IEnumerable{Post}"/> containing all posts in the specified thread.
        /// </returns>
        public async Task<IEnumerable<Post>> GetPostsByThreadIdNoPaginationAsync(int threadId)
        {
            return await this.context.Posts
                                 .Where(p => p.ThreadId == threadId)
                                 .Include(p => p.User)
                                 .Include(p => p.Thread)
                                 .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific post by its identifier, including user details.
        /// </summary>
        /// <param name="id">The unique identifier of the post.</param>
        /// <returns>
        /// An asynchronous operation that returns the <see cref="Post"/> with the specified identifier,
        /// or <c>null</c> if no matching post is found.
        /// </returns>
        public async Task<Post?> GetByIdAsync(int id)
        {
            return await this.context.Posts.Include(p => p.User)
                                       .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Adds a new post to the database.
        /// </summary>
        /// <param name="post">The <see cref="Post"/> to add.</param>
        /// <returns>
        /// An asynchronous operation that returns the added <see cref="Post"/>.
        /// </returns>
        public async Task<Post> AddAsync(Post post)
        {
            _ = await this.context.Posts.AddAsync(post);
            _ = await this.context.SaveChangesAsync();
            return post;
        }

        /// <summary>
        /// Updates an existing post in the database.
        /// </summary>
        /// <param name="post">The <see cref="Post"/> containing updated data.</param>
        /// <returns>
        /// An asynchronous operation that returns the updated <see cref="Post"/>.
        /// </returns>
        public async Task<Post> UpdateAsync(Post post)
        {
            _ = this.context.Posts.Update(post);
            _ = await this.context.SaveChangesAsync();
            return post;
        }

        /// <summary>
        /// Deletes a post by its identifier from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the post to delete.</param>
        /// <returns>An asynchronous operation.</returns>
        public async Task DeleteAsync(int id)
        {
            var post = await this.context.Posts.FindAsync(id);
            if (post != null)
            {
                _ = this.context.Posts.Remove(post);
                _ = await this.context.SaveChangesAsync();
            }
        }
    }
}
