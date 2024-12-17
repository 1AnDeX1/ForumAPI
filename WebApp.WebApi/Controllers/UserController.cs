using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.IServices;
using WebApp.BusinessLogic.Models.UserModels;

namespace WebApp.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
#pragma warning disable SA1404 // Code analysis suppression should have justification
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
#pragma warning restore SA1404 // Code analysis suppression should have justification
public class UserController : ControllerBase
{
    private readonly IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? userName, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (users, usersCount) = await this.userService.GetAllUsersAsync(userName, page, pageSize);

        if (usersCount == 0)
        {
            return this.NotFound("No users found.");
        }

        return this.Ok(new { users, usersCount });
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await this.userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return this.NotFound($"User with ID {id} not found.");
        }

        return this.Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] RegistrationModel registrationModel)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var updatedUser = await this.userService.UpdateUserAsync(id, registrationModel);
            return this.Ok(updatedUser);
        }
        catch (Exception ex)
        {
            return this.BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            await this.userService.DeleteUserAsync(id);
            return this.NoContent();
        }
        catch (Exception ex)
        {
            return this.NotFound(ex.Message);
        }
    }
}
