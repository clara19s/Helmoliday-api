namespace HELMoliday.Contracts.Holiday;
public record ChatAuthRequest(
    string SocketId,
    string ChannelName);

public record ChatMessageRequest(
    string? ClientId,
    string Text,
    List<IFormFile>? Images);