using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // POST: api/User
    [HttpPost]
    public async Task<IActionResult> CreateUser(UserRegisterDto model)
    {
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        return Ok();
    }

    // PUT: api/User/{email}
    [HttpPut("{email}")]
    public async Task<IActionResult> UpdateUser(string email, UserUpdateDto model)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }

        // Update the user properties
        user.Email = model.Email;
        // Set other properties

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }

    // POST: api/User/ChangeRole
    [HttpPost("ChangeRole")]
    public async Task<IActionResult> ChangeUserRole(UserRoleDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!removeResult.Succeeded)
        {
            return BadRequest(removeResult.Errors);
        }

        var addResult = await _userManager.AddToRoleAsync(user, model.RoleName);

        if (!addResult.Succeeded)
        {
            return BadRequest(addResult.Errors);
        }

        return Ok();
    }

    // Implement other CRUD operations (Get, Delete) as needed
}
