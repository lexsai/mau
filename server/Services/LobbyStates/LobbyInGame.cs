using mao.GameLogic;
using mao.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace mao.Services.LobbyStates;

class LobbyInGame : ILobbyState {
    private readonly LobbyService _lobby;

    private GameState? _gameState;

    public LobbyInGame(LobbyService lobby) {
        _lobby = lobby;
    }

    public async Task JoinGame(HubCallerContext hubCallerContext, string userName) {
        if (_lobby.Group == null) {
            return;
        }

        await _lobby.Group.WriteMessage("Can't join because lobby is in-game.");
    }

    public async Task StartGame() {
        if (_lobby.Group == null) {
            return;
        }

        await _lobby.Group.WriteMessage("Game has already started.");
    }

    public async Task OnStateChange() {
        if (_lobby.Group == null) {
            return;
        }

        _gameState = new GameState(_lobby.Users);

        await _lobby.Group.WriteMessage("Game started!");

        _ = Task.Run(GameLoop);
    }

    public async Task GameLoop() {
        if (_gameState == null) {
            return;
        }

        while (true) {
            PlayerState? currentPlayer = _gameState.CurrentPlayer;
            if (currentPlayer == null) {
                return;
            }

            IGameHub connection = _lobby.Users[currentPlayer.ConnectionId].Connection;
            // int card = await connection.RequestCard();

            _gameState.EndTurn();

            await Task.FromResult(0);
        }
    }
}