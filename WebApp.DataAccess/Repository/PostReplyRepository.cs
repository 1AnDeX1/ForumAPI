using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;

namespace WebApp.DataAccess.Repository
{
    /// <summary>
    /// Repository for managing <see cref="PostReply"/> entities in the database.
    /// </summary>
    public class PostReplyRepository : IPostReplyRepository
    {
        private readonly ApplicationDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostReplyRepository"/> class.
        /// </summary>
        /// <param name="context">The application's database context.</param>
        public PostReplyRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Retrieves all replies associated with a specific post.
        /// </summary>
        /// <param name="postId">The unique identifier of the post.</param>
        /// <returns>
        /// An asynchronous operation that returns an <see cref="IEnumerable{PostReply}"/> containing all replies for the specified post.
        /// </returns>
        public async Task<IEnumerable<PostReply>> GetRepliesByPostIdAsync(int postId)
        {
            return await this.context.PostReplies
                                     .Where(r => r.Post!.Id == postId)
                                     .Include(r => r.User)
                                     .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific post reply by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the post reply.</param>
        /// <returns>
        /// An asynchronous operation that returns the <see cref="PostReply"/> with the specified identifier,
        /// or <c>null</c> if no matching reply is found.
        /// </returns>
        public async Task<PostReply?> GetByIdAsync(int id)
        {
            return await this.context.PostReplies.Include(r => r.User)
                                                 .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Adds a new reply to a post in the database.
        /// </summary>
        /// <param name="reply">The <see cref="PostReply"/> to add.</param>
        /// <returns>
        /// An asynchronous operation that returns the added <see cref="PostReply"/>.
        /// </returns>
        public async Task<PostReply> AddAsync(PostReply reply)
        {
            _ = await this.context.PostReplies.AddAsync(reply);
            _ = await this.context.SaveChangesAsync();
            return reply;
        }

        /// <summary>
        /// Updates an existing post reply in the database.
        /// </summary>
        /// <param name="reply">The <see cref="PostReply"/> containing updated data.</param>
        /// <returns>
        /// An asynchronous operation that returns the updated <see cref="PostReply"/>.
        /// </returns>
        public async Task<PostReply> UpdateAsync(PostReply reply)
        {
            _ = this.context.PostReplies.Update(reply);
            _ = await this.context.SaveChangesAsync();
            return reply;
        }

        /// <summary>
        /// Deletes a post reply by its identifier from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the post reply to delete.</param>
        /// <returns>An asynchronous operation.</returns>
        public async Task DeleteAsync(int id)
        {
            var reply = await this.context.PostReplies.FindAsync(id);
            if (reply != null)
            {
                _ = this.context.PostReplies.Remove(reply);
                _ = await this.context.SaveChangesAsync();
            }
        }
    }
}
