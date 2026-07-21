namespace ChatClient.Models;
public class ChatMessageDto
{
    public string Sender { get; set; } = "";
    public string Receiver { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime SentAt { get; set; }
    public string? FileUrl { get; set; }
    public bool IsMine { get; set; }
}