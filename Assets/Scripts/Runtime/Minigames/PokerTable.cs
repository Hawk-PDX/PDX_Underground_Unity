using System.Collections.Generic;
using UnityEngine;
using PDXUnderground.Core.Cards;

namespace PDXUnderground.Minigames
{
    public enum PokerGameState
    {
        Lobby,
        PreFlop,
        Flop,
        Turn,
        River,
        Showdown,
        EndRound
    }

    public enum PokerAction
    {
        Fold,
        Check,
        Call,
        Bet,
        Raise
    }

    public class PokerTable : MonoBehaviour
    {
        [Header("Game Settings")]
        public int smallBlind = 5;
        public int bigBlind = 10;
        public int maxPlayers = 6;
        public float aiDecisionDelay = 1.5f;

        [Header("References")]
        public PokerUI pokerUI;
        public List&lt;PokerPlayer&gt; players = new List&lt;PokerPlayer&gt;();
        public List&lt;ICard&gt; communityCards = new List&lt;ICard&gt;();
        
        private PokerGameState currentState = PokerGameState.Lobby;
        private int dealerPosition = 0;
        private int currentPot = 0;
        private int currentBet = 0;
        private Deck pokerDeck;
        
        private void Start()
        {
            pokerDeck = new Deck();
            InitializeTable();
        }

        private void InitializeTable()
        {
            // Create a standard poker deck (no jokers)
            pokerDeck.InitializeStandardDeck();
            pokerDeck.Shuffle();
        }

        public void StartNewHand()
        {
            if (players.Count &lt; 2) 
            {
                Debug.LogError("Need at least 2 players to start a hand");
                return;
            }

            // Reset previous hand state
            communityCards.Clear();
            currentPot = 0;
            currentBet = 0;
            
            // Post blinds
            PostBlinds();
            
            // Deal cards
            DealHoleCards();
            
            // Start pre-flop betting
            currentState = PokerGameState.PreFlop;
            StartBettingRound();
        }

        private void PostBlinds()
        {
            int smallBlindPos = (dealerPosition + 1) % players.Count;
            int bigBlindPos = (dealerPosition + 2) % players.Count;
            
            players[smallBlindPos].PostBlind(smallBlind);
            players[bigBlindPos].PostBlind(bigBlind);
            
            currentBet = bigBlind;
        }

        private void DealHoleCards()
        {
            foreach (var player in players)
            {
                player.ClearHand();
                player.AddCard(pokerDeck.DrawCard());
                player.AddCard(pokerDeck.DrawCard());
            }
        }

        private void StartBettingRound()
        {
            int firstToAct = (dealerPosition + 3) % players.Count;
            StartCoroutine(ProcessBettingRound(firstToAct));
        }

        private IEnumerator ProcessBettingRound(int startPosition)
        {
            int currentPlayer = startPosition;
            int lastRaisePosition = -1;
            bool bettingComplete = false;

            while (!bettingComplete)
            {
                PokerPlayer player = players[currentPlayer];
                
                // Skip folded players
                if (player.IsFolded)
                {
                    currentPlayer = (currentPlayer + 1) % players.Count;
                    continue;
                }

                // Get player action
                PokerAction action = player.GetAction(currentBet - player.CurrentBet);
                
                // Process action
                switch (action)
                {
                    case PokerAction.Fold:
                        player.Fold();
                        break;
                    case PokerAction.Check:
                        player.Check();
                        break;
                    case PokerAction.Call:
                        int callAmount = currentBet - player.CurrentBet;
                        player.Call(callAmount);
                        currentPot += callAmount;
                        break;
                    case PokerAction.Bet:
                    case PokerAction.Raise:
                        int raiseAmount = player.GetRaiseAmount(currentBet);
                        player.Raise(raiseAmount);
                        currentBet += raiseAmount;
                        currentPot += raiseAmount;
                        lastRaisePosition = currentPlayer;
                        break;
                }

                // Check for end of betting round
                int nextPlayer = (currentPlayer + 1) % players.Count;
                if (nextPlayer == lastRaisePosition)
                {
                    bettingComplete = true;
                }
                
                currentPlayer = nextPlayer;
                
                // Add delay for AI decisions
                if (player.isAI)
                {
                    yield return new WaitForSeconds(aiDecisionDelay);
                }
            }

            // Proceed to next game state
            AdvanceGameState();
        }

        private void AdvanceGameState()
        {
            switch (currentState)
            {
                case PokerGameState.PreFlop:
                    currentState = PokerGameState.Flop;
                    DealCommunityCards(3);
                    StartBettingRound();
                    break;
                case PokerGameState.Flop:
                    currentState = PokerGameState.Turn;
                    DealCommunityCards(1);
                    StartBettingRound();
                    break;
                case PokerGameState.Turn:
                    currentState = PokerGameState.River;
                    DealCommunityCards(1);
                    StartBettingRound();
                    break;
                case PokerGameState.River:
                    currentState = PokerGameState.Showdown;
                    Showdown();
                    break;
                case PokerGameState.Showdown:
                    currentState = PokerGameState.EndRound;
                    EndHand();
                    break;
            }
        }

        private void DealCommunityCards(int count)
        {
            for (int i = 0; i &lt; count; i++)
            {
                communityCards.Add(pokerDeck.DrawCard());
            }
            
            pokerUI.UpdateCommunityCards(communityCards);
        }

        private void Showdown()
        {
            // Evaluate hands and determine winner
            List&lt;PokerPlayer&gt; activePlayers = players.FindAll(p =&gt; !p.IsFolded);
            
            if (activePlayers.Count == 1)
            {
                // Single player wins by default
                activePlayers[0].WinHand(currentPot);
            }
            else
            {
                // Compare hands
                EvaluateShowdown(activePlayers);
            }
            
            // Update UI with results
            pokerUI.ShowShowdownResults(activePlayers);
        }

        private void EvaluateShowdown(List&lt;PokerPlayer&gt; players)
        {
            // Use our existing hand evaluation logic from PokerHandEvaluator
            foreach (var player in players)
            {
                List&lt;ICard&gt; allCards = new List&lt;ICard&gt;(player.HoleCards);
                allCards.AddRange(communityCards);
                player.EvaluateHand(allCards);
            }
            
            // Sort players by hand strength
            players.Sort((a, b) =&gt; b.HandStrength.CompareTo(a.HandStrength));
            
            // Award pot to winner(s)
            players[0].WinHand(currentPot);
        }

        private void EndHand()
        {
            // Rotate dealer button
            dealerPosition = (dealerPosition + 1) % players.Count;
            
            // Remove bankrupt players
            players.RemoveAll(p =&gt; p.Chips &lt;= 0);
            
            // Start a new hand if enough players remain
            if (players.Count &gt;= 2)
            {
                StartNewHand();
            }
        }
    }
}

