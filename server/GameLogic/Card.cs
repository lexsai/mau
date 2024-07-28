namespace mao.GameLogic;

public class Card {
    public CardSuit Suit { get; }
    public CardRank Rank { get; }

    public Card(CardSuit suit, CardRank rank) {
        Suit = suit;
        Rank = rank;
    }
}

public enum CardSuit {
    Hearts,
    Diamonds,
    Spades,
    Clubs
}

public enum CardRank {
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
}

