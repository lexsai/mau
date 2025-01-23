using mao.GameLogic;
using mao.Hubs;

namespace mao.Services;

public class User {
    public IGameHub Connection { get; }
    public string Name { get; }
    public string ConnectionId { get; }

    public User(IGameHub connection, string name, string connectionId) {
        Connection = connection;
        Name = name;
        ConnectionId = connectionId;
    }
}