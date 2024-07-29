using mao.GameLogic;

namespace mao.Hubs;

public interface IGameHub {
    public Task WriteMessage(string message);
    public Task<int> RequestCard(CancellationToken cancellationToken);
    public Task LobbyUsersUpdate(string message);
    public Task StartGame(List<Card> hand);
}