using ChatAPI.Data;
using ChatAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatAPI.Validators;

namespace ChatAPI.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ChatDbContext _context;
    public UsersController(ChatDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Username
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!PasswordValidator.IsValid(dto.Password))
        {
            return BadRequest(
                "Password must contain at least 8 characters, uppercase, lowercase, number and special character."
            );
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Username == dto.Username);
        if (user == null)
        {
            user = new User
            {
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            return Unauthorized("Wrong password");
        }
        return Ok(user);
    }
}