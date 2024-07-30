using mao.GameLogic;

namespace mao.Hubs;

public interface IGameHub {
    public Task WriteMessage(string message);
    public Task<string> RequestCard(CancellationToken cancellationToken);
    public Task LobbyUsersUpdate(string message);
    public Task StartGame(List<string> hand);
    public Task HandUpdate(List<string> hand);
    public Task PlayedCardUpdate(string card);
}