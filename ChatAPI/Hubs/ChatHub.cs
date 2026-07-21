using ChatAPI.Data;
using ChatAPI.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace ChatAPI.Hubs;

public class ChatHub : Hub
{
    private readonly ChatDbContext _context;
    private static readonly ConcurrentDictionary<string, string> _users = new();

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }

    public async Task Register(string username)
    {
        _users[username] = Context.ConnectionId;

        await Clients.Caller.SendAsync(
            "OnlineUsers",
            _users.Keys.ToList()
        );

        await Clients.All.SendAsync(
            "UserConnected",
            username
        );
    }

    public async Task SendMessage(string fromUser, string toUser, string text, string? file)
    {
        var sender = await _context.Users
            .FirstOrDefaultAsync(x => x.Username == fromUser);

        if (sender == null)
            throw new Exception("Sender not found.");

        var receiver = await _context.Users
            .FirstOrDefaultAsync(x => x.Username == toUser);

        if (receiver == null)
            throw new Exception("Receiver not found.");

        var message = new Message
        {
            SenderId = sender.Id,
            ReceiverId = receiver.Id,
            Text = text,
            SentAt = DateTime.UtcNow,
            FileUrl = file
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var time = message.SentAt.ToString("HH:mm");

        await SendToUser(
            toUser,
            fromUser,
            text,
            time,
            file);

        await SendToUser(
            fromUser,
            fromUser,
            text,
            time,
            file);
    }

    private async Task SendToUser(
    string username,
    string fromUser,
    string text,
    string time,
    string? file)
    {
        if (_users.TryGetValue(username, out var connectionId))
        {
            await Clients.Client(connectionId)
                .SendAsync(
                    "ReceiveMessage",
                    fromUser,
                    text,
                    time,
                    file
                );
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _users
            .FirstOrDefault(x => x.Value == Context.ConnectionId);

        if (!string.IsNullOrEmpty(user.Key))
        {
            _users.TryRemove(user.Key, out _);

            await Clients.All.SendAsync(
                "UserDisconnected",
                user.Key
           );
        }

        await base.OnDisconnectedAsync(exception);
    }
}