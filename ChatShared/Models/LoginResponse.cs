using System.Text.Json.Serialization;

namespace ChatClient.Models;
public class LoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = "";

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";
}