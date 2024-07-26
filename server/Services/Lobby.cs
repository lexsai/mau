using System.Collections.Concurrent;
using mao.Hubs;

namespace mao.Services;

public class Lobby
{
    public IGameHub Group { get; }
    public string Name { get; }

    private readonly ConcurrentDictionary<string, Player> _players = new();

    private readonly CancellationTokenSource _completedCts = new();

    public List<string> PlayerNames { get => _players.Values.Select(p => p.Name).ToList(); }

    public CancellationToken Completed => _completedCts.Token;

    public Lobby(string name, IGameHub group)
    {
        Name = name;
        Group = group;
    }

    public bool TryAddPlayer(string connectionId, string playerName, IGameHub connection)
    {
        if (_players.ContainsKey(connectionId) || PlayerNames.Contains(playerName))
        {
            return false;
        }

        if (_players.TryAdd(connectionId, new Player(connection, playerName))) {
            return true;
        }

        return false;
    }

    public bool TryRemovePlayer(string connectionId)
    {
        return _players.TryRemove(connectionId, out _);
    }
}