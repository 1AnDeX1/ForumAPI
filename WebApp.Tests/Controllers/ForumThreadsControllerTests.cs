using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApp.WebApi.Controllers;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.ThreadModels;
using WebApp.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace WebApp.Tests.Controllers;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
public class ForumThreadsControllerTests
{
    private readonly Mock<IForumThreadService> mockThreadService;
    private readonly Mock<UserManager<ApplicationUser>> mockUserManager;
    private readonly Mock<ILogger<ForumThreadsController>> mockLogger;
    private readonly ForumThreadsController controller;

    public ForumThreadsControllerTests()
    {
        this.mockThreadService = new Mock<IForumThreadService>();
        this.mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        this.mockLogger = new Mock<ILogger<ForumThreadsController>>();

        this.controller = new ForumThreadsController(
            this.mockThreadService.Object,
            this.mockUserManager.Object,
            this.mockLogger.Object
        );
    }

    [Fact]
    public async Task GetAllThreads_ReturnsOk_WhenThreadsFound()
    {
        // Arrange
        var threads = new List<ForumThreadModel> { new ForumThreadModel { Id = 1, Title = "Test Thread" } };
        _ = this.mockThreadService.Setup(s => s.GetAllThreadsAsync(It.IsAny<string>(), 1, 20))
                         .ReturnsAsync((threads, threads.Count));

        // Act
        var result = await this.controller.GetAllThreads(null, 1, 20);

        // Assert
        var actionResult = Assert.IsType<ActionResult<(IEnumerable<ForumThreadModel>, int)>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetAllThreads_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _ = this.mockThreadService.Setup(s => s.GetAllThreadsAsync(It.IsAny<string>(), 1, 20))
                         .ThrowsAsync(new Exception("Internal error"));

        // Act
        var result = await this.controller.GetAllThreads(null, 1, 20);

        // Assert
        var actionResult = Assert.IsType<ActionResult<(IEnumerable<ForumThreadModel>, int)>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
    [Fact]
    public async Task GetThreadById_ReturnsOk_WhenThreadFound()
    {
        // Arrange
        var thread = new ForumThreadModel { Id = 1, Title = "Test Thread" };
        _ = this.mockThreadService.Setup(s => s.GetThreadByIdAsync(1)).ReturnsAsync(thread);

        // Act
        var result = await this.controller.GetThreadById(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ForumThreadModel>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetThreadById_ReturnsNotFound_WhenThreadNotFound()
    {
        // Arrange
        _ = this.mockThreadService.Setup(s => s.GetThreadByIdAsync(1)).ReturnsAsync((ForumThreadModel)null!);

        // Act
        var result = await this.controller.GetThreadById(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ForumThreadModel>>(result);
        var notFoundResult = Assert.IsType<NotFoundResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetThreadById_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _ = this.mockThreadService.Setup(s => s.GetThreadByIdAsync(1)).ThrowsAsync(new Exception("Internal error"));

        // Act
        var result = await this.controller.GetThreadById(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ForumThreadModel>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
    [Fact]
    public async Task CreateThread_ReturnsCreated_WhenThreadCreated()
    {
        // Arrange
        var threadCreateModel = new ForumThreadCreateModel { Title = "Test Thread", Content = "Test content" };
        var createdThread = new ForumThreadModel { Id = 1, Title = "Test Thread", Content = "Test content" };
        _ = this.mockThreadService.Setup(s => s.CreateThreadAsync(It.IsAny<ForumThreadCreateModel>(), It.IsAny<string>()))
                         .ReturnsAsync(createdThread);
        _ = this.mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser { Id = "userId" });

        // Act
        var result = await this.controller.CreateThread(threadCreateModel);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ForumThreadModel>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(201, createdAtActionResult.StatusCode);
    }

    [Fact]
    public async Task CreateThread_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        this.controller.ModelState.AddModelError("Title", "Required");
        var threadCreateModel = new ForumThreadCreateModel();

        // Act
        var result = await this.controller.CreateThread(threadCreateModel);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ForumThreadModel>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateThread_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var threadCreateModel = new ForumThreadCreateModel { Title = "Test Thread", Content = "Test content" };
        _ = this.mockThreadService.Setup(s => s.CreateThreadAsync(It.IsAny<ForumThreadCreateModel>(), It.IsAny<string>()))
                         .ThrowsAsync(new Exception("Internal error"));
        _ = this.mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser { Id = "userId" });

        // Act
        var result = await this.controller.CreateThread(threadCreateModel);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ForumThreadModel>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
    [Fact]
    public async Task UpdateThread_ReturnsNoContent_WhenThreadUpdated()
    {
        // Arrange
        var threadCreateModel = new ForumThreadCreateModel { Title = "Updated Thread", Content = "Updated content" };
        _ = this.mockThreadService.Setup(s => s.UpdateThreadAsync(1, threadCreateModel))
                         .ReturnsAsync(new ForumThreadModel { Id = 1, Title = "Test Thread", Content = "Test Content" });
        _ = this.mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser { Id = "userId" });
        _ = this.mockThreadService.Setup(s => s.CanUserModifyThread(It.IsAny<ApplicationUser>(), 1)).ReturnsAsync(true);

        // Act
        var result = await this.controller.UpdateThread(1, threadCreateModel);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task UpdateThread_ReturnsForbidden_WhenUserNotOwner()
    {
        // Arrange
        var threadCreateModel = new ForumThreadCreateModel { Title = "Updated Thread", Content = "Updated content" };
        _ = this.mockThreadService.Setup(s => s.UpdateThreadAsync(1, threadCreateModel))
                         .ReturnsAsync(new ForumThreadModel { Id = 1, Title = "Test Thread", Content = "Test Content" });
        _ = this.mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                       .ReturnsAsync(new ApplicationUser { Id = "otherUserId" });

        // Act
        var result = await this.controller.UpdateThread(1, threadCreateModel);

        // Assert
        _ = Assert.IsType<ForbidResult>(result);
    }
}
