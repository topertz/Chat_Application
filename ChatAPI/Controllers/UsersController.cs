using ChatAPI.Data;
using ChatAPI.Models;
using ChatAPI.Services;
using ChatShared.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAPI.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ChatDbContext _context;
    private readonly JwtService _jwtService;
    public UsersController(ChatDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;

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
        }
        else
        {
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized("Wrong password");
            }
        }

        var token = _jwtService.CreateToken(user.Username);

        return Ok(new
        {
            token,
            username = user.Username
        });
    }
}