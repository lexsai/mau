using Microsoft.AspNetCore.SignalR;

namespace mao.Services.LobbyStates;

public class LobbyFinished : ILobbyState {
    private readonly LobbyService _lobby;

    public LobbyFinished(LobbyService lobby) {
        _lobby = lobby;
    }

    public async Task JoinGame(HubCallerContext hubCallerContext, string userName) {
        await _lobby.Group.WriteMessage("Can't join because lobby is finished.");
    }

    public async Task SendChat(HubCallerContext hubCallerContext, string message) {
        await _lobby.Group.WriteMessage("send chat.");
    }

    public async Task StartGame() {
        await _lobby.Group.WriteMessage("Game has already started and finished.");
    }

    public async Task OnStateChange() {
        if (_lobby.Winner == null) {
            await _lobby.Group.WriteMessage("Finished with no winner.");
        } else {
            await _lobby.Group.WriteMessage($"Finished. The winner was {_lobby.Winner.Name}");
        }

        await Task.Delay(10000);
        _lobby.CompletedCts.Cancel();
    }
}