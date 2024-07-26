using mao.Hubs;

public class Player {
    public IGameHub Connection { get; init; }
    public string Name { get; init; }

    public Player(IGameHub connection, string name) {
        Connection = connection;
        Name = name;
    }
}