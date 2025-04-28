using UnityEngine;
using System.Collections;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.Effects
{
    /// <summary>
    /// Types of card effects for visual/audio feedback
    /// </summary>
    public enum CardEffectType
    {
        Attack = 0,
        Defense = 1,
        Utility = 2,
        Special = 3
    }
    
    /// <summary>
    /// Controls visual effects for card use and interactions
    /// </summary>
    public class CardEffectsController : MonoBehaviour
    {
        [Header("Card Effects")]
        [SerializeField] private ParticleSystem cardThrowParticles;
        [SerializeField] private ParticleSystem cardImpactParticles;
        [SerializeField] private ParticleSystem sliceParticles;
        [SerializeField] private ParticleSystem cardEffectParticles;
        
        [Header("Audio")]
        [SerializeField] private AudioSource effectAudioSource;
        [SerializeField] private float effectDuration = 0.5f;
        [SerializeField] private AudioClip cardDrawSound;
        [SerializeField] private AudioClip[] cardUseSound;
        
        private Coroutine activeEffect;
        
        /// <summary>
        /// Play a card throwing effect
        /// </summary>
        public GameObject PlayCardThrowEffect(Vector3 position, Vector3 direction, float speed, int cardType, bool isCritical)
        {
            if (cardThrowParticles != null)
            {
                var effect = Instantiate(cardThrowParticles, position, Quaternion.LookRotation(direction));
                var main = effect.main;
                main.startSpeed = speed;
                
                if (isCritical)
                {
                    effect.transform.localScale *= 1.5f;
                }
                
                effect.Play();
                
                // Play sound effect if available
                if (effectAudioSource != null && cardUseSound != null && cardUseSound.Length > 0)
                {
                    int soundIndex = cardType % cardUseSound.Length;
                    effectAudioSource.PlayOneShot(cardUseSound[soundIndex]);
                }
                
                return effect.gameObject;
            }
            return null;
        }

        /// <summary>
        /// Play a card impact effect
        /// </summary>
        public void PlayCardImpactEffect(Vector3 position, Vector3 normal, bool isCritical)
        {
            if (cardImpactParticles != null)
            {
                var effect = Instantiate(cardImpactParticles, position, Quaternion.LookRotation(normal));
                if (isCritical)
                {
                    effect.transform.localScale *= 1.5f;
                }
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }
        }

        /// <summary>
        /// Play a slicing effect
        /// </summary>
        public void PlaySliceEffect(Vector3 position, Quaternion rotation, bool isCritical)
        {
            if (sliceParticles != null)
            {
                var effect = Instantiate(sliceParticles, position, rotation);
                if (isCritical)
                {
                    effect.transform.localScale *= 1.5f;
                }
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }
        }
        
        /// <summary>
        /// Play effect when a card is used
        /// </summary>
        /// <param name="cardType">Type of card that was used</param>
        public void PlayCardUseEffect(CardEffectType effectType)
        {
            if (activeEffect != null)
            {
                StopCoroutine(activeEffect);
            }
            
            // Start the new effect coroutine
            activeEffect = StartCoroutine(PlayEffect((int)effectType));
        }
        
        /// <summary>
        /// Play effect when a card is drawn
        /// </summary>
        public void PlayCardDrawEffect()
        {
            if (activeEffect != null)
            {
                StopCoroutine(activeEffect);
            }
            
            // Play card draw sound
            if (effectAudioSource != null && cardDrawSound != null)
            {
                effectAudioSource.PlayOneShot(cardDrawSound);
            }
        }
        
        /// <summary>
        /// Play a card effect based on card type or effect
        /// </summary>
        private IEnumerator PlayEffect(int effectType)
        {
            // Play particles if available
            if (cardEffectParticles != null)
            {
                cardEffectParticles.Play();
            }
            
            // Play sound effect if available
            if (effectAudioSource != null && cardUseSound != null && cardUseSound.Length > 0)
            {
                int soundIndex = effectType % cardUseSound.Length;
                effectAudioSource.PlayOneShot(cardUseSound[soundIndex]);
            }
            
            // Wait for effect duration
            yield return new WaitForSeconds(effectDuration);
            
            // Stop effect particles
            if (cardEffectParticles != null)
            {
                cardEffectParticles.Stop();
            }
            
            // Clear active effect
            activeEffect = null;
        }
    }
}
