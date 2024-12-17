using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.BusinessLogic;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;
using WebApp.BusinessLogic.Models.PostReplyModels;

namespace WebApp.Tests.Services;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
public class PostServiceTests
{
    private readonly Mock<IPostRepository> postRepositoryMock;
    private readonly Mock<IPostReplyRepository> postReplyRepositoryMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
    private readonly Mock<ILogger<PostService>> loggerMock;
    private readonly PostService postService;

    public PostServiceTests()
    {
        this.postRepositoryMock = new Mock<IPostRepository>();
        this.postReplyRepositoryMock = new Mock<IPostReplyRepository>();
        this.mapperMock = new Mock<IMapper>();
        this.loggerMock = new Mock<ILogger<PostService>>();
        this.userManagerMock = MockUserManager<ApplicationUser>();

        this.postService = new PostService(
            this.postRepositoryMock.Object,
            this.postReplyRepositoryMock.Object,
            this.mapperMock.Object,
            this.userManagerMock.Object,
            this.loggerMock.Object
        );
    }

    private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        return new Mock<UserManager<TUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task GetPostsByThreadIdAsync_ShouldReturnPosts_WhenPostsExist()
    {
        // Arrange
        var threadId = 1;
        var page = 1;
        var pageSize = 10;
        var posts = new List<Post> { new Post { Id = 1, ThreadId = threadId } };
        _ = this.postRepositoryMock.Setup(repo => repo.GetPostsByThreadIdAsync(threadId, page, pageSize))
            .ReturnsAsync((posts, posts.Count));
        _ = this.mapperMock.Setup(mapper => mapper.Map<IEnumerable<PostModel>>(posts))
            .Returns([new PostModel { Id = 1 }]);

        // Act
        var result = await this.postService.GetPostsByThreadIdAsync(threadId, page, pageSize);

        // Assert
        _ = Assert.Single(result.posts);
        Assert.Equal(1, result.postsCount);
        this.postRepositoryMock.Verify(repo => repo.GetPostsByThreadIdAsync(threadId, page, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetPostsByThreadIdAsync_ShouldThrowException_WhenNoPostsFound()
    {
        // Arrange
        var threadId = 1;
        var page = 1;
        var pageSize = 10;
        _ = this.postRepositoryMock.Setup(repo => repo.GetPostsByThreadIdAsync(threadId, page, pageSize))
            .ReturnsAsync((Enumerable.Empty<Post>(), 0));

        // Act & Assert
        _ = await Assert.ThrowsAsync<AppException>(() => this.postService.GetPostsByThreadIdAsync(threadId, page, pageSize));
        this.postRepositoryMock.Verify(repo => repo.GetPostsByThreadIdAsync(threadId, page, pageSize), Times.Once);
    }
    [Fact]
    public async Task AddPostAsync_ShouldCreatePost_WhenValid()
    {
        // Arrange
        var threadId = 1;
        var userId = "user123";
        var postCreateModel = new PostCreateModel { Content = "Test Content" };
        var post = new Post { Content = "Test Content", ThreadId = threadId, UserId = userId };
        var createdPost = new Post { Id = 1, Content = "Test Content" };

        _ = this.mapperMock.Setup(mapper => mapper.Map<Post>(postCreateModel)).Returns(post);
        _ = this.postRepositoryMock.Setup(repo => repo.AddAsync(post)).ReturnsAsync(createdPost);
        _ = this.mapperMock.Setup(mapper => mapper.Map<PostModel>(createdPost)).Returns(new PostModel { Id = 1 });

        // Act
        var result = await this.postService.AddPostAsync(threadId, userId, postCreateModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        this.postRepositoryMock.Verify(repo => repo.AddAsync(post), Times.Once);
    }

    [Fact]
    public async Task AddPostAsync_ShouldThrowException_WhenPostInvalid()
    {
        // Arrange
        var threadId = 1;
        var userId = "user123";
        var postCreateModel = new PostCreateModel { Content = "" }; // Invalid content

        // Act & Assert
        _ = await Assert.ThrowsAsync<AppException>(() => this.postService.AddPostAsync(threadId, userId, postCreateModel));
    }
    [Fact]
    public async Task DeletePostAsync_ShouldDeletePost_WhenValidId()
    {
        // Arrange
        var postId = 1;
        var post = new Post { Id = postId };
        var replies = new List<PostReply> { new PostReply { Id = 1, PostId = postId } };

        _ = this.postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync(post);
        _ = this.postReplyRepositoryMock.Setup(repo => repo.GetRepliesByPostIdAsync(postId)).ReturnsAsync(replies);

        // Act
        await this.postService.DeletePostAsync(postId);

        // Assert
        this.postReplyRepositoryMock.Verify(repo => repo.DeleteAsync(1), Times.Once);
        this.postRepositoryMock.Verify(repo => repo.DeleteAsync(postId), Times.Once);
    }

    [Fact]
    public async Task DeletePostAsync_ShouldThrowException_WhenPostNotFound()
    {
        // Arrange
        var postId = 1;
        _ = this.postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync((Post?)null);

        // Act & Assert
        _ = await Assert.ThrowsAsync<AppException>(() => this.postService.DeletePostAsync(postId));
    }
    [Fact]
    public async Task CanUserModifyPost_ShouldReturnTrue_WhenUserIsAdmin()
    {
        // Arrange
        var user = new ApplicationUser { Id = "admin123" };
        var postId = 1;
        var post = new Post { Id = postId, UserId = "user123" };

        _ = this.postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync(post);
        _ = this.userManagerMock.Setup(manager => manager.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

        // Act
        var result = await this.postService.CanUserModifyPost(user, postId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserModifyPost_ShouldReturnFalse_WhenUserNotAuthorized()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user123" };
        var postId = 1;
        var post = new Post { Id = postId, UserId = "otherUser" };

        _ = this.postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync(post);
        _ = this.userManagerMock.Setup(manager => manager.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

        // Act
        var result = await this.postService.CanUserModifyPost(user, postId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdatePostAsync_ValidPost_ReturnsUpdatedPostModel()
    {
        // Arrange
        var postId = 1;
        var userId = "user123";
        var postModel = new PostCreateModel
        {
            Content = "Updated Content"
        };

        var existingPost = new Post
        {
            Id = postId,
            Content = "Old Content",
            UserId = userId
        };

        _ = this.postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId))
            .ReturnsAsync(existingPost);

        _ = this.mapperMock.Setup(m => m.Map(postModel, existingPost))
            .Callback<PostCreateModel, Post>((src, dest) =>
            {
                dest.Content = src.Content;
            });

        _ = this.mapperMock.Setup(m => m.Map<PostModel>(existingPost))
            .Returns(new PostModel { Id = postId, Content = "Updated Content" });

        // Act
        var result = await this.postService.UpdatePostAsync(postId, userId, postModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(postId, result.Id);
        Assert.Equal(postModel.Content, result.Content);

        this.postRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Post>()), Times.Once);
    }



    [Fact]
    public async Task UpdatePostAsync_PostNotFound_ThrowsAppException()
    {
        // Arrange
        var postId = 1;
        var userId = "userId";
        var postCreateModel = new PostCreateModel { Content = "Updated Content" };

        _ = this.postRepositoryMock.Setup(repo => repo.GetByIdAsync(postId)).ReturnsAsync((Post)null!);

        // Act & Assert
        _ = await Assert.ThrowsAsync<AppException>(() => this.postService.UpdatePostAsync(postId, userId, postCreateModel));
    }


    [Fact]
    public async Task GetRepliesByPostIdAsync_ValidPostId_ReturnsReplies()
    {
        // Arrange
        var postId = 1;
        var replies = new List<PostReply>
        {
            new PostReply { Id = 1, Content = "Reply 1" },
            new PostReply { Id = 2, Content = "Reply 2" }
        };
        _ = this.postReplyRepositoryMock.Setup(repo => repo.GetRepliesByPostIdAsync(postId)).ReturnsAsync(replies);
        _ = this.mapperMock.Setup(mapper => mapper.Map<IEnumerable<PostReplyModel>>(replies)).Returns(
            [
                new PostReplyModel { Content = "Reply 1" },
                new PostReplyModel { Content = "Reply 2" }
            ]);

        // Act
        var result = await this.postService.GetRepliesByPostIdAsync(postId);

        // Assert
        Assert.NotEmpty(result);
        this.postReplyRepositoryMock.Verify(repo => repo.GetRepliesByPostIdAsync(postId), Times.Once);
    }


    [Fact]
    public async Task GetRepliesByPostIdAsync_NoRepliesFound_ReturnsEmptyList()
    {
        // Arrange
        var postId = 1;
        _ = this.postReplyRepositoryMock.Setup(repo => repo.GetRepliesByPostIdAsync(postId)).ReturnsAsync([]);

        // Act
        var result = await this.postService.GetRepliesByPostIdAsync(postId);

        // Assert
        Assert.Empty(result);
        this.postReplyRepositoryMock.Verify(repo => repo.GetRepliesByPostIdAsync(postId), Times.Once);
    }


    [Fact]
    public async Task AddReplyAsync_ValidReply_ReturnsPostReplyModel()
    {
        // Arrange
        var postId = 1;
        var userId = "userId";
        var replyCreateModel = new PostReplyCreateModel { Content = "Reply Content" };
        var newReply = new PostReply { Id = 1, PostId = postId, UserId = userId, Content = replyCreateModel.Content };
        var createdReply = new PostReply { Id = 1, PostId = postId, UserId = userId, Content = "Reply Content" };

        _ = this.postReplyRepositoryMock.Setup(repo => repo.AddAsync(newReply)).ReturnsAsync(createdReply);
        _ = this.mapperMock.Setup(mapper => mapper.Map<PostReply>(replyCreateModel)).Returns(newReply);
        _ = this.mapperMock.Setup(mapper => mapper.Map<PostReplyModel>(createdReply)).Returns(new PostReplyModel());

        // Act
        var result = await this.postService.AddReplyAsync(postId, userId, replyCreateModel);

        // Assert
        Assert.NotNull(result);
        this.postReplyRepositoryMock.Verify(repo => repo.AddAsync(newReply), Times.Once);
    }


    [Fact]
    public async Task UpdateReplyAsync_ValidReply_ReturnsUpdatedPostReplyModel()
    {
        // Arrange
        var replyId = 1;
        var userId = "user123";
        var replyModel = new PostReplyCreateModel
        {
            Content = "Updated Reply Content"
        };

        var existingReply = new PostReply
        {
            Id = replyId,
            Content = "Old Reply Content",
            PostId = 1,
            UserId = userId
        };

        var post = new Post
        {
            Id = 1,
            Content = "Sample Content"
        };

        _ = this.postRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(post);

        _ = this.postReplyRepositoryMock.Setup(repo => repo.GetByIdAsync(replyId))
            .ReturnsAsync(existingReply);

        _ = this.mapperMock.Setup(m => m.Map(replyModel, existingReply))
            .Callback<PostReplyCreateModel, PostReply>((src, dest) =>
            {
                dest.Content = src.Content;
            });

        _ = this.mapperMock.Setup(m => m.Map<PostReplyModel>(existingReply))
            .Returns(new PostReplyModel
            {
                Id = replyId,
                Content = "Updated Reply Content"
            });

        // Act
        var result = await this.postService.UpdateReplyAsync(replyId, userId, replyModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(replyId, result.Id);
        Assert.Equal(replyModel.Content, result.Content);

        this.postReplyRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<PostReply>()), Times.Once);
    }




    [Fact]
    public async Task UpdateReplyAsync_ReplyNotFound_ThrowsAppException()
    {
        // Arrange
        var replyId = 1;
        var userId = "userId";
        var replyCreateModel = new PostReplyCreateModel { Content = "Updated Reply Content" };

        _ = this.postReplyRepositoryMock.Setup(repo => repo.GetByIdAsync(replyId)).ReturnsAsync((PostReply)null!);

        // Act & Assert
        _ = await Assert.ThrowsAsync<AppException>(() => this.postService.UpdateReplyAsync(replyId, userId, replyCreateModel));
    }

    [Fact]
    public async Task DeleteReplyAsync_ValidReply_DeletesSuccessfully()
    {
        // Arrange
        var replyId = 1;
        var reply = new PostReply { Id = replyId, Content = "Reply Content" };
        _ = this.postReplyRepositoryMock.Setup(repo => repo.GetByIdAsync(replyId)).ReturnsAsync(reply);
        _ = this.postReplyRepositoryMock.Setup(repo => repo.DeleteAsync(replyId)).Returns(Task.CompletedTask);

        // Act
        await this.postService.DeleteReplyAsync(replyId);

        // Assert
        this.postReplyRepositoryMock.Verify(repo => repo.DeleteAsync(replyId), Times.Once);
    }

    [Fact]
    public async Task DeleteReplyAsync_ReplyNotFound_ThrowsAppException()
    {
        // Arrange
        var replyId = 1;

        _ = this.postReplyRepositoryMock.Setup(repo => repo.GetByIdAsync(replyId)).ReturnsAsync((PostReply)null!);

        // Act & Assert
        _ = await Assert.ThrowsAsync<AppException>(() => this.postService.DeleteReplyAsync(replyId));
    }
}
