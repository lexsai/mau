using System.Collections;
using System.Collections.Concurrent;
using mao.Services;

namespace mao.GameLogic;

public class GameState {
    public ConcurrentStack<Card> Deck { get; set; } = new();
    public ConcurrentStack<Card> Discard { get; set; } = new();

    public ArrayList Players { get; } = ArrayList.Synchronized(new ArrayList());
    public ArrayList Messages { get; } = ArrayList.Synchronized(new ArrayList());

    public int CurrentPlayerIndex { get; private set; }

    public PlayerState? Winner { get; set; }

    private readonly Random _random = new();
    private int _veryNumber = 0;
    private int _order = 1; // 1 for clockwise, -1 for anticlockwise. 
    private bool _doSkip = false;

    public GameState(ConcurrentDictionary<string, User> players) {
        GenerateDeck();
        foreach (User user in players.Values) {
            AddPlayerForUser(user);
        }
        DealToPlayers();
    }

    private void AddPlayerForUser(User user) {
        Players.Add(new PlayerState(user.ConnectionId));
    }

    private void GenerateDeck() {
        List<Card> tmpList = new();
        foreach (CardRank rank in Enum.GetValues<CardRank>()) {
            foreach (CardSuit suit in Enum.GetValues<CardSuit>()) {
                tmpList.Add(new Card(suit, rank));
            }
        }
        Deck.Clear();
        foreach (Card card in tmpList.OrderBy(x => _random.Next())) {
            Deck.Push(card);
        }
    }

    public void ShuffleDiscardToDeck() {
        List<Card> tmpList = new();
        Card? discardCard;
        while (Discard.TryPop(out discardCard)) {
            tmpList.Add(discardCard);
        }
        foreach (Card card in tmpList.OrderBy(x => _random.Next())) {
            Deck.Push(card);
        }
    }

    private void DealToPlayers() {
        Card? topCard;
        foreach (PlayerState player in Players) {
            for (int i = 0; i < 3; i++) {
                if (Deck.TryPop(out topCard)) {
                    player.Hand.Add(topCard);
                }
            }
        }
    }

    public void EndTurn() {
        if (CurrentPlayerIndex + _order >= Players.Count) {
            CurrentPlayerIndex = 0;
        } else if (CurrentPlayerIndex + _order < 0) {
            CurrentPlayerIndex = Players.Count - 1;
        } else {
            CurrentPlayerIndex += _order;
        }
        if (_doSkip) {
            _doSkip = false;
            EndTurn();
        }
    }

    public void Draw(PlayerState player) {
        if (Deck.IsEmpty) {
            GenerateDeck();
        }
        Card? topCard;
        if (Deck.TryPop(out topCard)) {
            player.Hand.Add(topCard);
        }
    }

    public Card? PlayCard(int cardIndex, 
                          Action<PlayerState, string> demandMessage,
                          Action<PlayerState, string> issuePenalty,
                          Action<PlayerState> requestWildcard) {
        PlayerState? currentPlayer = (PlayerState?)Players[CurrentPlayerIndex];
        if (currentPlayer == null) {
            return null;
        }

        Card chosenCard = currentPlayer.Hand[cardIndex];
        
        Card? discardTop;
        if (Discard.TryPeek(out discardTop) 
            && discardTop.Suit != chosenCard.Suit 
            && discardTop.Rank != chosenCard.Rank
            && chosenCard.Rank != CardRank.Jack) {
            issuePenalty(currentPlayer, "Failure to play a valid card.");
            return null;
        }

        currentPlayer.Hand.RemoveAt(cardIndex);
        Discard.Push(chosenCard);

        if (Deck.IsEmpty) {
            ShuffleDiscardToDeck();
        }

        if (currentPlayer.Hand.Count == 0) {
            Winner = currentPlayer;
        }
        
        if (currentPlayer.Hand.Count == 1) {
            demandMessage(currentPlayer, "last card");
        }
        
        if (chosenCard.Rank == CardRank.Seven) {
            string veryText = string.Concat(Enumerable.Repeat(" very", _veryNumber));
            demandMessage(currentPlayer, $"have a{veryText} nice day");
            _veryNumber += 1;
        } else {
            _veryNumber = 0;
        }
        if (chosenCard.Rank == CardRank.Eight) {
            _order *= -1;
        }
        if (chosenCard.Rank == CardRank.King) {
            demandMessage(currentPlayer, "hail to the chief");
            foreach (PlayerState player in Players) {
                if (player != currentPlayer) {
                    demandMessage(player, "all hail");
                }
            }
        }
        if (chosenCard.Rank == CardRank.Ace) {
            _doSkip = true;
        }
        if (chosenCard.Rank == CardRank.Jack) {
            requestWildcard(currentPlayer);
        }
        if (chosenCard.Suit == CardSuit.Spades) {
            demandMessage(currentPlayer, chosenCard.Name());
        }

        return chosenCard;
    }
}