using UnityEngine;
using System.Collections.Generic;

namespace PDXUnderground.Systems
{
    public class BlackMarketEconomy : MonoBehaviour
    {
        [Header("Whiskey Pricing")]
        public float baseWhiskeyPrice = 3.00f; // 1925 average
        public float maxWhiskeyPrice = 8.00f;
        public float raidPriceMultiplier = 1.5f;
        
        [Header("Police Interactions")]
        public float bribePercentage = 0.20f;
        public float raidWarningCost = 1.00f;
        
        private float _currentWhiskeyPrice;
        private bool _raidImminent;
        
        private void Start()
        {
            _currentWhiskeyPrice = baseWhiskeyPrice;
        }
        
        public float GetWhiskeyPrice(bool recentRaid)
        {
            if (recentRaid)
            {
                _currentWhiskeyPrice = Mathf.Min(
                    _currentWhiskeyPrice * raidPriceMultiplier, 
                    maxWhiskeyPrice
                );
            }
            else
            {
                // Gradual price normalization
                _currentWhiskeyPrice = Mathf.Lerp(
                    _currentWhiskeyPrice,
                    baseWhiskeyPrice,
                    Time.deltaTime * 0.1f
                );
            }
            return _currentWhiskeyPrice;
        }
        
        public float CalculateBribeCost(float currentMoney)
        {
            return Mathf.Round(currentMoney * bribePercentage * 100) / 100;
        }
        
        public bool CanAffordRaidWarning(float currentMoney)
        {
            return currentMoney >= raidWarningCost;
        }
        
        public void SetRaidStatus(bool imminent)
        {
            _raidImminent = imminent;
        }
    }
}

