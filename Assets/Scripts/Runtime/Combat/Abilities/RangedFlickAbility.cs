using UnityEngine;

namespace PDXUnderground.Combat
{
    public class RangedFlickAbility : MonoBehaviour
    {
        public float cooldownTime { get; private set; }
        private float lastUseTime;
        private float range;
        private float damage;

        public void Initialize(float range, float damage, float cooldown)
        {
            this.range = range;
            this.damage = damage;
            this.cooldownTime = cooldown;
        }

        public void UseAbility()
        {
            // Implement ability logic here
            lastUseTime = Time.time;
            
            // Log for debugging
            Debug.Log($"Ranged Flick ability used with range {range} and damage {damage}");
        }

        public float GetCooldownPercentage()
        {
            float timeElapsed = Time.time - lastUseTime;
            return Mathf.Clamp01(timeElapsed / cooldownTime);
        }
    }
}

