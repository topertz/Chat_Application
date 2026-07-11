namespace ChatAPI.Models;
public class ChatMessageDto
{
    public string? User { get; set; }

    public string? Receiver { get; set; }

    public string? Text { get; set; }

    public string? Attachment { get; set; }
}