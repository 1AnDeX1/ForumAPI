using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repository;

namespace WebApp.Tests.Repositories;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
public class PostReplyRepositoryTests
{
    private static ApplicationDbContext GetInMemoryDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        return new ApplicationDbContext(options);
    }

    private async Task SeedDataAsync(ApplicationDbContext context)
    {
        var user = new ApplicationUser { Id = "1", UserName = "TestUser" };
        var post = new Post { Id = 1, Content = "Test Post", UserId = "1" };
        var replies = new List<PostReply>
        {
            new PostReply { Id = 1, Content = "Reply 1", PostId = 1, UserId = "1" },
            new PostReply { Id = 2, Content = "Reply 2", PostId = 1, UserId = "1" }
        };

        _ = await context.Users.AddAsync(user);
        _ = await context.Posts.AddAsync(post);
        await context.PostReplies.AddRangeAsync(replies);
        _ = await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetRepliesByPostIdAsync_ShouldReturnReplies_WhenRepliesExist()
    {
        // Arrange
        var context = GetInMemoryDbContext("GetRepliesByPostIdAsync_ShouldReturnReplies");
        await this.SeedDataAsync(context);
        var repository = new PostReplyRepository(context);

        // Act
        var replies = await repository.GetRepliesByPostIdAsync(1);

        // Assert
        Assert.NotEmpty(replies);
        Assert.Equal(2, replies.Count());
    }

    [Fact]
    public async Task GetRepliesByPostIdAsync_ShouldReturnEmpty_WhenNoRepliesExist()
    {
        // Arrange
        var context = GetInMemoryDbContext("GetRepliesByPostIdAsync_ShouldReturnEmpty");
        var repository = new PostReplyRepository(context);

        // Act
        var replies = await repository.GetRepliesByPostIdAsync(1);

        // Assert
        Assert.Empty(replies);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnReply_WhenReplyExists()
    {
        // Arrange
        var context = GetInMemoryDbContext("GetByIdAsync_ShouldReturnReply");
        await this.SeedDataAsync(context);
        var repository = new PostReplyRepository(context);

        // Act
        var reply = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(reply);
        Assert.Equal(1, reply!.Id);
        Assert.Equal("Reply 1", reply.Content);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenReplyDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext("GetByIdAsync_ShouldReturnNull");
        var repository = new PostReplyRepository(context);

        // Act
        var reply = await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(reply);
    }

    [Fact]
    public async Task AddAsync_ShouldAddReply_WhenReplyIsValid()
    {
        // Arrange
        var context = GetInMemoryDbContext("AddAsync_ShouldAddReply");
        var repository = new PostReplyRepository(context);
        var newReply = new PostReply { Id = 3, Content = "New Reply", PostId = 1, UserId = "1" };

        // Act
        var addedReply = await repository.AddAsync(newReply);
        var storedReply = await context.PostReplies.FindAsync(3);

        // Assert
        Assert.NotNull(storedReply);
        Assert.Equal(newReply.Content, storedReply.Content);
        Assert.Equal(newReply.Id, addedReply.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowException_WhenReplyIsNull()
    {
        // Arrange
        var context = GetInMemoryDbContext("AddAsync_ShouldThrowException");
        var repository = new PostReplyRepository(context);

        // Act & Assert
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateReply_WhenReplyExists()
    {
        // Arrange
        var context = GetInMemoryDbContext("UpdateAsync_ShouldUpdateReply");
        await this.SeedDataAsync(context);
        var repository = new PostReplyRepository(context);
        var existingReply = await context.PostReplies.FindAsync(1);
        existingReply!.Content = "Updated Reply";

        // Act
        var updatedReply = await repository.UpdateAsync(existingReply);

        // Assert
        Assert.Equal("Updated Reply", updatedReply.Content);
        Assert.Equal("Updated Reply", (await context.PostReplies.FindAsync(1))!.Content);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenReplyDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext("UpdateAsync_ShouldThrowException");
        var repository = new PostReplyRepository(context);
        var nonExistingReply = new PostReply { Id = 999, Content = "Non-existing Reply", PostId = 1, UserId = "1" };

        // Act & Assert
        _ = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => repository.UpdateAsync(nonExistingReply));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveReply_WhenReplyExists()
    {
        // Arrange
        var context = GetInMemoryDbContext("DeleteAsync_ShouldRemoveReply");
        await this.SeedDataAsync(context);
        var repository = new PostReplyRepository(context);

        // Act
        await repository.DeleteAsync(1);
        var deletedReply = await context.PostReplies.FindAsync(1);

        // Assert
        Assert.Null(deletedReply);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDoNothing_WhenReplyDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext("DeleteAsync_ShouldDoNothing");
        var repository = new PostReplyRepository(context);

        // Act
        await repository.DeleteAsync(999);

        // Assert
        Assert.Empty(context.PostReplies);
    }
}
