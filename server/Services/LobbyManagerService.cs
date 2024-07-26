using System.Collections.Concurrent;
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

    public async Task JoinLobby(HubCallerContext hubCallerContext, string lobbyName, string playerName)
    {
        string connectionId = hubCallerContext.ConnectionId;
        IGameHub connection = _hubContext.Clients.Client(connectionId);

        if (hubCallerContext.Items.ContainsKey(_lobbyKey))
        {
            await connection.WriteMessage("Already in a lobby!");
            return;
        }

        Lobby lobby = _lobbies[lobbyName];

        if (lobby.TryAddPlayer(connectionId, playerName, connection))
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, lobbyName);

            await lobby.Group.WriteMessage(string.Join(", ", lobby.PlayerNames));

            hubCallerContext.ConnectionAborted.Register(() => lobby.TryRemovePlayer(connectionId));

            hubCallerContext.Items[_lobbyKey] = lobby;
            lobby.Completed.Register(() => hubCallerContext.Items.Remove(_lobbyKey));
        }
        else
        {
            await connection.WriteMessage("Could not join lobby.");
        }
    }

    public async Task CreateLobby(HubCallerContext hubCallerContext, string lobbyName, string playerName)
    {
        string connectionId = hubCallerContext.ConnectionId;
        IGameHub connection = _hubContext.Clients.Client(connectionId);

        if (lobbyName.Length >= 20 || lobbyName.Length == 0)
        {
            await connection.WriteMessage("Invalid lobby name.");
            return;
        }

        if (_lobbies.ContainsKey(lobbyName))
        {
            await connection.WriteMessage("Lobby already exists with chosen name.");
            return;
        }

        Lobby lobby = new Lobby(lobbyName, _hubContext.Clients.Group(lobbyName));
        _lobbies.TryAdd(lobbyName, lobby);

        lobby.Completed.Register(() => _lobbies.TryRemove(lobbyName, out _));

        await JoinLobby(hubCallerContext, lobbyName, playerName);
    }
}