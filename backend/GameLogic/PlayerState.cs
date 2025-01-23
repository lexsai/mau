namespace mao.GameLogic;

public class PlayerState {
    public List<Card> Hand { get; } = new();
    public string ConnectionId { get; }

    public PlayerState(string connectionId) {
        ConnectionId = connectionId;
    }
}