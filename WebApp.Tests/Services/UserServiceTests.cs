using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.BusinessLogic;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.IRepository;
using Xunit;

namespace WebApp.Tests.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly IUserService userService;

        public UserServiceTests()
        {
            this.userRepositoryMock = new Mock<IUserRepository>();
            this.userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null
            );
            this.mapperMock = new Mock<IMapper>();
            this.userService = new UserService(
                this.userRepositoryMock.Object,
                this.userManagerMock.Object,
                this.mapperMock.Object
            );
        }

        [Fact]
        public async Task DeleteUserAsync_DeletesUserSuccessfully()
        {
            // Arrange
            var userId = "123";
            var user = new ApplicationUser { Id = userId };
            this.userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            this.userRepositoryMock.Setup(repo => repo.DeleteAsync(userId)).Returns(Task.CompletedTask);

            // Act
            await this.userService.DeleteUserAsync(userId);

            // Assert
            this.userRepositoryMock.Verify(repo => repo.DeleteAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ThrowsExceptionIfUserNotFound()
        {
            // Arrange
            var userId = "999";
            this.userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => this.userService.DeleteUserAsync(userId));
            Assert.Equal($"User with ID {userId} not found.", exception.Message);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsUsersSuccessfully()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", UserName = "User1" },
                new ApplicationUser { Id = "2", UserName = "User2" }
            };
            this.userRepositoryMock
                .Setup(repo => repo.GetUsersAsync(1, 10))
                .ReturnsAsync((users, users.Count));
            this.mapperMock
                .Setup(mapper => mapper.Map<IEnumerable<UserModel>>(users))
                .Returns(users.Select(u => new UserModel { Id = u.Id, UserName = u.UserName }));

            // Act
            var result = await this.userService.GetAllUsersAsync(null, 1, 10);

            // Assert
            Assert.Equal(2, result.users.Count());
            Assert.Equal(2, result.usersCount);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUserIfFound()
        {
            // Arrange
            var userId = "123";
            var user = new ApplicationUser { Id = userId, UserName = "TestUser" };
            this.userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            this.mapperMock.Setup(mapper => mapper.Map<UserModel>(user)).Returns(new UserModel { Id = userId, UserName = "TestUser" });

            // Act
            var result = await this.userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("TestUser", result.UserName);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNullIfUserNotFound()
        {
            // Arrange
            var userId = "999";
            this.userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await this.userService.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesUserSuccessfully()
        {
            // Arrange
            var userId = "123";
            var user = new ApplicationUser { Id = userId, UserName = "OldUser" };
            var registrationModel = new RegistrationModel { UserName = "UpdatedUser", Password = "NewPassword" };

            this.userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            this.userRepositoryMock.Setup(repo => repo.UpdateAsync(user)).ReturnsAsync(user);
            this.mapperMock.Setup(mapper => mapper.Map(registrationModel, user)).Callback(() => user.UserName = registrationModel.UserName);
            this.userManagerMock
                .Setup(manager => manager.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("resetToken");
            this.userManagerMock
                .Setup(manager => manager.ResetPasswordAsync(user, "resetToken", registrationModel.Password))
                .ReturnsAsync(IdentityResult.Success);
            this.mapperMock
                .Setup(mapper => mapper.Map<RegistrationModel>(user))
                .Returns(registrationModel);

            // Act
            var result = await this.userService.UpdateUserAsync(userId, registrationModel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UpdatedUser", result.UserName);
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsExceptionIfUserNotFound()
        {
            // Arrange
            var userId = "999";
            var registrationModel = new RegistrationModel();
            this.userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => this.userService.UpdateUserAsync(userId, registrationModel));
            Assert.Equal($"User with ID {userId} not found.", exception.Message);
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsExceptionIfPasswordUpdateFails()
        {
            // Arrange
            var userId = "123";
            var user = new ApplicationUser { Id = userId };
            var registrationModel = new RegistrationModel { Password = "NewPassword" };
            var errors = new[] { new IdentityError { Description = "Password too weak" } };

            this.userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            this.userManagerMock
                .Setup(manager => manager.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("resetToken");
            this.userManagerMock
                .Setup(manager => manager.ResetPasswordAsync(user, "resetToken", registrationModel.Password))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => this.userService.UpdateUserAsync(userId, registrationModel));
            Assert.Contains("Password too weak", exception.Message);
        }
    }
}
