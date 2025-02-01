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

    private readonly Random _random = new();

    public PlayerState? Winner { get; set; }

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
        if (CurrentPlayerIndex + 1 >= Players.Count) {
            CurrentPlayerIndex = 0;
        } else {
            CurrentPlayerIndex++;
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

    public Card? PlayCard(int cardIndex) {
        PlayerState? currentPlayer = (PlayerState?)Players[CurrentPlayerIndex];
        if (currentPlayer == null) {
            return null;
        }

        Card chosenCard = currentPlayer.Hand[cardIndex];
        currentPlayer.Hand.RemoveAt(cardIndex);
        Discard.Push(chosenCard);

        if (Deck.IsEmpty) {
            ShuffleDiscardToDeck();
        }

        if (currentPlayer.Hand.Count == 0) {
            Winner = currentPlayer;
        }

        return chosenCard;
    }
}