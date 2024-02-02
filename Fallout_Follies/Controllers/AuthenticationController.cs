using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName, 
            LastName = model.LastName, 
                                       
        };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            return Ok();
        }
        return BadRequest(result.Errors);
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Message = "Authentication successful",
                Token = token
            });
        }

        return Unauthorized(new { Message = "Authentication failed" });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email)
    }
        .Union(userClaims)
        .Union(roleClaims);

        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(6),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }


}
