using ChatAPI.Data;
using ChatAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> Login([FromBody] User dto)
    {
        var user = _context.Users.FirstOrDefault(x => x.Username ==  dto.Username);
        if (user == null)
        {
            user = new User { Username = dto.Username };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        return Ok(user);
    }
}