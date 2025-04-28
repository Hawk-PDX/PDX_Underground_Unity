using UnityEngine;

namespace PDXUnderground.Minigames
{
    [CreateAssetMenu(fileName = "PokerRules", menuName = "PDX Underground/Poker Rules")]
    public class PokerRules : ScriptableObject
    {
        [Header("Buy-in Settings")]
        public int minBuyIn = 50;
        public int maxBuyIn = 500;
        public float initialBuyInPercentage = 0.5f;
        [Header("Buy-in Settings")]
        [Tooltip("1 day's wages (~$1 in 1925)")]
        public float minBuyIn = 1.00f;
        [Tooltip("2 week's pay (~$20 in 1925)")]
        public float maxBuyIn = 20.00f;
        [Tooltip("Standard speakeasy buy-in amount")]
        public float initialBuyInPercentage = 0.4f;

        [Header("Blind Structure")] 
        [Tooltip("Quarter (standard speakeasy stakes)")]
        public float smallBlind = 0.25f;
        [Tooltip("50-cent (standard speakeasy stakes)")] 
        public float bigBlind = 0.50f;
        public AnimationCurve bluffCurve;
        [Tooltip("How much AI difficulty increases per won hand")]
        public float difficultyRamp = 0.05f;

        [Header("Visual Settings")]
        public float chipMoveDuration = 0.75f;
        public float chipStackHeight = 0.05f;
    }
}

