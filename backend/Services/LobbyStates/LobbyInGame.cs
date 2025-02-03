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
    
    public async Task SendChat(HubCallerContext hubCallerContext, string content) { 
        if (_gameState == null) {
            return;
        }

        User user = _lobby.Users[hubCallerContext.ConnectionId];
        
        ChatMessage message = new ChatMessage(content, user.Name);
        _gameState.Messages.Add(message);

        await _lobby.Group.ChatMessage(message);
        
        foreach (PlayerState player in _gameState.Players) {
            if (player.ConnectionId == hubCallerContext.ConnectionId
                && !player.DemandedPhrases.Contains(content)) {
                IssuePenalty(player, "No unnecessary speech.");
            }    
        }
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

    public async Task GameLoop() {
        if (_gameState == null) {
            return;
        }

        Broadcast("Mau is a game of rules.");
        await Task.Delay(3000);

        while (true) {
            PlayerState? currentPlayer = (PlayerState?)_gameState.Players[_gameState.CurrentPlayerIndex];
            if (currentPlayer == null) {
                return;
            }

            User currentUser = _lobby.Users[currentPlayer.ConnectionId];
            IGameHub userConnection = currentUser.Connection;

            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(15000);

            List<Task<(PlayerState, string?)>> cardRequests = new(_gameState.Players.Count);
            foreach (PlayerState player in _gameState.Players) {
                cardRequests.Add(RequestCardWrapper(player, cancellationTokenSource.Token));
            }
            await Task.WhenAll(cardRequests);

            cancellationTokenSource.Dispose();

            int cardIndex = -1;
            foreach ((PlayerState receivedPlayer, string? receivedCard) in cardRequests.Select(r => r.Result)) {
                IGameHub receivedConnection = _lobby.Users[receivedPlayer.ConnectionId].Connection;
                await receivedConnection.TurnUpdate(currentUser.Name);
                if (receivedPlayer.ConnectionId == currentPlayer.ConnectionId) {
                    int chosenCardIndex = -1;
                    bool success = int.TryParse(receivedCard, out chosenCardIndex);
                    await receivedConnection.WriteMessage($"...");
                    if (success && chosenCardIndex < currentPlayer.Hand.Count && chosenCardIndex >= 0) {
                        cardIndex = chosenCardIndex;
                    } else {
                        IssuePenalty(receivedPlayer, "Failure to play a card during their turn.");
                    }
                } else if (receivedCard != "-1" && receivedCard != null && receivedCard != "-2") {
                    await receivedConnection.WriteMessage($"... It was {currentUser.Name}'s turn.");
                    IssuePenalty(receivedPlayer, "Playing a card during another player's turn.");
                } else {
                    await receivedConnection.WriteMessage($"... It was {currentUser.Name}'s turn.");
                }
            }

            if (cardIndex != -1) {
                Card? card = _gameState.PlayCard(cardIndex, CheckMessageSent, IssuePenalty, ReceiveWildcard);
                if (card != null) {
                    await _lobby.Group.PlayedCardUpdate(card.ToString());
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

            await Task.Delay(8000);
            
            int maxDemandedPhrases = 0;
            foreach (PlayerState player in _gameState.Players) {
                if (player.DemandedPhrases.Count > maxDemandedPhrases) {
                    maxDemandedPhrases = player.DemandedPhrases.Count;
                }
            }
            await Task.Delay(5000 * maxDemandedPhrases);
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
    public void CheckMessageSent(PlayerState player, string content) {
        player.DemandedPhrases.Add(content);
        _ = Task.Run(async () => {
            await CheckMessageSentAsync(player, content);
        });
    }

    public async Task CheckMessageSentAsync(PlayerState player, string content) {
        if (_gameState == null) {
            return;
        }

        await Task.Delay(5000);
        if (_gameState.Winner != null) {
            return;
        }
        
        string userName = _lobby.Users[player.ConnectionId].Name;

        bool messageSent = false;
        foreach (ChatMessage message in _gameState.Messages) {
            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - message.TimeSent <= 5000 && 
                message.Sender == userName && message.Content.Trim().ToLower() == content) {
                messageSent = true;
            }
        }

        player.DemandedPhrases.Remove(content);
        if (!messageSent) {
            IssuePenalty(player, $"Failure to say \"{content}\"");

            IGameHub playerConnection = _lobby.Users[player.ConnectionId].Connection;
            await playerConnection.HandUpdate(player.Hand.Select(c => c.ToString()).ToList());
        }
    }
    
    public void IssuePenalty(PlayerState target, string message) {
        if (_gameState == null) {
            return;
        }

        _gameState.Draw(target);
        string targetName = _lobby.Users[target.ConnectionId].Name;
        
        if (!target.DemandedPhrases.Contains("thank you")) {
            CheckMessageSent(target, "thank you");
        }
        
        IGameHub userConnection = _lobby.Users[target.ConnectionId].Connection;
        userConnection.HandUpdate(target.Hand.Select(c => c.ToString()).ToList());

        Broadcast($"+1 penalty card to {targetName}. {message}");
    }

    public void Broadcast(string message) {
        if (_gameState == null) {
            return;
        }

        foreach (PlayerState player in _gameState.Players) {
            IGameHub playerConnection = _lobby.Users[player.ConnectionId].Connection;
            playerConnection.ChatMessage(new ChatMessage(message, "The Dealer"));
        }
    }

    public void ReceiveWildcard(PlayerState player) {
        player.DemandedPhrases.Add("spades");
        player.DemandedPhrases.Add("clubs");
        player.DemandedPhrases.Add("hearts");
        player.DemandedPhrases.Add("diamonds");
        _ = Task.Run(async () => {
            await ReceiveWildcardAsync(player);
        });
    }

    public async Task ReceiveWildcardAsync(PlayerState player) {
        if (_gameState == null) {
            return;
        }

        await Task.Delay(5000);
        if (_gameState.Winner != null) {
            return;
        }
        
        string userName = _lobby.Users[player.ConnectionId].Name;

        ChatMessage? suitMessage = null;
        foreach (ChatMessage message in _gameState.Messages) {
            string messageContent = message.Content.Trim().ToLower();
            bool isSuit = messageContent == "spades"
                          || messageContent == "clubs"
                          || messageContent == "hearts"
                          || messageContent == "diamonds";
            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - message.TimeSent <= 5000 && 
                message.Sender == userName && isSuit) {
                suitMessage = message;
            }
        }
        player.DemandedPhrases.Remove("spades");
        player.DemandedPhrases.Remove("clubs");
        player.DemandedPhrases.Remove("hearts");
        player.DemandedPhrases.Remove("diamonds");
        if (suitMessage == null) {
            Broadcast("Time elapsed to declare a new suit-- you can declare a new suit after playing a jack.");
        } else {
            Card? card;
            switch (suitMessage.Content.Trim().ToLower()) {
                case "spades":
                    card = new Card(CardSuit.Spades, CardRank.Jack);
                    break;
                case "clubs":
                    card = new Card(CardSuit.Clubs, CardRank.Jack);
                    break;
                case "hearts":
                    card = new Card(CardSuit.Hearts, CardRank.Jack);
                    break;
                case "diamonds":
                    card = new Card(CardSuit.Diamonds, CardRank.Jack);
                    break;
                default:
                    card = null;
                    break;
            }
            if (card != null) {
                _gameState.Discard.Push(card);
                await _lobby.Group.PlayedCardUpdate(card.ToString());
            }
        }
    }
}