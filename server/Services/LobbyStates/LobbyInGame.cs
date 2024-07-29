using System.Text.RegularExpressions;
using mao.GameLogic;
using mao.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace mao.Services.LobbyStates;

public class LobbyInGame : ILobbyState {
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

        await _lobby.Group.WriteMessage($"Game started for {_lobby.Name}!");

        _gameState = new GameState(_lobby.Users);

        foreach (PlayerState player in _gameState.Players) {
            IGameHub connection = _lobby.Users[player.ConnectionId].Connection;
            await connection.StartGame(player.Hand);
        }

        _ = Task.Run(GameLoop);
    }

    public async Task GameLoop() {
        if (_gameState == null || _lobby.Group == null) {
            return;
        }

        while (true) {
            PlayerState? currentPlayer = (PlayerState?)_gameState.Players[_gameState.CurrentPlayerIndex];
            if (currentPlayer == null) {
                return;
            }

            User currentUser = _lobby.Users[currentPlayer.ConnectionId];
            IGameHub userConnection = currentUser.Connection;

            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(5 * 1000);
            try {
                int x = await userConnection.RequestCard(cancellationTokenSource.Token);
                await _lobby.Group.WriteMessage($"{currentUser.Name} played the card {x}");
            } catch {
                await _lobby.Group.WriteMessage($"No input received from {currentUser.Name}");
            } finally {
                cancellationTokenSource.Dispose();
            }

            _gameState.EndTurn();

            await Task.Delay(500);
        }

    }
}