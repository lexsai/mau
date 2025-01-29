namespace mao.GameLogic;

public class ChatMessage {
    public string Message { get; }
    public long TimeSent { get; }

    public ChatMessage(string message) {
        Message = message;
        TimeSent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}