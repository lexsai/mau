using System.Collections;
using System.Collections.Concurrent;
using mao.Services;

namespace mao.GameLogic;

public class GameState {
    public ConcurrentStack<Card> Deck { get; set; } = new();
    public ArrayList Players { get; } = ArrayList.Synchronized(new ArrayList());

    public int CurrentPlayerIndex { get; private set; }

    private readonly Random _random = new();

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
        Deck.Clear();
        foreach (CardRank rank in Enum.GetValues<CardRank>()) {
            foreach (CardSuit suit in Enum.GetValues<CardSuit>()) {
                Deck.Push(new Card(suit, rank));
            }
        }
        Deck.OrderBy(x => _random.Next());
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
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
    }
}