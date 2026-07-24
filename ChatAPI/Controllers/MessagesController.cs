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

    [HttpGet("{user1}/{user2}")]
    public async Task<IActionResult> GetMessages(string user1, string user2, int page = 1, int pageSize = 50)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m =>
                (m.Sender!.Username == user1 && 
                m.Receiver!.Username == user2)
                ||
                (m.Sender!.Username == user2 &&
                m.Receiver!.Username == user1)
            )
            .OrderBy(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                Sender = m.Sender!.Username,
                Receiver = m.Receiver!.Username,
                m.Text,
                m.SentAt,
                m.FileUrl
            })
            .ToListAsync();
        return Ok(messages);
    }
}