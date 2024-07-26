namespace mao.Hubs;

public interface IGameHub {
    public Task WriteMessage(string message);
    public Task GameStarted();
}