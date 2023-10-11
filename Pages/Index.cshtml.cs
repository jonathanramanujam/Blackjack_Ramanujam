using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Blackjack.Models;
using Blackjack;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        #region Properties

        [BindProperty]
        public Stack<Card> Deck { get; set; }
        [BindProperty]
        public Card Card { get; set; }
        [BindProperty]
        public List<Card> PlayerHand { get; set; }
        [BindProperty]
        public List<Card> DealerHand { get; set; }

        [BindProperty]
        public bool IsPlayersTurn { get; set; }
        [BindProperty]
        public bool IsInProgress { get; set; }
        [BindProperty]
        public bool IsBlackjack { get; set; }

        [BindProperty]
        public int PlayerTotal { get; set; }
        [BindProperty]
        public int DealerTotal { get; set; }
        [BindProperty]
        public string GameState { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if the game is in progress.
        /// If so, check if it is the player's turn, and retrieve the current hands.
        /// Otherwise, deal a new hand.
        /// </summary>
        public void OnGet()
        {
            IsInProgress = HttpContext.Session.GetObjectFromJson<bool>("_IsInProgress");
            IsPlayersTurn = HttpContext.Session.GetObjectFromJson<bool>("_IsPlayersTurn");
            GameState = HttpContext.Session.GetString("_GameState");
            if (IsInProgress)
            {
                PlayerHand = HttpContext.Session.GetObjectFromJson<List<Card>>("_PlayerHand");
                DealerHand = HttpContext.Session.GetObjectFromJson<List<Card>>("_DealerHand");
            }
            else
            {
                OnPostDealHand();
                CheckForBlackjack();
            }
            CalculateTotals();
            CheckWinCondition();
        }

        /// <summary>
        /// Reset the game and deal a new hand.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostDealHand()
        {

            Deck = BuildDeck();
            PlayerHand = new List<Card>();
            DealerHand = new List<Card>();
            var Random = new Random();
            int num = 0;
            for (int i = 0; i < 2; i++)
            {
                PlayerHand.Add(Deck.Pop());
                DealerHand.Add(Deck.Pop());
            }
            IsPlayersTurn = true;
            IsInProgress = true;
            HttpContext.Session.SetObjectAsJson("_PlayerHand", PlayerHand);
            HttpContext.Session.SetObjectAsJson("_DealerHand", DealerHand);
            HttpContext.Session.SetObjectAsJson("_Deck", Deck);
            HttpContext.Session.SetObjectAsJson("_IsPlayersTurn", IsPlayersTurn);
            HttpContext.Session.SetObjectAsJson("_IsInProgress", IsInProgress);
            return Redirect("/Index");
        }

        /// <summary>
        /// Draw a card depending on who's turn it is.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostDrawCard()
        {
            Deck = HttpContext.Session.GetObjectFromJson<Stack<Card>>("_Deck");
            PlayerHand = HttpContext.Session.GetObjectFromJson<List<Card>>("_PlayerHand");
            DealerHand = HttpContext.Session.GetObjectFromJson<List<Card>>("_DealerHand");
            IsPlayersTurn = HttpContext.Session.GetObjectFromJson<bool>("_IsPlayersTurn");
            var rand = new Random();
            if (IsPlayersTurn)
            {
                PlayerHand.Add(Deck.Pop());
                HttpContext.Session.SetObjectAsJson("_PlayerHand", PlayerHand);
            }
            else
            {
                DealerHand.Add(Deck.Pop());
                HttpContext.Session.SetObjectAsJson("_DealerHand", DealerHand);
            }
            HttpContext.Session.SetObjectAsJson("_Deck", Deck);
            return Redirect("/Index");
        }

        /// <summary>
        /// The player has chosen to stay with their hand. Shifts to the dealer's turn.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostStay()
        {
            IsPlayersTurn = false;
            HttpContext.Session.SetObjectAsJson("_IsPlayersTurn", IsPlayersTurn);

            return Redirect("/Index");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Build a fresh deck of 52 cards.
        /// </summary>
        /// <returns></returns>
        private Stack<Card> BuildDeck()
        {
            int cardNum = 0;
            List<Card> deck = new List<Card>();
            for (int i = 2; i < 15; i++)
            {
                string rank = i.ToString();
                switch (i)
                {
                    case 11: rank = "Jack"; break;
                    case 12: rank = "Queen"; break;
                    case 13: rank = "King"; break;
                    case 14: rank = "Ace"; break;
                }
                deck.Add(new Card("Hearts", rank));
                deck.Add(new Card("Diamonds", rank));
                deck.Add(new Card("Clubs", rank));
                deck.Add(new Card("Spades", rank));
            }
            return Shuffle(deck);
        }

        /// <summary>
        /// Shuffle a fresh deck and add to a new stack.
        /// </summary>
        /// <param name="newDeck">A fresh deck to be shuffled</param>
        /// <returns></returns>
        private Stack<Card> Shuffle(List<Card> newDeck)
        {
            Stack<Card> shuffledDeck = new Stack<Card>();
            var rand = new Random();
            int num;
            for (int i = 0; newDeck.Count > 0; i++)
            {
                num = rand.Next(0, newDeck.Count);
                shuffledDeck.Push(newDeck.ElementAt(num));
                newDeck.RemoveAt(num);
            }
            return shuffledDeck;
        }

        /// <summary>
        /// Calculate the current valid maximum total for both players.
        /// </summary>
        private void CalculateTotals()
        {
            int aces = 0;
            foreach (Card card in DealerHand)
            {
                DealerTotal += card.Value;
                if (card.Rank == "Ace")
                {
                    aces++;
                }
                while (DealerTotal > 21 && aces > 0)
                {
                    DealerTotal -= 10;
                    aces--;
                }
            }
            aces = 0;
            foreach (Card card in PlayerHand)
            {
                PlayerTotal += card.Value;
                if (card.Rank == "Ace")
                {
                    aces++;
                }
                while (PlayerTotal > 21 && aces > 0)
                {
                    PlayerTotal -= 10;
                    aces--;
                }
            }
        }

        /// <summary>
        /// Checks to see if a win condition has been met.
        /// </summary>
        private void CheckWinCondition()
        {
            GameState = "";
            if (IsBlackjack)
            {
                GameState = "You got a Blackjack! You win!";
                IsInProgress = false;
                IsPlayersTurn = false;
                HttpContext.Session.SetObjectAsJson("_IsPlayersTurn", IsPlayersTurn);
            }
            else if (IsPlayersTurn)
            {
                if (PlayerTotal > 21)
                {
                    GameState = "Bust! You Lose.";
                    IsInProgress = false;
                }
            }
            else
            {
                if (DealerTotal > 21)
                {
                    GameState = "You Win!";
                    IsInProgress = false;
                }
                else if (DealerTotal > PlayerTotal)
                {
                    GameState = "You Lose.";
                    IsInProgress = false;
                }
            }
            HttpContext.Session.SetString("_GameState", GameState);
            HttpContext.Session.SetObjectAsJson("_IsInProgress", IsInProgress);
        }

        /// <summary>
        /// Checks the player's starting hand for a blackjack.
        /// </summary>
        private void CheckForBlackjack()
        {
            int total = 0;
            foreach (Card card in PlayerHand)
            {
                total += card.Value;
            }
            if (total == 21)
            {
                IsBlackjack = true;
            }
            else
            {
                IsBlackjack = false;
            }
        }

        #endregion
    }
}