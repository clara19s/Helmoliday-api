namespace HELMoliday.Models;
public class ChatImage
{
    public Guid Id { get; set; }
    public string Path { get; set; }
    public ChatMessage Message { get; set; }
}