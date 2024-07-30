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

    public async Task StartGame() {
        await _lobbyManager.StartGame(Context);
    }

    public async Task Test() {
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.CancelAfter(5 * 1000);
        try {
            string x = await Clients.Client(Context.ConnectionId).RequestCard(cancellationTokenSource.Token);
            await Clients.Client(Context.ConnectionId).WriteMessage($"Card acknowledged: {x}");
        } catch {
            await Clients.Client(Context.ConnectionId).WriteMessage("No input received");
        } finally {
            cancellationTokenSource.Dispose();
        }
    }
}
