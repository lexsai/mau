using mao.Services;
using Microsoft.AspNetCore.SignalR;

namespace mao.Hubs;

public class GameHub: Hub<IGameHub>
{
    private readonly LobbyManagerService _lobbyManager;

    public GameHub(LobbyManagerService lobby) {
        _lobbyManager = lobby;
    }

    public async Task JoinLobby(string lobbyName, string playerName)
    {
        await _lobbyManager.JoinLobby(Context, lobbyName, playerName);
    }

    public async Task CreateLobby(string lobbyName, string playerName)
    {
        await _lobbyManager.CreateLobby(Context, lobbyName, playerName);
    }
    public async Task GetPlayerList()
    {
        await Clients.Client(Context.ConnectionId).WriteMessage("hi");
    }
}
