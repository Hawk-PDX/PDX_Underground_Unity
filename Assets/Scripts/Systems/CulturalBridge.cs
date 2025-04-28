
using System.Collections.Generic;
using UnityEngine;

namespace PDXUnderground.Systems
{
    public class CulturalBridge : MonoBehaviour
    {
        [Header("Medicine Recipes")]
        public List<string> chineseHerbs = new() { "Ginseng", "Goji Berry", "Agarwood" };
        public float medicineHealAmount = 35f;

        [Header("Language Settings")] 
        public float bilingualBonus = 0.2f; // 20% better deals

        public void ApplyHerbalMedicine()
        {
            var gambler = FindObjectOfType<GamblerCharacter>();
            gambler.Heal(medicineHealAmount);
            
            // Special Agarwood effect
            if (chineseHerbs.Contains("Agarwood"))
            {
                gambler.AddStatBoost("defense", 15, true, 300f, "Agarwood");
            }
        }

        public float GetBilingualPriceAdjustment(bool isChineseVendor)
        {
            return isChineseVendor ? 1f - bilingualBonus : 1f;
        }

        public void TeachEnglish()
        {
            // Improve relations with immigrant NPCs
            Debug.Log("Teaching English to Chinese community");
        }
    }
}

