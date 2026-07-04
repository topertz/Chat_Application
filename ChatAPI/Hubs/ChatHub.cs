using ChatAPI.Data;
using ChatAPI.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatAPI.Hubs;

public class ChatHub : Hub
{
    private readonly ChatDbContext _context;
    private static readonly Dictionary<string, string> _users = new();

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }

    public Task Register(string username)
    {
        _users[username] = Context.ConnectionId;
        return Task.CompletedTask;
    }

    public async Task SendMessage(string fromUser, string toUser, string text)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == fromUser);

        if (user == null)
        {
            user = new User { Username = fromUser };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var message = new Message
        {
            Text = text,
            UserId = user.Id,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage",
            fromUser,
            text,
            message.SentAt.ToString("HH:mm"));
    }
}