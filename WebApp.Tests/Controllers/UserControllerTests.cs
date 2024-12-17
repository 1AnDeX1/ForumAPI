using Moq;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
namespace WebApp.Tests.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            this._userServiceMock = new Mock<IUserService>();
            this._controller = new UserController(this._userServiceMock.Object);
        }

        // Test for GetAllUsers Method (Good and Bad behavior)
        [Fact]
        public async Task GetAllUsers_ShouldReturnOk_WhenUsersExist()
        {
            // Arrange
            var users = new List<UserModel> { new UserModel { Id = "1", UserName = "TestUser" } };
            var usersCount = 1;
            _ = this._userServiceMock.Setup(service => service.GetAllUsersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                            .ReturnsAsync((users, usersCount));

            // Act
            var result = await this._controller.GetAllUsers(null, 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Extract the anonymous object and validate its properties
            var value = okResult.Value;
            Assert.NotNull(value);

            // Use reflection or dynamic typing to verify the properties
            var usersProp = value.GetType().GetProperty("users")?.GetValue(value);
            var usersCountProp = value.GetType().GetProperty("usersCount")?.GetValue(value);

            Assert.Equal(users, usersProp);
            Assert.Equal(usersCount, usersCountProp);
        }


        [Fact]
        public async Task GetAllUsers_ShouldReturnNotFound_WhenNoUsersExist()
        {
            // Arrange
            var usersCount = 0;
            _ = this._userServiceMock.Setup(service => service.GetAllUsersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                            .ReturnsAsync((new List<UserModel>(), usersCount));

            // Act
            var result = await this._controller.GetAllUsers(null, 1, 10);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No users found.", notFoundResult.Value);
        }


        // Test for GetUserById Method (Good and Bad behavior)
        [Fact]
        public async Task GetUserById_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var user = new UserModel { Id = "1", UserName = "TestUser" };
            _ = this._userServiceMock.Setup(service => service.GetUserByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync(user);

            // Act
            var result = await this._controller.GetUserById("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _ = this._userServiceMock.Setup(service => service.GetUserByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync((UserModel)null!);

            // Act
            var result = await this._controller.GetUserById("1");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("User with ID 1 not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenUserUpdatedSuccessfully()
        {
            // Arrange
            var registrationModel = new RegistrationModel { UserName = "UpdatedUser" };
            var updatedRegistrationModel = new RegistrationModel { UserName = "UpdatedUser" };

            // Mock UpdateUserAsync to return a RegistrationModel
            _ = this._userServiceMock
                .Setup(service => service.UpdateUserAsync(It.IsAny<string>(), It.IsAny<RegistrationModel>()))
                .ReturnsAsync(updatedRegistrationModel);  // Return RegistrationModel

            // Act
            var result = await this._controller.UpdateUser("1", registrationModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Validate result
            var resultValue = Assert.IsType<RegistrationModel>(okResult.Value); // Expect RegistrationModel
            Assert.Equal(updatedRegistrationModel.UserName, resultValue.UserName); // Check properties of RegistrationModel
        }




        [Fact]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenServiceThrowsException()
        {
            // Arrange
            var registrationModel = new RegistrationModel { UserName = "InvalidUser" };
            _ = this._userServiceMock.Setup(service => service.UpdateUserAsync(It.IsAny<string>(), It.IsAny<RegistrationModel>()))
                            .ThrowsAsync(new Exception("Error updating user"));

            // Act
            var result = await this._controller.UpdateUser("1", registrationModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var value = JObject.FromObject(badRequestResult.Value);

            Assert.Equal("Error updating user", value["message"].ToString());
        }


        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent_WhenUserDeletedSuccessfully()
        {
            // Arrange
            _ = this._userServiceMock.Setup(service => service.DeleteUserAsync(It.IsAny<string>()))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await this._controller.DeleteUser("1");

            // Assert
            _ = Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _ = this._userServiceMock.Setup(service => service.DeleteUserAsync(It.IsAny<string>()))
                            .ThrowsAsync(new Exception("User not found"));

            // Act
            var result = await this._controller.DeleteUser("1");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("User not found", notFoundResult.Value);
        }
    }
}
