using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class UserDetailsDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
}