namespace mao.GameLogic;

public class ChatMessage {
    public string Content { get; }
    public string Sender { get; }
    public long TimeSent { get; }

    public ChatMessage(string content, string sender) {
        Content = content;
        Sender = sender;
        TimeSent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}