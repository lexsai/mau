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
        await _lobby.Group.WriteMessage("Can't join because lobby is in-game.");
    }

    public async Task StartGame() {
        await _lobby.Group.WriteMessage("Game has already started.");
    }
    
    public async Task SendChat(HubCallerContext hubCallerContext, string message) {
        _gameState?.Messages.Add(new ChatMessage(message));
        await _lobby.Group.ChatMessage(message);
    }

    public async Task OnStateChange() {
        await _lobby.Group.WriteMessage($"Game started for {_lobby.Name}!");

        _gameState = new GameState(_lobby.Users);

        foreach (PlayerState player in _gameState.Players) {
            IGameHub connection = _lobby.Users[player.ConnectionId].Connection;
            await connection.GameStarted(player.Hand.Select(c => c.ToString()).ToList());
        }

        _ = Task.Run(GameLoop);
    }

    public async Task CheckMessageSent(PlayerState player) {
        if (_gameState == null) {
            return;
        }

        await Task.Delay(5000);
        
        bool messageSent = false;
        foreach (ChatMessage message in _gameState.Messages) {
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - message.TimeSent <= 5) {
                messageSent = true;
            }
        }

        if (!messageSent) {
            _gameState.Draw(player);
            _gameState.Draw(player);
            _gameState.Draw(player);
            _gameState.Draw(player);
            _gameState.Draw(player);

            IGameHub playerConnection = _lobby.Users[player.ConnectionId].Connection;
            await playerConnection.HandUpdate(player.Hand.Select(c => c.ToString()).ToList());
            await playerConnection.WriteMessage($"+1 card. Failure to say 'Have a nice day'.");
        }
    }

    public async Task GameLoop() {
        if (_gameState == null) {
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
            cancellationTokenSource.CancelAfter(6000);

            List<Task<(PlayerState, string?)>> cardRequests = new(_gameState.Players.Count);
            foreach (PlayerState player in _gameState.Players) {
                cardRequests.Add(RequestCardWrapper(player, cancellationTokenSource.Token));
            }
            await Task.WhenAll(cardRequests);

            cancellationTokenSource.Dispose();

            int cardIndex = -1;
            foreach ((PlayerState receivedPlayer, string? receivedCard) in cardRequests.Select(r => r.Result)) {
                IGameHub receivedConnection = _lobby.Users[receivedPlayer.ConnectionId].Connection;
                if (receivedPlayer.ConnectionId == currentPlayer.ConnectionId) {
                    int chosenCardIndex = -1;
                    bool success = int.TryParse(receivedCard, out chosenCardIndex);
                    if (success && chosenCardIndex < currentPlayer.Hand.Count && chosenCardIndex >= 0) {
                        cardIndex = chosenCardIndex;
                        await receivedConnection.WriteMessage($"...");
                    } else {
                        _gameState.Draw(receivedPlayer);
                        _gameState.Draw(receivedPlayer);
                        await receivedConnection.WriteMessage($"+1 card. It was your turn to play a card.");
                    }
                } else if (receivedCard != "-1") {
                    _gameState.Draw(receivedPlayer);
                    await receivedConnection.WriteMessage($"+1 card. It was {currentUser.Name}'s turn to play a card.");
                } else {
                    await receivedConnection.WriteMessage($"... It was {currentUser.Name}'s turn.");
                }
            }

            if (cardIndex != -1) {
                Card? card = _gameState.PlayCard(cardIndex);
                if (card == null) {
                    return;
                }
                await _lobby.Group.PlayedCardUpdate(card.ToString());
                if (card.Rank == CardRank.Seven) {
                    _ = Task.Run(async () => {
                        await CheckMessageSent(currentPlayer);
                    });
                }
            }

            foreach (PlayerState player in _gameState.Players) {
                IGameHub playerConnection = _lobby.Users[player.ConnectionId].Connection;
                await playerConnection.HandUpdate(player.Hand.Select(c => c.ToString()).ToList());
            }

            if (_gameState.Winner != null) {
                _lobby.Winner = _lobby.Users[_gameState.Winner.ConnectionId];
                await _lobby.ChangeState(_lobby.FinishedState);
                return;
            }

            _gameState.EndTurn();

            await Task.Delay(3000);
        }
    }

    private async Task<(PlayerState, string?)> RequestCardWrapper(PlayerState player, CancellationToken cancellationToken) {
        IGameHub userConnection = _lobby.Users[player.ConnectionId].Connection;
        try {
            await userConnection.WriteMessage("A new turn begins. Select a card to play or pass.");
            string card = await userConnection.RequestCard(cancellationToken);
            return (player, card);
        }
        catch {
            return (player, null);
        }
    }
}