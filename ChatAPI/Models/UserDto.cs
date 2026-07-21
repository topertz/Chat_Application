namespace ChatAPI.Models;
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public bool IsOnline { get; set; }
}