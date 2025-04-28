using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PDXUnderground.Models;

namespace PDXUnderground.Player
{
    /// <summary>
    /// Base character class for all playable and non-playable characters in the game.
    /// Manages character stats, status effects, combat, and interactions.
    /// </summary>
    [RequireComponent(typeof(CharacterStats))]
    public class Character : MonoBehaviour
    {
        #region Character Properties
        [Header("Basic Information")]
        [SerializeField] private string _characterName = "Character";
        [SerializeField] private int _level = 1;
        [SerializeField] private int _id = 0;
        
        [Header("References")]
        [SerializeField] private CharacterStats _stats;
        
        // Public accessors
        public string CharacterName => _characterName;
        public int Level => _level;
        public int Id => _id;
        public CharacterStats Stats => _stats;
        
        // Character state
        private bool _isDead = false;
        public bool IsDead => _isDead;
        
        // Events
        public event Action<int, string, Character> OnDamageTaken;
        public event Action<int, string, Character> OnDamageDealt;
        public event Action<Character> OnDeath;
        public event Action<string, float, string> OnStatusEffectApplied;
        public event Action<string> OnStatusEffectRemoved;
        #endregion
        
        #region Status Effect System
        // Active status effects
        private Dictionary<string, StatusEffect> activeStatusEffects = new Dictionary<string, StatusEffect>();
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            // Get references
            if (_stats == null)
            {
                _stats = GetComponent<CharacterStats>();
            }
            
            if (_stats == null)
            {
                Debug.LogError($"Character {_characterName} is missing CharacterStats component!");
            }
        }
        
        private void Start()
        {
            // Initialize character
            if (_id == 0)
            {
                _id = GetInstanceID(); // Use instance ID as default if not set
            }
        }
        
        private void Update()
        {
            // Check for expired status effects
            CheckStatusEffects();
        }
        #endregion
        
        #region Status Effect Methods
        /// <summary>
        /// Apply a status effect to the character
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="source">Source of the effect</param>
        /// <returns>True if effect was applied</returns>
        public bool ApplyStatusEffect(string effectName, float duration, string source)
        {
            if (string.IsNullOrEmpty(effectName) || _isDead)
                return false;
                
            effectName = effectName.ToLower();
            
            // Create new effect
            StatusEffect effect = new StatusEffect
            {
                EffectName = effectName,
                Duration = duration,
                StartTime = Time.time,
                Source = source
            };
            
            // Add or refresh effect
            if (activeStatusEffects.ContainsKey(effectName))
            {
                // If existing effect has longer duration, keep it
                // If existing effect has longer duration, keep it
                if (activeStatusEffects[effectName].RemainingTime > effect.RemainingTime)
                    Debug.Log($"{_characterName} already has {effectName} with longer duration");
                    return false;
                }
                
                // Otherwise, refresh with new effect
                activeStatusEffects[effectName] = effect;
                Debug.Log($"{_characterName}'s {effectName} effect refreshed from {source} for {duration}s");
            }
            else
            {
                // Add new effect
                activeStatusEffects.Add(effectName, effect);
                Debug.Log($"{_characterName} affected by {effectName} from {source} for {duration}s");
            }
            
            // Apply effect mechanics based on effect type
            ApplyEffectMechanics(effectName);
            
            // Trigger event
            OnStatusEffectApplied?.Invoke(effectName, duration, source);
            
