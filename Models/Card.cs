namespace Blackjack.Models
{
    public class Card
    {
        public string Suit { get; set; }
        public string Rank { get; set; }
        public int Value { get; set; }

        public Card()
        {
            Suit = string.Empty;
            Rank = string.Empty;
            Value = 0;
        }

        public Card(string suit, string rank)
        {
            Suit = suit;
            Rank = rank;
            if (rank == "Jack" || rank == "Queen" || rank == "King")
            {
                Value = 10;
            }
            else if (rank == "Ace")
            {
                Value = 11;
            }
            else
            {
                Value = int.Parse(rank);
            }
        }
    }
}
