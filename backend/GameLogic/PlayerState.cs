using System.Collections;

namespace mao.GameLogic;

public class PlayerState {
    public List<Card> Hand { get; } = new();
    public string ConnectionId { get; }
    public ArrayList DemandedPhrases { get; } = ArrayList.Synchronized(new ArrayList());

    public PlayerState(string connectionId) {
        ConnectionId = connectionId;
    }
}