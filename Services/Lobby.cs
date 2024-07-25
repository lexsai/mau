using System.Collections.Concurrent;
using mao.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace mao.Services;

public class Lobby
{
    private IGameHub Group { get; }
    private string Name { get; }
    private readonly ConcurrentDictionary<string, Player> _players = new();

    private readonly CancellationTokenSource _completedCts = new();
    public CancellationToken Completed => _completedCts.Token;

    public Lobby(String name, IGameHub group)
    {
        Name = name;
        Group = group;
    }

    public bool TryAddPlayer(String connectionId, String playerName)
    {
        _players.TryAdd(connectionId, new Player() {
            Name = playerName,
        });
        return true;
    }

    public bool TryRemovePlayer(String connectionId)
    {
        return true;
    }
}