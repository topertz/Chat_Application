namespace ChatAPI.Models; 

public class Message
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string? FileUrl { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}