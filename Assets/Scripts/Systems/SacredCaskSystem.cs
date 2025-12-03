using UnityEngine;
using PDXUnderground.Player;
using System.Linq;
using PDXUnderground.Systems;

namespace PDXUnderground.Systems
{
    public enum WhiskeyQuality { Normal, Rare, Legendary }
    
    public class SacredCaskSystem : MonoBehaviour
    {
        [Header("Agarwood Cask Properties")]
        public WhiskeyQuality currentQuality = WhiskeyQuality.Normal;
        public int chargesRemaining = 3;
        public float cooldownTime = 300f; // 5 minutes
        
        [Header("Healing Effects")]
        public float normalHeal = 20f;
        public float rareHeal = 40f; 
        public float legendaryHeal = 75f;
        
        private float lastUseTime;

        public bool CanUseCask()
        {
            return chargesRemaining > 0 && 
                   Time.time - lastUseTime > cooldownTime;
        }

        public void UseCask()
        {
            if (!CanUseCask()) return;
            
            chargesRemaining--;
            lastUseTime = Time.time;
            
            float healAmount = currentQuality switch {
                WhiskeyQuality.Rare => rareHeal,
                WhiskeyQuality.Legendary => legendaryHeal,
                _ => normalHeal
            };
            
            FindObjectOfType<GamblerCharacter>().Heal(healAmount);
            
            if (currentQuality == WhiskeyQuality.Legendary)
            {
                ActivatePengMemory();
            }
        }

        private void ActivatePengMemory()
        {
            // Apply +25% accuracy and defense for 2 minutes
            var gambler = FindObjectOfType<GamblerCharacter>();
            gambler.AddStatBoost("accuracy", 25, true, 120f, "PengBuff");
            gambler.AddStatBoost("defense", 25, true, 120f, "PengBuff");
        }
    }
}

