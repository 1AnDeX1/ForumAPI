using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.BusinessLogic.Models.PostReplyModels;
using WebApp.DataAccess.Entities;
using WebApp.WebApi.Controllers;

namespace WebApp.Tests.Controllers;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]

public class PostsControllerTests
{
    private readonly Mock<IPostService> _mockPostService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<PostsController>> _mockLogger;
    private readonly PostsController _controller;
    private readonly ApplicationUser _testUser;

    public PostsControllerTests()
    {
        this._mockPostService = new Mock<IPostService>();
        var userStore = Mock.Of<IUserStore<ApplicationUser>>(); // Mock IUserStore
        this._mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore, null!, null!, null!, null!, null!, null!, null!, null!);
        this._mockLogger = new Mock<ILogger<PostsController>>();
        this._testUser = new ApplicationUser { Id = "123", UserName = "testuser" };

        this._controller = new PostsController(this._mockPostService.Object, this._mockUserManager.Object, this._mockLogger.Object);
    }

    #region GetPosts

    [Fact]
    public async Task GetPosts_ReturnsOkResult_WhenPostsFound()
    {
        // Arrange
        int threadId = 1;
        int page = 1;
        int pageSize = 10;
        var posts = new List<PostModel>
    {
        new PostModel { Id = 1, Content = "Post 1", Created = DateTime.Now, ThreadId = threadId },
        new PostModel { Id = 2, Content = "Post 2", Created = DateTime.Now, ThreadId = threadId }
    };
        var postsCount = posts.Count;

        // Setup the mock service to return the posts and the count
        _ = this._mockPostService.Setup(service => service.GetPostsByThreadIdAsync(threadId, page, pageSize))
            .ReturnsAsync((posts, postsCount));

        // Act
        var result = await this._controller.GetPosts(threadId, page, pageSize);

        // Assert
        var actionResult = Assert.IsType<ActionResult<(IEnumerable<PostModel>, int)>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        // Extract the anonymous object
        var returnValue = okResult.Value;

        // Access postsCount and posts using reflection or dynamic casting
        var postsCountProp = returnValue?.GetType().GetProperty("postsCount");
        var postsProp = returnValue?.GetType().GetProperty("posts");

        // Ensure properties are not null
        Assert.NotNull(postsCountProp);
        Assert.NotNull(postsProp);

        var returnedPostsCount = (int)postsCountProp.GetValue(returnValue);
        var returnedPosts = (IEnumerable<PostModel>)postsProp.GetValue(returnValue);

        // Check if the posts count is as expected
        Assert.Equal(postsCount, returnedPostsCount);

        // Check if the posts are returned correctly
        Assert.Equal(posts.Count, returnedPosts?.Count());
        Assert.Equal(posts[0].Content, returnedPosts?.First().Content);
        Assert.Equal(posts[1].Content, returnedPosts?.Last().Content);
    }

    [Fact]
    public async Task GetPosts_ReturnsNotFound_WhenNoPostsFound()
    {
        // Arrange
        var threadId = 1;
        _ = this._mockPostService.Setup(x => x.GetPostsByThreadIdAsync(threadId, 1, 10))
                        .ReturnsAsync((new List<PostModel>(), 0));  // Empty list, 0 count

        // Act
        var result = await this._controller.GetPosts(threadId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<(IEnumerable<PostModel> posts, int postsCount)>>(result); // Correct type assertion
        var notFoundResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Check for NotFoundResult
    }



    #endregion

    #region AddPost

    [Fact]
    public async Task AddPost_ReturnsCreatedAtAction_WhenPostAdded()
    {
        // Arrange
        var threadId = 1;
        var postModel = new PostCreateModel { Content = "New Post" };
        var createdPost = new PostModel { Id = 1, Content = "New Post" };

        _ = this._mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(this._testUser);
        _ = this._mockPostService.Setup(x => x.AddPostAsync(threadId, this._testUser.Id, postModel))
                        .ReturnsAsync(createdPost);

        // Act
        var result = await this._controller.AddPost(threadId, postModel);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PostModel>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal("GetPosts", createdAtActionResult.ActionName); // or adjust this based on your actual action name
        Assert.Equal(threadId, createdAtActionResult?.RouteValues?["threadId"]);
        Assert.Equal(createdPost, createdAtActionResult?.Value);
    }


    [Fact]
    public async Task AddPost_ReturnsBadRequest_WhenFailedToAddPost()
    {
        // Arrange
        var threadId = 1;
        var postModel = new PostCreateModel { Content = "New Post" };
        _ = this._mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testUser);
        _ = this._mockPostService.Setup(x => x.AddPostAsync(threadId, _testUser.Id, postModel))
                        .ReturnsAsync((PostModel)null);

        // Act
        var result = await this._controller.AddPost(threadId, postModel);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PostModel>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("Failed to add post.", badRequestResult.Value);
    }


    #endregion

    #region UpdatePost

    [Fact]
    public async Task UpdatePost_ReturnsNoContent_WhenPostUpdated()
    {
        // Arrange
        var postId = 1;
        var postModel = new PostCreateModel { Content = "Updated Post" };
        _ = this._mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(this._testUser);
        _ = this._mockPostService.Setup(x => x.CanUserModifyPost(this._testUser, postId)).ReturnsAsync(true);
        _ = this._mockPostService.Setup(x => x.UpdatePostAsync(postId, this._testUser.Id, postModel))
                .ReturnsAsync(new PostModel { Id = postId, Content = "Updated Post" });

        // Act
        var result = await this._controller.UpdatePost(postId, postModel);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdatePost_ReturnsForbidden_WhenUserNotAuthorized()
    {
        // Arrange
        var postId = 1;
        var postModel = new PostCreateModel { Content = "Updated Post" };
        _ = this._mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(this._testUser);
        _ = this._mockPostService.Setup(x => x.CanUserModifyPost(this._testUser, postId)).ReturnsAsync(false);

        // Act
        var result = await this._controller.UpdatePost(postId, postModel);

        // Assert
        _ = Assert.IsType<ForbidResult>(result);
    }

    #endregion

    #region DeletePost

    [Fact]
    public async Task DeletePost_ReturnsNoContent_WhenPostDeleted()
    {
        // Arrange
        var postId = 1;
        _ = this._mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(this._testUser);
        _ = this._mockPostService.Setup(x => x.CanUserModifyPost(this._testUser, postId)).ReturnsAsync(true);
        _ = this._mockPostService.Setup(x => x.DeletePostAsync(postId)).Returns(Task.CompletedTask);

        // Act
        var result = await this._controller.DeletePost(postId);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePost_ReturnsNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var postId = 1;
        _ = this._mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(this._testUser);
        _ = this._mockPostService.Setup(x => x.CanUserModifyPost(this._testUser, postId)).ReturnsAsync(true);
        _ = this._mockPostService.Setup(x => x.DeletePostAsync(postId)).ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await this._controller.DeletePost(postId);

        // Assert
        _ = Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region GetReplies

    [Fact]
    public async Task GetReplies_ReturnsOk_WhenRepliesFound()
    {
        // Arrange
        var postId = 1;
        var replies = new List<PostReplyModel> { new PostReplyModel { Id = 1, Content = "Reply 1" } };
        _ = this._mockPostService.Setup(x => x.GetRepliesByPostIdAsync(postId)).ReturnsAsync(replies);

        // Act
        var result = await this._controller.GetReplies(postId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<PostReplyModel>>>(result); // Check ActionResult type
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Get the actual result (OkObjectResult)
        var returnValue = Assert.IsType<List<PostReplyModel>>(okResult.Value); // Check that the value inside OkObjectResult is of the expected type

        Assert.NotEmpty(returnValue);  // Verify that the reply list is not empty
    }


    [Fact]
    public async Task GetReplies_ReturnsNotFound_WhenNoRepliesFound()
    {
        // Arrange
        var postId = 1;
        _ = this._mockPostService.Setup(x => x.GetRepliesByPostIdAsync(postId)).ReturnsAsync([]);

        // Act
        var result = await this._controller.GetReplies(postId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<PostReplyModel>>>(result); // Check ActionResult type
        var notFoundResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Check if the result is NotFoundResult
    }
    #endregion
}
