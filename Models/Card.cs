namespace Blackjack.Models
{
    public class Card
    {
        public string Suit { get; set; }
        public string Rank { get; set; }
        public int Value { get; set; }
        public string Icon { get; set; }

        public Card()
        {
            Suit = string.Empty;
            Rank = string.Empty;
            Icon = string.Empty;
            Value = 0;
        }

        public Card(string suit, string rank)
        {
            Suit = suit;
            Rank = rank;
            if (rank == "J" || rank == "Q" || rank == "K")
            {
                Value = 10;
            }
            else if (rank == "A")
            {
                Value = 11;
            }
            else
            {
                Value = int.Parse(rank);
            }
            switch (suit) 
            {
                case "Hearts":
                    Icon = "images/suit-heart-fill.svg";
                    break;
                case "Diamonds":
                    Icon = "images/suit-diamond-fill.svg";
                    break;
                case "Spades":
                    Icon = "images/suit-spade-fill.svg";
                    break;
                case "Clubs":
                    Icon = "images/suit-club-fill.svg";
                    break;

            }
        }
    }
}
