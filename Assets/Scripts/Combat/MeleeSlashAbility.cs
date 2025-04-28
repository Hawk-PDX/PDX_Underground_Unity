using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDXUnderground.Effects;

namespace PDXUnderground.Combat
{
    /// <summary>
    /// Melee card-slashing ability for the Gambler character.
    /// Allows close-range card attacks with combo system and configurable properties.
    /// </summary>
    public class MeleeSlashAbility : CombatAbility
    {
        [Header("Slash Ability Properties")]
        [Tooltip("Attack arc in degrees")]
        [SerializeField] private float slashAttackArc = 120f;
        
        [Tooltip("Attack range in meters")]
        [SerializeField] private float slashAttackRange = 2.5f;
        
        [Tooltip("Number of hits in the combo")]
        [SerializeField] private int slashComboCount = 3;
        
        [Tooltip("Delay between combo hits")]
        [SerializeField] private float slashComboDelay = 0.2f;
        
        [Tooltip("Window of time to continue combo (seconds)")]
    new protected float comboWindowTime = 1.5f;
        
        [Tooltip("Layers that can be hit by slash attacks")]
        [SerializeField] private LayerMask targetLayers;
        
        [Tooltip("Origin point for slash attacks")]
        [SerializeField] private Transform attackOrigin;
        
        [Header("Combo Properties")]
        [Tooltip("Damage multiplier for each hit in combo (1st, 2nd, 3rd, etc.)")]
        [SerializeField] private float[] comboMultipliers = new float[] { 1.0f, 1.2f, 1.5f };
        
        [Tooltip("Critical hit chance bonus for combo hits")]
        [SerializeField] private float comboCriticalChanceBonus = 0.05f;
        
        [Tooltip("Buzz return on hit")]
        [SerializeField] private float buzzReturnOnHit = 3f;
        
        [Tooltip("Additional buzz return on critical hit")]
        [SerializeField] private float criticalBuzzReturn = 5f;
        
        [Header("Visual Effects")]
        [Tooltip("VFX prefab for slash effect")]
        [SerializeField] private GameObject slashVFXPrefab;
        
        [Tooltip("Color of the slash VFX")]
        [SerializeField] private Color slashVFXColor = Color.red;
        
        [Tooltip("Sound for slash attack")]
        [SerializeField] private AudioClip[] slashSounds;
        
        [Tooltip("Sound for critical slash")]
        [SerializeField] private AudioClip criticalSlashSound;
        
        // Runtime state
    new protected int currentComboCount = 0;
        private float lastComboTime = 0f;
        private bool inCombo = false;
        private CardEffectsController cardEffects;
        
        // For combo animation states
        private string[] comboAnimationTriggers = new string[] { "Slash1", "Slash2", "Slash3" };

        /// <summary>
        /// Initialize references and default values
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Set default animation trigger for slash if not specified
            if (string.IsNullOrEmpty(animationTriggerName))
            {
                animationTriggerName = "Slash";
            }
            
            // If no attack origin specified, use transform
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }
            
