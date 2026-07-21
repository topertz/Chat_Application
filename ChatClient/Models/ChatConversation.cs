using System.Collections.ObjectModel;

namespace ChatClient.Models;

public class ChatConversation
{
    public string Username { get; set; } = "";

    public ObservableCollection<ChatMessageDto> Messages { get; set; }
        = new();
}