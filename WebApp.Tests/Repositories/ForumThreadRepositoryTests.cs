using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repository;

namespace WebApp.Tests.Repositories
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public class ForumThreadRepositoryTests
    {
        private static ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnThreads_WithPagination()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetAllAsync_ShouldReturnThreads_WithPagination");
            var repository = new ForumThreadRepository(context);

            context.Threads.AddRange(
                new ForumThread { Title = "Thread 1", UserId = "1" },
                new ForumThread { Title = "Thread 2", UserId = "2" },
                new ForumThread { Title = "Thread 3", UserId = "3" }
            );
            _ = await context.SaveChangesAsync();

            // Act
            var (threads, threadsCount) = await repository.GetAllAsync(1, 2);

            // Assert
            Assert.Equal(3, threadsCount);
            Assert.Equal(2, threads.Count());
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmpty_WhenNoThreadsExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetAllAsync_ShouldReturnEmpty_WhenNoThreadsExist");
            var repository = new ForumThreadRepository(context);

            // Act
            var (threads, threadsCount) = await repository.GetAllAsync(1, 10);

            // Assert
            Assert.Empty(threads);
            Assert.Equal(0, threadsCount);
        }

        [Fact]
        public async Task GetAllAsync_ShouldHandleInvalidPageAndPageSize()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetAllAsync_ShouldHandleInvalidPageAndPageSize");
            var repository = new ForumThreadRepository(context);

            _ = context.Threads.Add(new ForumThread { Title = "Valid Thread", UserId = "1" });
            _ = await context.SaveChangesAsync();

            // Act
            var (threads, threadsCount) = await repository.GetAllAsync(-1, -10); // Invalid pagination inputs

            // Assert
            Assert.Empty(threads); // Should return empty due to invalid pagination
            Assert.Equal(1, threadsCount); // Total count remains valid
        }

        [Fact]
        public async Task GetAllByTitleAsync_ShouldReturnFilteredThreads()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetAllByTitleAsync_ShouldReturnFilteredThreads");
            var repository = new ForumThreadRepository(context);

            context.Threads.AddRange(
                new ForumThread { Title = "Test Thread", UserId = "1" },
                new ForumThread { Title = "Another Thread", UserId = "2" }
            );
            _ = await context.SaveChangesAsync();

            // Act
            var (threads, threadsCount) = await repository.GetAllByTitleAsync("Test", 1, 2);

            // Assert
            _ = Assert.Single(threads);
            Assert.Equal(1, threadsCount);
            Assert.Contains(threads, t => t.Title == "Test Thread");
        }

        [Fact]
        public async Task GetAllByTitleAsync_ShouldReturnEmpty_WhenNoMatchingTitleExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetAllByTitleAsync_ShouldReturnEmpty_WhenNoMatchingTitleExists");
            var repository = new ForumThreadRepository(context);

            _ = context.Threads.Add(new ForumThread { Title = "Unrelated Title", UserId = "1" });
            _ = await context.SaveChangesAsync();

            // Act
            var (threads, threadsCount) = await repository.GetAllByTitleAsync("Nonexistent", 1, 10);

            // Assert
            Assert.Empty(threads);
            Assert.Equal(0, threadsCount);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnThread_WhenExists()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetByIdAsync_ShouldReturnThread_WhenExists");
            var repository = new ForumThreadRepository(context);

            _ = context.Threads.Add(new ForumThread { Id = 1, Title = "Thread 1", UserId = "1" });
            _ = await context.SaveChangesAsync();

            // Act
            var thread = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(thread);
            Assert.Equal("Thread 1", thread.Title);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenThreadDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("GetByIdAsync_ShouldReturnNull_WhenThreadDoesNotExist");
            var repository = new ForumThreadRepository(context);

            // Act
            var thread = await repository.GetByIdAsync(999); // Non-existing ID

            // Assert
            Assert.Null(thread);
        }


        [Fact]
        public async Task AddAsync_ShouldAddThread()
        {
            // Arrange
            var context = GetInMemoryDbContext("AddAsync_ShouldAddThread");
            var repository = new ForumThreadRepository(context);

            var newThread = new ForumThread { Title = "New Thread", UserId = "1" };

            // Act
            var addedThread = await repository.AddAsync(newThread);

            // Assert
            Assert.NotNull(addedThread);
            Assert.Equal(1, await context.Threads.CountAsync());
            Assert.Equal("New Thread", addedThread.Title);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenThreadIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext("AddAsync_ShouldThrowException_WhenThreadIsNull");
            var repository = new ForumThreadRepository(context);

            // Act & Assert
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await repository.AddAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateThread()
        {
            // Arrange
            var context = GetInMemoryDbContext("UpdateAsync_ShouldUpdateThread");
            var repository = new ForumThreadRepository(context);

            var existingThread = new ForumThread { Id = 1, Title = "Old Title", UserId = "1" };
            _ = context.Threads.Add(existingThread);
            _ = await context.SaveChangesAsync();

            // Act
            existingThread.Title = "Updated Title";
            var updatedThread = await repository.UpdateAsync(existingThread);

            // Assert
            Assert.Equal("Updated Title", updatedThread.Title);
            Assert.Equal("Updated Title", (await context.Threads.FirstAsync()).Title);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenThreadDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("UpdateAsync_ShouldThrowException_WhenThreadDoesNotExist");
            var repository = new ForumThreadRepository(context);

            var nonExistingThread = new ForumThread { Id = 999, Title = "Non-existing Thread", UserId = "1" };

            // Act & Assert
            _ = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await repository.UpdateAsync(nonExistingThread));
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveThread()
        {
            // Arrange
            var context = GetInMemoryDbContext("DeleteAsync_ShouldRemoveThread");
            var repository = new ForumThreadRepository(context);

            var threadToDelete = new ForumThread { Id = 1, Title = "Thread to Delete", UserId = "1" };
            _ = context.Threads.Add(threadToDelete);
            _ = await context.SaveChangesAsync();

            // Act
            await repository.DeleteAsync(1);

            // Assert
            Assert.Equal(0, await context.Threads.CountAsync());
        }

        [Fact]
        public async Task DeleteAsync_ShouldDoNothing_WhenThreadDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext("DeleteAsync_ShouldDoNothing_WhenThreadDoesNotExist");
            var repository = new ForumThreadRepository(context);

            // Act
            await repository.DeleteAsync(999); // Non-existing ID

            // Assert
            Assert.Empty(context.Threads); // Ensure no threads were deleted
        }

    }
}
