using Microsoft.AspNetCore.SignalR;

namespace mao.Services.LobbyStates;

public interface ILobbyState {
    public Task JoinGame(HubCallerContext hubCallerContext, string userName);
    public Task StartGame();

    public Task OnStateChange();
}