using System.Collections.Concurrent;
using mao.Hubs;
using mao.Models;
using Microsoft.AspNetCore.SignalR;

namespace mao.Services;

public class LobbyManagerService {
    private readonly IHubContext<GameHub, IGameHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<string, LobbyService> _lobbies = new();

    public static object LobbyKey { get; } = new();

    public LobbyManagerService(IHubContext<GameHub, IGameHub> hubContext, IServiceProvider serviceProvider) {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
    }

    public async Task JoinLobby(HubCallerContext hubCallerContext, string lobbyName, string userName) {
        IGameHub connection = _hubContext.Clients.Client(hubCallerContext.ConnectionId);
        if (_lobbies.ContainsKey(lobbyName)) {
            await _lobbies[lobbyName].JoinGame(hubCallerContext, userName);
        } else {
            await connection.WriteMessage("Tried to join lobby that does not exist.");
        }
    }

    public async Task StartGame(HubCallerContext hubCallerContext) {
        IGameHub connection = _hubContext.Clients.Client(hubCallerContext.ConnectionId);

        if (!hubCallerContext.Items.ContainsKey(LobbyKey)) {
            await connection.WriteMessage("Not in a lobby.");
            return;
        }

        LobbyService? lobby = (LobbyService?)hubCallerContext.Items[LobbyKey];
        if (lobby != null) {
            await lobby.StartGame();
        }
    }


    public async Task SendChat(HubCallerContext hubCallerContext, string message) {
        IGameHub connection = _hubContext.Clients.Client(hubCallerContext.ConnectionId);

        if (!hubCallerContext.Items.ContainsKey(LobbyKey)) {
            await connection.WriteMessage("Not in a lobby.");
            return;
        }

        LobbyService? lobby = (LobbyService?)hubCallerContext.Items[LobbyKey];
        if (lobby != null) {
            await lobby.SendChat(hubCallerContext, message);
        }
    }

    public CreatedLobby CreateLobby() {
        LobbyService lobby = _serviceProvider.GetRequiredService<LobbyService>();
        _lobbies.TryAdd(lobby.Name, lobby);
        lobby.Completed.Register(() => _lobbies.TryRemove(lobby.Name, out _));
        
        return new CreatedLobby {
            Name = lobby.Name
        };
    }

    public bool HasLobby(string lobbyName) {
        return _lobbies.ContainsKey(lobbyName);
    }
}