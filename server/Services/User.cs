using mao.GameLogic;
using mao.Hubs;

namespace mao.Services;

public class User {
    public IGameHub Connection { get; }
    public string Name { get; }

    public User(IGameHub connection, string name) {
        Connection = connection;
        Name = name;
    }
}