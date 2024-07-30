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
            await connection.StartGame(player.Hand.Select(c => c.ToString()).ToList());
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

            int cardIndex = 0;

            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(5 * 1000);
            try {
                int chosenCardIndex = 0;
                bool success = int.TryParse(await userConnection.RequestCard(cancellationTokenSource.Token), out chosenCardIndex);
                if (success && chosenCardIndex < currentPlayer.Hand.Count && chosenCardIndex >= 0) {
                    cardIndex = chosenCardIndex;
                }
                await _lobby.Group.WriteMessage($"{currentUser.Name} has chosen hand index {cardIndex}");
            } catch {
                await _lobby.Group.WriteMessage($"No input received from {currentUser.Name}");
            } finally {
                cancellationTokenSource.Dispose();
            }

            Card? card = _gameState.PlayCard(cardIndex);
            if (card == null) {
                return;
            }

            await userConnection.HandUpdate(currentPlayer.Hand.Select(c => c.ToString()).ToList());
            await _lobby.Group.PlayedCardUpdate(card.ToString());

            if (_gameState.Winner != null) {
                _lobby.Winner = _lobby.Users[_gameState.Winner.ConnectionId];
                await _lobby.ChangeState(_lobby.FinishedState);
                return;
            }

            _gameState.EndTurn();

            await Task.Delay(3000);
        }
    }
}