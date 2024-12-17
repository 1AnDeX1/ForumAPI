using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;

namespace WebApp.Tests.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        this._mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        this._mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(),
            null!, null!, null!, null!);

        this._mockConfiguration = new Mock<IConfiguration>();
        this._mockLogger = new Mock<ILogger<AuthService>>();
        this._mockMapper = new Mock<IMapper>();

        this._authService = new AuthService(
            this._mockUserManager.Object,
            this._mockRoleManager.Object,
            this._mockConfiguration.Object,
            this._mockLogger.Object,
            this._mockMapper.Object);
    }

    [Fact]
    public async Task Registeration_ShouldReturnSuccess_WhenUserIsValid()
    {
        // Arrange
        var model = new RegistrationModel { UserName = "testuser", Password = "Test@123" };
        var role = "User";
        var user = new ApplicationUser { UserName = model.UserName };

        _ = this._mockUserManager.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync((ApplicationUser)null!);
        _ = this._mockMapper.Setup(x => x.Map<ApplicationUser>(model)).Returns(user);
        _ = this._mockUserManager.Setup(x => x.CreateAsync(user, model.Password)).ReturnsAsync(IdentityResult.Success);
        _ = this._mockRoleManager.Setup(x => x.RoleExistsAsync(role)).ReturnsAsync(true);
        _ = this._mockUserManager.Setup(x => x.AddToRoleAsync(user, role)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await this._authService.Registeration(model, role);

        // Assert
        Assert.Equal(1, result.Item1);
        Assert.Equal("User created successfully!", result.Item2);
    }

    [Fact]
    public async Task Registeration_ShouldReturnFailure_WhenUserExists()
    {
        // Arrange
        var model = new RegistrationModel { UserName = "testuser", Password = "Test@123" };
        var role = "User";
        var user = new ApplicationUser { UserName = model.UserName };

        _ = this._mockUserManager.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync(user);

        // Act
        var result = await this._authService.Registeration(model, role);

        // Assert
        Assert.Equal(0, result.Item1);
        Assert.Equal("User with this name already exists", result.Item2);
    }

    [Fact]
    public async Task Registeration_ShouldReturnFailure_WhenModelIsNull()
    {
        // Act
        var result = await this._authService.Registeration(null!, "User");

        // Assert
        Assert.Equal(0, result.Item1);
        Assert.Equal("User is empty", result.Item2);
    }
    [Fact]
    public async Task Login_ShouldReturnToken_WhenLoginIsValid()
    {
        // Arrange
        var model = new LoginModel { UserName = "testuser", Password = "Test@123" };
        var user = new ApplicationUser { UserName = model.UserName, Email = "test@example.com", Id = "123" };
        var roles = new List<string> { "User" };

        // Mock configuration for JWT settings
        _ = this._mockConfiguration.Setup(x => x["JWT:Secret"]).Returns("zODHJfezY37VcTBaxb1hMybR6qEnwcdh");
        _ = this._mockConfiguration.Setup(x => x["JWT:ValidIssuer"]).Returns("issuer");
        _ = this._mockConfiguration.Setup(x => x["JWT:ValidAudience"]).Returns("audience");

        // Mock UserManager
        _ = this._mockUserManager.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync(user);
        _ = this._mockUserManager.Setup(x => x.CheckPasswordAsync(user, model.Password)).ReturnsAsync(true);
        _ = this._mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);

        // Act
        var result = await this._authService.Login(model);

        // Assert
        Assert.Equal(1, result.Item1);
        Assert.NotNull(result.Item2); // Ensure token is generated
        Assert.Equal(model.UserName, result.Item3); // Ensure username matches
    }


    [Fact]
    public async Task Login_ShouldReturnFailure_WhenUserNameNotFound()
    {
        // Arrange
        var model = new LoginModel { UserName = "nonexistentuser", Password = "Test@123" };

        _ = this._mockUserManager.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await this._authService.Login(model);

        // Assert
        Assert.Equal(0, result.Item1);
        Assert.Equal("No such username", result.Item2);
        Assert.Null(result.Item3);
    }

    [Fact]
    public async Task Login_ShouldReturnFailure_WhenPasswordIsIncorrect()
    {
        // Arrange
        var model = new LoginModel { UserName = "testuser", Password = "WrongPassword" };
        var user = new ApplicationUser { UserName = model.UserName, Email = "test@example.com", Id = "123" };

        _ = this._mockUserManager.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync(user);
        _ = this._mockUserManager.Setup(x => x.CheckPasswordAsync(user, model.Password)).ReturnsAsync(false);

        // Act
        var result = await this._authService.Login(model);

        // Assert
        Assert.Equal(0, result.Item1);
        Assert.Equal("Invalid password", result.Item2);
        Assert.Null(result.Item3);
    }

    [Fact]
    public async Task Login_ShouldReturnFailure_WhenModelIsNull()
    {
        // Act
        var result = await this._authService.Login(null!);

        // Assert
        Assert.Equal(0, result.Item1);
        Assert.Equal("User is empty", result.Item2);
        Assert.Null(result.Item3);
    }
}
