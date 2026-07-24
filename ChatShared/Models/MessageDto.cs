using System.IO;

namespace ChatShared.Models
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Sender { get; set; } = "";
        public string Receiver { get; set; } = "";
        public string Text { get; set; } = "";
        public string? FileUrl { get; set; }
        public DateTime SentAt { get; set; }
        public string Time => SentAt.ToString("HH:mm");
        public bool IsMine { get; set; }
        public bool HasFile =>
            !string.IsNullOrWhiteSpace(FileUrl);
        public bool IsImage =>
            HasFile &&
            (
                FileUrl!.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                FileUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                FileUrl.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                FileUrl.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                FileUrl.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                FileUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)
            );
    }
}