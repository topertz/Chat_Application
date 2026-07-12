namespace ChatAPI.Models;
public class ChatMessage
{
    public string User { get; set; } = "";

    public string Text { get; set; } = "";

    public string Time { get; set; } = "";
    public bool IsMine { get; set; }
    public string? FileUrl { get; set; }
    public bool IsImage =>
    !string.IsNullOrEmpty(FileUrl) &&
    (FileUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
     FileUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
     FileUrl.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
     FileUrl.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
     FileUrl.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
     FileUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase));
}