namespace mao.GameLogic;

public class Card {
    public CardSuit Suit { get; }
    public CardRank Rank { get; }

    public Card(CardSuit suit, CardRank rank) {
        Suit = suit;
        Rank = rank;
    }

    public string Name() {
        return $"{Rank.ToString().ToLower()} of {Suit.ToString().ToLower()}";
    }

    public override string ToString()
    {
        string rankString;
        if ((int)Rank >= 1 && (int)Rank < 10) {
            rankString = (((int)Rank) + 1).ToString();
        } else {
            rankString = Rank.ToString().ToLower();
        }
        return $"{Suit.ToString().ToLower()}_{rankString}";
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

