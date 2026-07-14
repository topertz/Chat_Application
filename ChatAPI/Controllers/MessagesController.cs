using ChatAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAPI.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly ChatDbContext _context;

    public MessagesController(ChatDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        var messages = await _context.Messages
            .Include(m => m.User)
            .OrderBy(m => m.SentAt)
            .Select(m => new
            {
                m.Id,
                Username = m.User!.Username,
                m.Text,
                m.SentAt
            })
            .ToListAsync();
        return Ok(messages);
    }
}