            // Find card effects controller
            cardEffects = FindObjectOfType<CardEffectsController>();
        }
        
        /// <summary>
        /// Update combo state
        /// </summary>
        protected override void Update()
        {
            base.Update();
            
            // Check if combo should time out
            if (inCombo && Time.time - lastComboTime > comboWindowTime)
            {
                ResetCombo();
            }
        }

        /// <summary>
        /// Execute the slash attack
        /// </summary>
        protected override void ExecuteAbilityEffect()
        {
            // Check if we're continuing a combo
            if (inCombo)
            {
                // Increment combo counter
                currentComboCount++;
                
                // Cap combo at max count
                if (currentComboCount >= slashComboCount)
                {
                    currentComboCount = 0;
                    inCombo = false;
                }
            }
            else
            {
                // Start new combo
                currentComboCount = 0;
                inCombo = true;
            }
            
            // Update last combo time
            lastComboTime = Time.time;
            
            // Start the slash combo coroutine
            StartCoroutine(PerformSlashAttack());
        }
        
        /// <summary>
        /// Reset the combo state
        /// </summary>
        protected override void ResetCombo()
        {
            currentComboCount = 0;
            inCombo = false;
        }

        /// <summary>
        /// Perform the slash attack, handling combos
        /// </summary>
        private IEnumerator PerformSlashAttack()
        {
            // Get the appropriate animation trigger for this combo hit
            string animTrigger = comboAnimationTriggers.Length > currentComboCount 
                ? comboAnimationTriggers[currentComboCount] 
                : animationTriggerName;
            
            // Trigger the animation
            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger(animTrigger);
            }
            
            // Short delay to sync with animation
            yield return new WaitForSeconds(0.1f);
            
            // Perform the actual slice
            PerformSlice();
            
            // Wait for combo delay before allowing next hit
            if (inCombo && currentComboCount < slashComboCount - 1)
            {
                yield return new WaitForSeconds(slashComboDelay);
            }
        }
        
        /// <summary>
        /// Perform the actual slice attack, detecting hits and applying damage
        /// </summary>
        private void PerformSlice()
        {
            // Find targets in range and arc
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin.position, slashAttackRange, targetLayers);
            
            // Get damage multiplier for this combo hit
            float damageMultiplier = 1.0f;
            if (comboMultipliers.Length > currentComboCount)
            {
                damageMultiplier = comboMultipliers[currentComboCount];
            }
            
            // Get crit chance for this combo hit (increases with combo)
            float critChance = criticalHitChance + (comboCriticalChanceBonus * currentComboCount);
            
            // Track how many targets we hit
            int targetsHit = 0;
            
            // Get forward direction for arc calculation
            Vector3 forward = attackOrigin.forward;
            
            foreach (var hitCollider in hitColliders)
            {
                // Check if target is within arc
                Vector3 directionToTarget = (hitCollider.transform.position - attackOrigin.position).normalized;
                float angleToTarget = Vector3.Angle(forward, directionToTarget);
                
                if (angleToTarget <= slashAttackArc * 0.5f)
                {
                    // Check for critical hit
                    bool isCritical = Random.value < critChance;
                    
                    // Apply damage
                    float damage = baseDamage * damageMultiplier;
                    ApplyDamage(hitCollider.gameObject, damage, isCritical);
                    
                    // Return some buzz on hit
                    if (gamblerCharacter != null)
                    {
                        float buzzReturn = buzzReturnOnHit;
                        if (isCritical)
                        {
                            buzzReturn += criticalBuzzReturn;
                        }
                        
                        gamblerCharacter.UpdateBuzz(buzzReturn);
                    }
                    
                    // Try to get hit position for effects
                    Vector3 hitPosition = hitCollider.transform.position;
                    
                    // Try to use collider bounds center if available
                    if (hitCollider != null)
                    {
                        hitPosition = hitCollider.bounds.center;
                    }
                    
                    // Play slash impact visuals
                    PlaySlashVisualEffects(hitPosition, isCritical);
                    
                    targetsHit++;
                }
            }
            
            // If no targets were hit, spawn VFX in front anyway for visual feedback
            if (targetsHit == 0)
            {
                Vector3 forwardPos = attackOrigin.position + forward * slashAttackRange * 0.7f;
                PlaySlashVisualEffects(forwardPos, false);
            }
            
            // Play slash sound
            PlaySlashSound(targetsHit > 0, (targetsHit > 0 && Random.value < criticalHitChance));
            
            Debug.Log($"Slash attack {currentComboCount} hit {targetsHit} targets");
        }
        
        /// <summary>
        /// Play visual effects for the slash attack
        /// </summary>
        /// <param name="position">Position to show effects</param>
        /// <param name="isCritical">Whether this was a critical hit</param>
        private void PlaySlashVisualEffects(Vector3 position, bool isCritical)
        {
            // Use CardEffectsController if available
            if (cardEffects != null)
            {
                Quaternion rotation = Quaternion.LookRotation(attackOrigin.forward, Vector3.up);
                cardEffects.PlaySliceEffect(position, rotation, isCritical);
                return;
            }
            
            // Fallback: use local VFX if no card effects controller
            if (slashVFXPrefab != null)
            {
                GameObject vfx = Instantiate(slashVFXPrefab, position, Quaternion.identity);
                
                // Try to set color if possible
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var mainModule = ps.main;
                    mainModule.startColor = isCritical ? Color.yellow : slashVFXColor;
                }
                
                // Scale up critical effects
                if (isCritical)
                {
                    vfx.transform.localScale *= 1.5f;
                }
                
                Destroy(vfx, 1f);
            }
        }
        
        /// <summary>
        /// Play sound effect for slash attack
        /// </summary>
        /// <param name="hitTarget">Whether a target was hit</param>
        /// <param name="isCritical">Whether this was a critical hit</param>
        private void PlaySlashSound(bool hitTarget, bool isCritical)
        {
            // Play critical hit sound
            if (hitTarget && isCritical && criticalSlashSound != null)
            {
                AudioSource.PlayClipAtPoint(criticalSlashSound, attackOrigin.position);
                return;
            }
            
            // Play regular slash sound
            if (slashSounds != null && slashSounds.Length > 0)
            {
                // Pick a random slash sound
                int soundIndex = Random.Range(0, slashSounds.Length);
                AudioClip sound = slashSounds[soundIndex];
                
                if (sound != null)
                {
                    AudioSource.PlayClipAtPoint(sound, attackOrigin.position);
                }
            }
        }
        
        /// <summary>
        /// Handle ability use attempt - check combo state and requirements
        /// </summary>
        public override bool UseAbility()
        {
            // If we're not in a combo, use standard requirements
            if (!inCombo)
            {
                return base.UseAbility();
            }
            
            // For combo hits, we need special handling
            
            // Check if ability is on cooldown
            if (isOnCooldown)
            {
                Debug.Log($"{abilityName} is on cooldown. Remaining: {currentCooldown:F1} seconds");
                return false;
            }
            
            // Check if within combo window time
            if (Time.time - lastComboTime > comboWindowTime)
            {
                ResetCombo();
                return base.UseAbility(); // Start a new combo
            }
            
            // Check if Gambler has enough Buzz for this combo hit
            // Each successive hit costs less buzz (70% of base cost)
            float comboBuzzCost = buzzCost * (1.0f - (0.3f * currentComboCount));
            if (gamblerCharacter.GetCurrentBuzzPercentage() * gamblerCharacter.MaxBuzz / 100f < comboBuzzCost)
            {
                Debug.Log($"Not enough Buzz to continue {abilityName} combo. Required: {comboBuzzCost}");
                return false;
            }
            
            // Consume Buzz (reduced for combo hits)
            gamblerCharacter.UpdateBuzz(-comboBuzzCost);
            
            // No cooldown for combo hits, but reset lastUseTime
            lastUseTime = Time.time;
            
            // Execute ability implementation
            ExecuteAbilityEffect();
            
            return true;
        }
        
        /// <summary>
        /// Visualize the attack arc in the editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (attackOrigin == null)
                return;
                
            Gizmos.color = Color.red;
            
            // Draw attack range
            Gizmos.DrawWireSphere(attackOrigin.position, slashAttackRange);
            
            // Draw attack arc
            float halfArc = slashAttackArc * 0.5f * Mathf.Deg2Rad;
            Vector3 leftArcDir = Quaternion.Euler(0, -slashAttackArc * 0.5f, 0) * attackOrigin.forward;
            Vector3 rightArcDir = Quaternion.Euler(0, slashAttackArc * 0.5f, 0) * attackOrigin.forward;
            
            Gizmos.DrawRay(attackOrigin.position, leftArcDir * slashAttackRange);
            Gizmos.DrawRay(attackOrigin.position, rightArcDir * slashAttackRange);
            
            // Draw arc segments
            int segments = 10;
            Vector3 prevPos = attackOrigin.position + leftArcDir * slashAttackRange;
            
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = -halfArc + t * slashAttackArc * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                Vector3 arcPos = attackOrigin.position + attackOrigin.TransformDirection(direction) * slashAttackRange;
                
                Gizmos.DrawLine(prevPos, arcPos);
                prevPos = arcPos;
            }
        }
    }
}
