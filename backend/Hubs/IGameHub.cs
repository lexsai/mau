using mao.GameLogic;

namespace mao.Hubs;

public interface IGameHub {
    public Task WriteMessage(string message);
    public Task<string> RequestCard(CancellationToken cancellationToken);
    public Task LobbyUsersUpdate(IEnumerable<string> message);
    public Task GameStarted(List<string> hand);
    public Task HandUpdate(List<string> hand);
    public Task PlayedCardUpdate(string card);
    public Task TurnUpdate(string lastTurnPlayer);
    public Task NotifyAdmin();
    public Task ChatMessage(ChatMessage message);
}