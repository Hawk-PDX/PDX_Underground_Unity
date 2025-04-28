
using UnityEngine;
using TMPro;

namespace PDXUnderground
{
    /// <summary>
    /// Simple health system for test dummies
    /// Used for testing the Gambler's abilities
    /// </summary>
    public class TestDummyHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        
        [SerializeField] private TextMeshPro damageText;
        [SerializeField] private ParticleSystem hitEffect;
        
        /// <summary>
        /// Apply damage to the test dummy and show visual feedback
        /// </summary>
        public void TakeDamage(float amount, bool isCritical = false)
        {
            // Apply damage
            currentHealth -= amount;
            currentHealth = Mathf.Max(0, currentHealth);
            
            // Show damage text
            if (damageText != null)
            {
                damageText.text = isCritical ? $"CRIT! {amount}" : amount.ToString();
                damageText.color = isCritical ? Color.red : Color.white;
                
                // Animate text
                StartCoroutine(AnimateDamageText());
            }
            
            // Play hit effect
            if (hitEffect != null)
            {
                hitEffect.Play();
            }
            
            // Log for debugging
            Debug.Log($"Dummy took {amount} damage. Health: {currentHealth}/{maxHealth}");
            
            // Check if destroyed
            if (currentHealth <= 0)
            {
                OnDeath();
            }
        }
        
        /// <summary>
        /// Handle dummy death
        /// </summary>
        private void OnDeath()
        {
            // Just log for testing purposes
            Debug.Log("Test dummy defeated!");
            
            // In a real implementation, we might play a death animation or particle effect
        }
        
        /// <summary>
        /// Animate the damage text
        /// </summary>
        private System.Collections.IEnumerator AnimateDamageText()
        {
            // Show text
            damageText.gameObject.SetActive(true);
            
            // Animate up
            Vector3 startPos = damageText.transform.localPosition;
            Vector3 endPos = startPos + Vector3.up * 0.5f;
            float duration = 0.8f;
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                // Move up
                damageText.transform.localPosition = Vector3.Lerp(startPos, endPos, t / duration);
                
                // Fade out at the end
                if (t > duration * 0.5f)
                {
                    float alpha = 1 - ((t - (duration * 0.5f)) / (duration * 0.5f));
                    damageText.alpha = alpha;
                }
                
                yield return null;
            }
            
            // Reset
            damageText.gameObject.SetActive(false);
            damageText.alpha = 1f;
            damageText.transform.localPosition = startPos;
        }
    }
}