            return true;
        }
        
        /// <summary>
        /// Remove a status effect from the character
        /// </summary>
        /// <param name="effectName">Name of the effect to remove</param>
        /// <param name="source">Source that is removing the effect (optional)</param>
        /// <returns>True if effect was removed</returns>
        public bool RemoveStatusEffect(string effectName, string source = "")
        {
            if (string.IsNullOrEmpty(effectName))
                return false;
                
            effectName = effectName.ToLower();
            
            // Check if effect exists
            if (activeStatusEffects.ContainsKey(effectName))
                // If source specified, only remove if it matches
                if (!string.IsNullOrEmpty(source) && 
                    activeStatusEffects[effectName].Source != source)
                    activeStatusEffects[effectName].source != source)
                {
                    return false;
                }
                
                // Remove effect
                activeStatusEffects.Remove(effectName);
                
                // Remove effect mechanics
                RemoveEffectMechanics(effectName);
                
                Debug.Log($"{_characterName} no longer affected by {effectName}");
                
                // Trigger event
                OnStatusEffectRemoved?.Invoke(effectName);
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check for expired status effects and remove them
        /// </summary>
        private void CheckStatusEffects()
        {
            List<string> expiredEffects = new List<string>();
            
            // Find expired effects
            foreach (var kvp in activeStatusEffects)
            {
                if (kvp.Value.IsExpired)
                {
                    expiredEffects.Add(kvp.Key);
                }
            }
            
            // Remove expired effects
            foreach (string effect in expiredEffects)
            {
                RemoveStatusEffect(effect);
            }
        }
        
        /// <summary>
        /// Remove expired status effect from a specific source
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        /// <param name="source">Source of the effect</param>
        /// <returns>True if effect was removed</returns>
        public bool RemoveExpiredStatusEffect(string effectName, string source)
        {
            if (string.IsNullOrEmpty(effectName) || string.IsNullOrEmpty(source))
                return false;
                
            effectName = effectName.ToLower();
            
            // Check if effect exists and is from the specified source
            if (activeStatusEffects.TryGetValue(effectName, out StatusEffect effect) && 
                effect.Source == source)
            {
                // Check if expired
                if (effect.IsExpired)
                {
                    return RemoveStatusEffect(effectName, source);
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Apply mechanical effects based on status effect type
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        private void ApplyEffectMechanics(string effectName)
        {
            if (_stats == null)
                return;
                
            // Apply different effects based on effect name
            switch (effectName)
            {
                case "poison":
                    StartCoroutine(PoisonEffect());
                    break;
                case "burn":
                    StartCoroutine(BurnEffect());
                    break;
                case "slow":
                    // Example: Apply movement speed reduction
                    _stats.AddStatBoost("movement_speed", -30, true, activeStatusEffects[effectName].RemainingTime, "Effect:slow");
                    break;
                case "stun":
                    // Example: Immobilize character
                    // Implementation depends on movement system
                    break;
                case "strength":
                    _stats.AddStatBoost("strength", 5, false, activeStatusEffects[effectName].RemainingTime, "Effect:strength");
                    break;
                case "weakness":
                    _stats.AddStatBoost("strength", -3, false, activeStatusEffects[effectName].RemainingTime, "Effect:weakness");
                    break;
            }
        }
        
        /// <summary>
        /// Remove mechanical effects of a status effect
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        private void RemoveEffectMechanics(string effectName)
        {
            if (_stats == null)
                return;
                
            // Remove stat boosts associated with the effect
            _stats.RemoveStatBoostsFromSource($"Effect:{effectName}");
            
            // Additional cleanup for specific effects
            switch (effectName)
            {
                case "poison":
                case "burn":
                    // Stop damage over time coroutines
                    StopCoroutine(effectName + "Effect");
                    break;
                case "stun":
                    // Re-enable movement
                    break;
            }
        }
        
        /// <summary>
        /// Poison effect coroutine - deals damage over time
        /// </summary>
        private IEnumerator PoisonEffect()
        {
            while (activeStatusEffects.ContainsKey("poison") && !_isDead)
            {
                // Damage based on max health
                int poisonDamage = Mathf.Max(1, (int)(_stats.MaxHealth * 0.03f));
                TakeDamage(poisonDamage, "poison", null);
                
                yield return new WaitForSeconds(2f); // Poison tick every 2 seconds
            }
        }
        
        /// <summary>
        /// Burn effect coroutine - deals damage over time
        /// </summary>
        private IEnumerator BurnEffect()
        {
            while (activeStatusEffects.ContainsKey("burn") && !_isDead)
            {
                // Flat damage
                int burnDamage = 5;
                TakeDamage(burnDamage, "fire", null);
                
                yield return new WaitForSeconds(1f); // Burn tick every 1 second
            }
        }
        #endregion
        
        #region Damage and Combat
        /// <summary>
        /// Take damage from an attack or effect
        /// </summary>
        /// <param name="damageAmount">Amount of damage</param>
        /// <param name="damageType">Type of damage</param>
        /// <param name="attacker">Character causing the damage (can be null)</param>
        /// <returns>Actual damage taken after mitigation</returns>
        public int TakeDamage(int damageAmount, string damageType, Character attacker)
        {
            if (_isDead || damageAmount <= 0)
                return 0;
                
            // Apply damage mitigation based on type
            int mitigatedDamage = damageAmount;
            
            if (_stats != null)
            {
                if (damageType == "physical")
                {
                    // Apply physical damage reduction
                    mitigatedDamage = Mathf.RoundToInt(damageAmount * (1f - _stats.DamageReduction));
                }
                else
                {
                    // Apply magic resistance for non-physical damage
                    mitigatedDamage = Mathf.RoundToInt(damageAmount * (1f - _stats.MagicResistance));
                }
            }
            
            // Ensure minimum damage of 1
            mitigatedDamage = Mathf.Max(1, mitigatedDamage);
            
            // Apply damage to health
            if (_stats != null)
            {
                _stats.CurrentHealth -= mitigatedDamage;
                
                // Check for death
                if (_stats.CurrentHealth <= 0)
                {
                    Die();
                }
            }
            
            // Log damage
            Debug.Log($"{_characterName} took {mitigatedDamage} {damageType} damage from {(attacker != null ? attacker.CharacterName : "an effect")}");
            
            // Trigger damage event
            OnDamageTaken?.Invoke(mitigatedDamage, damageType, attacker);
            
            // If attacker exists, trigger their damage dealt event
            attacker?.OnDamageDealt?.Invoke(mitigatedDamage, damageType, this);
            
            return mitigatedDamage;
        }
        
        /// <summary>
        /// Die - handle character death
        /// </summary>
        private void Die()
        {
            if (_isDead)
                return;
                
            _isDead = true;
            
            // Log death
            Debug.Log($"{_characterName} has died");
            
            // Trigger death event
            OnDeath?.Invoke(this);
            
            // Clean up active effects
            List<string> effectsToRemove = new List<string>(activeStatusEffects.Keys);
            foreach (string effect in effectsToRemove)
            {
                RemoveStatusEffect(effect);
            }
            
            // TODO: Add any additional death handling (animations, loot drops, etc.)
        }
        
        /// <summary>
        /// Deal damage to another character
        /// </summary>
        /// <param name="target">Target character</param>
        /// <param name="damage">Amount of damage</param>
        /// <param name="damageType">Type of damage</param>
        /// <returns>Actual damage dealt after mitigation</returns>
        public int DealDamage(Character target, int damage, string damageType)
        {
            if (target == null || _isDead || damage <= 0)
                return 0;
                
            return target.TakeDamage(damage, damageType, this);
        }
        #endregion
        
        #region Status Effect Utilities
        /// <summary>
        /// Check if character has a specific status effect
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        /// <returns>True if the effect is active</returns>
        public bool HasStatusEffect(string effectName)
        {
            if (string.IsNullOrEmpty(effectName))
                return false;
                
            return activeStatusEffects.ContainsKey(effectName.ToLower());
        }
        
        /// <summary>
        /// Get the remaining duration of a status effect
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        /// <returns>Remaining duration in seconds, 0 if not active</returns>
        public float GetStatusEffectDuration(string effectName)
        {
            if (string.IsNullOrEmpty(effectName))
                return 0f;
                
            effectName = effectName.ToLower();
            
            if (activeStatusEffects.TryGetValue(effectName, out StatusEffect effect))
            {
                return effect.RemainingTime;
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Get a list of all active status effects
        /// </summary>
        /// <returns>List of effect names</returns>
        public List<string> GetActiveStatusEffects()
        {
            return new List<string>(activeStatusEffects.Keys);
        }
        
        /// <summary>
        /// Check if character is immune to a specific effect type
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        /// <returns>True if immune</returns>
        public bool IsImmuneToEffect(string effectName)
        {
            if (string.IsNullOrEmpty(effectName))
                return false;
                
            // Add immunity rules here based on character attributes, equipment, etc.
            // Example: Fire-based characters might be immune to burn effects
            
            return false; // Default is not immune
        }
        #endregion
    }
}
