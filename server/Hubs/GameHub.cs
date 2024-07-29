using mao.Services;
using Microsoft.AspNetCore.SignalR;

namespace mao.Hubs;

public class GameHub: Hub<IGameHub> {
    private readonly LobbyManagerService _lobbyManager;

    public GameHub(LobbyManagerService lobby) {
        _lobbyManager = lobby;
    }

    public async Task JoinLobby(string lobbyName, string userName) {
        await _lobbyManager.JoinLobby(Context, lobbyName, userName);
    }

    public async Task CreateLobby(string lobbyName, string userName) {
        await _lobbyManager.CreateLobby(Context, lobbyName, userName);
    }

    public async Task<int> Test() {
        int x = await Clients.Client(Context.ConnectionId).RequestCard();
        await Clients.Client(Context.ConnectionId).WriteMessage($"card acknowledged: {x}");
        return x;
    }
}
