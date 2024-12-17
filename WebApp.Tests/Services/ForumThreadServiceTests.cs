using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic;
using WebApp.BusinessLogic.Models.ThreadModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;
using Xunit;

namespace WebApp.Tests.Services;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
public class ForumThreadServiceTests
{
    private readonly Mock<IForumThreadRepository> mockThreadRepository;
    private readonly Mock<IPostRepository> mockPostRepository;
    private readonly Mock<IPostService> mockPostService;
    private readonly Mock<IMapper> mockMapper;
    private readonly Mock<UserManager<ApplicationUser>> mockUserManager;
    private readonly Mock<ILogger<ForumThreadService>> mockLogger;
    private readonly ForumThreadService forumThreadService;

    public ForumThreadServiceTests()
    {
        mockThreadRepository = new Mock<IForumThreadRepository>();
        mockPostRepository = new Mock<IPostRepository>();
        mockPostService = new Mock<IPostService>();
        mockMapper = new Mock<IMapper>();
        mockLogger = new Mock<ILogger<ForumThreadService>>();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        forumThreadService = new ForumThreadService(
            mockThreadRepository.Object,
            mockMapper.Object,
            mockUserManager.Object,
            mockLogger.Object,
            mockPostRepository.Object,
            mockPostService.Object
        );
    }

    [Fact]
    public async Task GetAllThreadsAsync_ReturnsThreads_WhenTitleIsNull()
    {
        // Arrange
        var threads = new List<ForumThread>
        {
            new ForumThread { Id = 1, Title = "Thread 1", Content = "Content 1" },
            new ForumThread { Id = 2, Title = "Thread 2", Content = "Content 2" }
        };

        mockThreadRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((threads, threads.Count));

        mockMapper
            .Setup(mapper => mapper.Map<IEnumerable<ForumThreadModel>>(threads))
            .Returns(threads.Select(t => new ForumThreadModel { Id = t.Id, Title = t.Title }));

        // Act
        var result = await forumThreadService.GetAllThreadsAsync(null, 1, 10);

        // Assert
        Assert.Equal(2, result.threads.Count());
        Assert.Equal(2, result.threadsCount);
    }

    [Fact]
    public async Task GetAllThreadsAsync_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        mockThreadRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => forumThreadService.GetAllThreadsAsync(null, 1, 10));
    }

    [Fact]
    public async Task GetThreadByIdAsync_ReturnsThread_WhenThreadExists()
    {
        // Arrange
        var thread = new ForumThread { Id = 1, Title = "Thread 1", Content = "Content 1" };
        mockThreadRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(thread);
        mockMapper.Setup(mapper => mapper.Map<ForumThreadModel>(thread))
                  .Returns(new ForumThreadModel { Id = 1, Title = "Thread 1" });

        // Act
        var result = await forumThreadService.GetThreadByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Thread 1", result.Title);
    }

    [Fact]
    public async Task GetThreadByIdAsync_ThrowsAppException_WhenThreadNotFound()
    {
        // Arrange
        mockThreadRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ForumThread?)null);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => forumThreadService.GetThreadByIdAsync(1));
    }

    [Fact]
    public async Task CreateThreadAsync_ReturnsCreatedThread()
    {
        // Arrange
        var threadCreateModel = new ForumThreadCreateModel { Title = "New Thread", Content = "Some content" };
        var thread = new ForumThread { Id = 1, Title = "New Thread", Content = "Some content", UserId = "user123" };

        mockMapper.Setup(mapper => mapper.Map<ForumThread>(threadCreateModel)).Returns(thread);
        mockThreadRepository.Setup(repo => repo.AddAsync(It.IsAny<ForumThread>())).ReturnsAsync(thread);
        mockMapper.Setup(mapper => mapper.Map<ForumThreadModel>(thread))
                  .Returns(new ForumThreadModel { Id = 1, Title = "New Thread" });

        // Act
        var result = await forumThreadService.CreateThreadAsync(threadCreateModel, "user123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Thread", result.Title);
    }

    [Fact]
    public async Task CreateThreadAsync_ThrowsAppException_WhenValidationFails()
    {
        // Arrange
        var threadCreateModel = new ForumThreadCreateModel { Title = "", Content = "Content" }; // Invalid title
        mockMapper.Setup(mapper => mapper.Map<ForumThread>(threadCreateModel)).Returns(new ForumThread());

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => forumThreadService.CreateThreadAsync(threadCreateModel, "user123"));
    }

    [Fact]
    public async Task UpdateThreadAsync_ReturnsUpdatedThread()
    {
        // Arrange
        var threadModel = new ForumThreadCreateModel { Title = "Updated Thread", Content = "Updated content" };
        var existingThread = new ForumThread { Id = 1, Title = "Old Thread", Content = "Old content", UserId = "user123" };

        mockThreadRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existingThread);
        mockMapper.Setup(mapper => mapper.Map(threadModel, existingThread)).Returns(existingThread);
        mockThreadRepository.Setup(repo => repo.UpdateAsync(existingThread)).ReturnsAsync(existingThread);
        mockMapper.Setup(mapper => mapper.Map<ForumThreadModel>(existingThread))
                  .Returns(new ForumThreadModel { Id = 1, Title = "Updated Thread" });

        // Act
        var result = await forumThreadService.UpdateThreadAsync(1, threadModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Thread", result.Title);
    }

    [Fact]
    public async Task UpdateThreadAsync_ThrowsAppException_WhenThreadNotFound()
    {
        // Arrange
        mockThreadRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ForumThread?)null);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => forumThreadService.UpdateThreadAsync(1, new ForumThreadCreateModel()));
    }

    [Fact]
    public async Task DeleteThreadAsync_DeletesThread_WhenThreadExists()
    {
        // Arrange
        var thread = new ForumThread { Id = 1, UserId = "user123" };
        mockThreadRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(thread);
        mockThreadRepository.Setup(repo => repo.DeleteAsync(thread.Id)).Returns(Task.CompletedTask);

        // Act
        await forumThreadService.DeleteThreadAsync(1);

        // Assert
        mockThreadRepository.Verify(repo => repo.DeleteAsync(thread.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteThreadAsync_ThrowsAppException_WhenThreadNotFound()
    {
        // Arrange
        mockThreadRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((ForumThread?)null);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => forumThreadService.DeleteThreadAsync(1));
    }
}
