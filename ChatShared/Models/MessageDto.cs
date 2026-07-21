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
    }
}
