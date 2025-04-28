
using System.Collections.Generic;
using UnityEngine;
using PDXUnderground.Core.Cards;

namespace PDXUnderground.Minigames
{
    public class PokerPlayer : MonoBehaviour
    {
        public PokerAction GetAction(int amountToCall)
        {
            if (isAI)
            {
                // Apply vengeance bluff bonus if active
                float modifiedBluffFactor = bluffFactor + vengeanceBluffBonus;
                
                // Check for card sharp special cases
                if (EvaluateHandStrength() >= 8) // Royal flush or justice hand
                {
                    cardSharpMultiplier = 5f;
                }
                
                return GetAIAction(amountToCall);
            gambler = GetComponent<GamblerCharacter>();
            if (gambler != null)
            {
                BuyIn(gambler.CurrentMoney / 2); // Start with half of current money
            }
        }

        public bool BuyIn(int amount)
        {
            if (gambler != null && gambler.TransferMoneyToPoker(amount))
            {
                Chips += amount;
                return true;
            }
            return false;
        }

        public bool CashOut()
        {
            if (gambler != null && Chips > 0)
            {
                gambler.TransferMoneyFromPoker(Chips);
                Chips = 0;
                return true;
            }
            return false;
        }
        public string playerName;
        public int Chips { get; private set; } = 1000;
        public int CurrentBet { get; private set; } = 0;
        public bool IsFolded { get; private set; } = false;
        public List&lt;ICard&gt; HoleCards { get; private set; } = new List&lt;ICard&gt;();
        public HandEvaluationResult HandStrength { get; private set; }

        // AI Personality Settings
        public float aggressionFactor = 0.5f; // 0-1 where 1 is most aggressive
        public float bluffFactor = 0.3f; // 0-1 likelihood to bluff
        
        // Narrative Integration
        public float cardSharpMultiplier = 1f;
        public float vengeanceBluffBonus = 0f;
        public bool knowsChinesePoker = false;

        public void AddCard(ICard card)
        {
            HoleCards.Add(card);
        }

        public void ClearHand()
        {
            HoleCards.Clear();
            IsFolded = false;
            CurrentBet = 0;
        }

        public void PostBlind(int amount)
        {
            CurrentBet = amount;
            Chips -= amount;
        }

        public void EvaluateHand(List&lt;ICard&gt; allCards)
        {
            HandStrength = PokerHandEvaluator.EvaluateHand(allCards);
        }

        public PokerAction GetAction(int amountToCall)
        {
            if (isAI)
            {
                return GetAIAction(amount

