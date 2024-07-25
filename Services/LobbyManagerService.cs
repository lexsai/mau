using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using mao.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace mao.Services;

public class LobbyManagerService
{
    private readonly IHubContext<GameHub, IGameHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<string, Lobby> _lobbies = new();
    private static readonly object _lobbyKey = new();

    public LobbyManagerService(IHubContext<GameHub, IGameHub> hubContext, IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
    }

    public void JoinGame(HubCallerContext hubCallerContext, string lobbyName, string playerName)
    {
        Lobby lobby = _lobbies[lobbyName];

        if (!lobby.TryAddPlayer(hubCallerContext.ConnectionId, playerName)) {
            return;
        }

        hubCallerContext.Items[_lobbyKey] = lobby;

        hubCallerContext.ConnectionAborted.Register(() => lobby.TryRemovePlayer(hubCallerContext.ConnectionId));

        lobby.Completed.Register(() => hubCallerContext.Items.Remove(_lobbyKey));
    }

    public void CreateLobby(HubCallerContext hubCallerContext, string lobbyName, string playerName)
    {
        Regex r = new Regex("^[a-zA-Z0-9 _]*$");
        if (lobbyName.Length >= 20 || lobbyName.Length == 0 || !r.IsMatch(lobbyName))
        {
            return;
        }

        if (_lobbies.ContainsKey(lobbyName))
        {
            return;
        }

        Lobby lobby = new Lobby(lobbyName, _hubContext.Clients.Group(lobbyName));
        _lobbies.TryAdd(lobbyName, lobby);

        JoinGame(hubCallerContext, lobbyName, playerName);
    }
}