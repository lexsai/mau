namespace mao.Hubs;

public interface IGameHub {
    public Task WriteMessage(string message);
    public Task GameStarted();
    public Task<int> RequestCard();
    public Task LobbyUsersUpdate(string message);
}