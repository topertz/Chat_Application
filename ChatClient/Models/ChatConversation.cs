using ChatAPI.Models;
using System.Collections.ObjectModel;

namespace ChatClient.Models;
public class ChatConversation
{
    public string Username { get; set; } = "";

    public ObservableCollection<ChatMessage> Messages { get; set; }
        = new();
}