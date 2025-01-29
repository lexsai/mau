using mao.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace mao.Services.LobbyStates;

public class LobbyWaiting : ILobbyState {
    private readonly LobbyService _lobby;

    public LobbyWaiting(LobbyService lobby) {
        _lobby = lobby;
    }

    public async Task JoinGame(HubCallerContext hubCallerContext, string userName) {
        await _lobby.AddUser(hubCallerContext, userName);
    }

    public async Task StartGame() {
        await _lobby.ChangeState(_lobby.InGameState);
    }

    public async Task SendChat(HubCallerContext hubCallerContext, string message) {
        await _lobby.Group.WriteMessage("send chat yup");
    }

    public async Task OnStateChange() {
        await Task.FromResult(0);
    }
}