using System.Collections.Concurrent;
using mao.Hubs;
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
        await _lobbies[lobbyName].JoinGame(hubCallerContext, userName);
    }

    public async Task StartGame(string lobbyName) {
        await _lobbies[lobbyName].StartGame();
    }

    public async Task CreateLobby(HubCallerContext hubCallerContext, string lobbyName, string userName) {
        string connectionId = hubCallerContext.ConnectionId;
        IGameHub connection = _hubContext.Clients.Client(connectionId);

        if (lobbyName.Length >= 20 || lobbyName.Length == 0) {
            await connection.WriteMessage("Invalid lobby name.");
            return;
        }

        if (_lobbies.ContainsKey(lobbyName)) {
            await connection.WriteMessage("Lobby already exists with chosen name.");
            return;
        }

        LobbyService lobby = _serviceProvider.GetRequiredService<LobbyService>();
        lobby.Init(lobbyName);

        _lobbies.TryAdd(lobbyName, lobby);

        lobby.Completed.Register(() => _lobbies.TryRemove(lobbyName, out _));

        await JoinLobby(hubCallerContext, lobbyName, userName);
    }
}