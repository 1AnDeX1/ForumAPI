using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repository;

namespace WebApp.Tests.Repositories
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public class PostRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext context;
        private readonly PostRepository repository;

        public PostRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            this.context = new ApplicationDbContext(options);
            this.repository = new PostRepository(this.context);
        }

        public void Dispose()
        {
            _ = this.context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task GetPostsByThreadIdAsync_ReturnsPostsForThread()
        {
            // Arrange
            var thread = new ForumThread { Id = 1, Title = "Test Thread" };
            _ = this.context.Threads.Add(thread);
            this.context.Posts.AddRange(
                new Post { Content = "Post 1", ThreadId = 1 },
                new Post { Content = "Post 2", ThreadId = 1 }
            );
            _ = await this.context.SaveChangesAsync();

            // Act
            var result = await this.repository.GetPostsByThreadIdAsync(1, page: 1, pageSize: 10);

            // Assert
            Assert.Equal(2, result.posts.Count());
            Assert.Equal(2, result.postsCount);
        }

        [Fact]
        public async Task GetPostsByThreadIdNoPaginationAsync_ReturnsAllPosts()
        {
            // Arrange
            var thread = new ForumThread { Id = 1, Title = "Test Thread" };
            _ = this.context.Threads.Add(thread);
            this.context.Posts.AddRange(
                new Post { Content = "Post 1", ThreadId = 1 },
                new Post { Content = "Post 2", ThreadId = 1 }
            );
            _ = await this.context.SaveChangesAsync();

            // Act
            var result = await this.repository.GetPostsByThreadIdNoPaginationAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsPostIfFound()
        {
            // Arrange
            var post = new Post { Content = "Post 1" };
            _ = this.context.Posts.Add(post);
            _ = await this.context.SaveChangesAsync();

            // Act
            var result = await this.repository.GetByIdAsync(post.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Post 1", result?.Content);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullIfNotFound()
        {
            // Act
            var result = await this.repository.GetByIdAsync(99); // ID that doesn't exist

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_AddsNewPost()
        {
            // Arrange
            var post = new Post { Content = "New Post" };

            // Act
            var result = await this.repository.AddAsync(post);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Post", result.Content);
            Assert.Equal(1, await this.context.Posts.CountAsync()); // Ensure one post exists
        }

        [Fact]
        public async Task UpdateAsync_UpdatesPost()
        {
            // Arrange
            var post = new Post { Content = "Old Content" };
            _ = this.context.Posts.Add(post);
            _ = await this.context.SaveChangesAsync();

            // Act
            post.Content = "Updated Content";
            var result = await this.repository.UpdateAsync(post);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Content", result.Content);
        }

        [Fact]
        public async Task DeleteAsync_DeletesPostIfFound()
        {
            // Arrange
            var post = new Post { Content = "Post to Delete" };
            _ = this.context.Posts.Add(post);
            _ = await this.context.SaveChangesAsync();

            // Act
            await this.repository.DeleteAsync(post.Id);

            // Assert
            Assert.Equal(0, await this.context.Posts.CountAsync()); // Post should be deleted
        }

        [Fact]
        public async Task DeleteAsync_DoesNothingIfPostNotFound()
        {
            // Arrange
            var post = new Post { Content = "Existing Post" };
            _ = this.context.Posts.Add(post);
            _ = await this.context.SaveChangesAsync();

            // Act
            await this.repository.DeleteAsync(99); // ID that doesn't exist

            // Assert
            Assert.Equal(1, await this.context.Posts.CountAsync()); // Post count should remain the same
        }
    }
}
