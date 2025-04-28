using UnityEngine;
using System.Collections;
using PDXUnderground.Core.Interfaces; // For IDamageable and IGamblerCharacter

namespace PDXUnderground.Combat
{
    public abstract class CombatAbility : MonoBehaviour
    {
        [Header("Basic Ability Properties")]
        [SerializeField] protected string abilityName = "Default Ability";
        [SerializeField] protected string description = "Base combat ability";
        [SerializeField] protected float baseDamage = 10f;
        [SerializeField] protected float range = 5f;
        [SerializeField] protected float cooldownTime = 1f;
        [SerializeField] protected float buzzCost = 10f;
        
        [Header("Visual Effects")]
        [SerializeField] protected GameObject abilityVFXPrefab;
        
        [Header("Critical Hit Properties")]
        [SerializeField] protected float criticalHitChance = 0.2f;  // 20% chance by default
        [SerializeField] protected float criticalHitDamageMultiplier = 2f;
        
        [Header("Combo System")]
        [SerializeField] protected bool hasComboSystem = false;
        [SerializeField] protected int maxComboCount = 3;
        [SerializeField] protected float comboWindowTime = 1.5f;
        [SerializeField] protected float[] comboBuzzCostReduction;
        
        [Header("Animation")]
        [SerializeField] protected string animationTriggerName;
        
        // Protected references
        protected Animator characterAnimator;
        protected IGamblerCharacter gamblerCharacter;  // Using interface instead of concrete class
        
        // Runtime state
        protected bool isOnCooldown = false;
        protected float currentCooldown = 0f;
        protected float lastUseTime = 0f;
        protected int currentComboCount = 0;
        protected float comboTimeRemaining = 0f;
        
        #region Core Functionality

        protected virtual void Awake()
        {
            // Get required components
            characterAnimator = GetComponent<Animator>();
            gamblerCharacter = GetComponent<IGamblerCharacter>();
            
            if (gamblerCharacter == null)
            {
                Debug.LogWarning($"CombatAbility {abilityName} couldn't find IGamblerCharacter component!");
            }
        }
        
        protected virtual void Update()
        {
            // Update cooldown
            if (isOnCooldown)
            {
                currentCooldown -= Time.deltaTime;
                if (currentCooldown <= 0)
                {
                    isOnCooldown = false;
                    currentCooldown = 0;
                }
            }
            
            // Update combo timer
            if (hasComboSystem && currentComboCount > 0)
            {
                comboTimeRemaining -= Time.deltaTime;
                if (comboTimeRemaining <= 0)
                {
                    ResetCombo();
                }
            }
        }
        
        /// <summary>
        /// Attempt to use the ability
        /// </summary>
        /// <returns>True if the ability was used successfully</returns>
        public virtual bool UseAbility()
        {
            // Check if ability is on cooldown
            if (isOnCooldown)
            {
                Debug.Log($"{abilityName} is on cooldown. Remaining: {currentCooldown:F1} seconds");
                return false;
            }
            
            // Check if character has enough buzz
            if (gamblerCharacter != null)
            {
                float currentBuzzCost = GetCurrentComboBuzzCost();
                if (gamblerCharacter.GetCurrentBuzzPercentage() * gamblerCharacter.MaxBuzz / 100f < currentBuzzCost)
                {
                    Debug.Log($"Not enough Buzz to use {abilityName}. Required: {currentBuzzCost}");
                    return false;
                }
                
                // Consume buzz
                gamblerCharacter.UpdateBuzz(-currentBuzzCost);
            }
            
            // Start cooldown
            StartCooldown();
            
            // Execute ability implementation
            ExecuteAbilityEffect();
            
            return true;
        }
        
        /// <summary>
        /// Execute the ability's effect (to be implemented by derived classes)
        /// </summary>
        protected abstract void ExecuteAbilityEffect();
        
        /// <summary>
        /// Start the ability's cooldown
        /// </summary>
        protected virtual void StartCooldown()
        {
            isOnCooldown = true;
            currentCooldown = cooldownTime;
            lastUseTime = Time.time;
        }
        
        /// <summary>
        /// Reset combo state
        /// </summary>
        protected virtual void ResetCombo()
        {
            currentComboCount = 0;
            comboTimeRemaining = 0;
        }
        
        /// <summary>
        /// Apply damage to a target
        /// </summary>
        protected virtual void ApplyDamage(GameObject target, float damage, bool isCritical = false)
        {
            // Get IDamageable interface from target
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float finalDamage = damage;
                if (isCritical)
                {
                    finalDamage *= criticalHitDamageMultiplier;
                }
                
                damageable.TakeDamage(finalDamage);
                Debug.Log($"{abilityName} dealt {finalDamage} damage to {target.name}. Critical: {isCritical}");
            }
        }
        
        /// <summary>
        /// Roll for critical hit
        /// </summary>
        protected bool RollForCriticalHit()
        {
            return Random.value < criticalHitChance;
        }

        #endregion

        #region Combo System
        /// <summary>
        /// Get the adjusted buzz cost for the current combo state
        /// </summary>
        /// <returns>Adjusted buzz cost for the current combo state</returns>
        protected virtual float GetCurrentComboBuzzCost()
        {
            if (!hasComboSystem || currentComboCount <= 0 || comboBuzzCostReduction == null || comboBuzzCostReduction.Length == 0)
                return buzzCost;
                
            // Get the cost reduction factor for the current combo count
            int index = Mathf.Min(currentComboCount - 1, comboBuzzCostReduction.Length - 1);
            float reductionFactor = comboBuzzCostReduction[index];
            
            // Apply the reduction to the base cost
            return buzzCost * (1f - reductionFactor);
        }
        #endregion

        #region Property Accessors
        /// <summary>
        /// Is this ability currently on cooldown?
        /// </summary>
        public bool IsOnCooldown => isOnCooldown;

        /// <summary>
        /// Current cooldown time remaining
        /// </summary>
        public float CurrentCooldown => currentCooldown;

        /// <summary>
        /// Current cooldown as a percentage (0-1)
        /// </summary>
        public float CooldownPercentage => isOnCooldown ? currentCooldown / cooldownTime : 0f;

        /// <summary>
        /// Current combo count
        /// </summary>
        public int CurrentComboCount => currentComboCount;

        /// <summary>
        /// Time remaining in current combo window
        /// </summary>
        public float ComboTimeRemaining => comboTimeRemaining;

        /// <summary>
        /// Time of last ability use
        /// </summary>
        public float LastUseTime => lastUseTime;

        /// <summary>
        /// Name of the ability
        /// </summary>
        public string AbilityName => abilityName;

        /// <summary>
        /// Description of the ability
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Base damage of the ability
        /// </summary>
        public float BaseDamage => baseDamage;

        /// <summary>
        /// Effective range of the ability
        /// </summary>
        public float Range => range;

        /// <summary>
        /// Base buzz cost of the ability
        /// </summary>
        public float BuzzCost => buzzCost;

        /// <summary>
        /// Get the cooldown percentage (0-1)
        /// </summary>
        public float GetCooldownPercentage()
        {
            return isOnCooldown ? currentCooldown / cooldownTime : 0f;
        }

        #endregion

    } // end of CombatAbility class
} // end of namespace
