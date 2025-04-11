using System;
using System.Collections;
using UnityEngine;
using PDXUnderground;

namespace PDXUnderground.Combat
{
    /// <summary>
    /// Base abstract class for all combat abilities in the game
    /// </summary>
    public abstract class CombatAbility : MonoBehaviour
    {
        [Header("Base Ability Properties")]
        [SerializeField] protected string abilityName = "Default Ability";
        [SerializeField] protected string description = "Base ability description";
        [SerializeField] protected float baseDamage = 10f;
        [SerializeField] protected float range = 2f;
        [SerializeField] protected float cooldownTime = 1f;
        [SerializeField] protected float buzzCost = 5f;
        [SerializeField] protected AudioClip abilitySound;
        [SerializeField] protected GameObject abilityVFXPrefab;
        [SerializeField] protected string animationTriggerName = "Attack";
        
        [Header("Critical Hit Properties")]
        [SerializeField] protected float criticalHitChance = 0.1f; // 10% chance by default
        [SerializeField] protected float criticalHitDamageMultiplier = 1.5f;
        [SerializeField] protected float criticalHitBuzzReturn = 5f; // Amount of Buzz returned on critical hit
        
        // Runtime state
        protected float currentCooldown = 0f;
        protected bool isOnCooldown = false;
        protected Animator characterAnimator;
        protected GamblerCharacter gamblerCharacter;

        protected virtual void Awake()
        {
            // Cache component references
            characterAnimator = GetComponentInParent<Animator>();
            gamblerCharacter = GetComponentInParent<GamblerCharacter>();
            
            if (gamblerCharacter == null)
            {
                Debug.LogError($"CombatAbility {abilityName} requires a GamblerCharacter component in the parent hierarchy.");
            }
        }

        protected virtual void Update()
        {
            // Update cooldown timer
            if (isOnCooldown)
            {
                currentCooldown -= Time.deltaTime;
                if (currentCooldown <= 0f)
                {
                    isOnCooldown = false;
                    currentCooldown = 0f;
                    OnCooldownComplete();
                }
            }
        }

        /// <summary>
        /// Attempt to use the ability
        /// </summary>
        /// <returns>True if ability was used successfully, false otherwise</returns>
        public virtual bool UseAbility()
        {
            // Check if ability is on cooldown
            if (isOnCooldown)
            {
                Debug.Log($"{abilityName} is on cooldown. Remaining: {currentCooldown:F1} seconds");
                return false;
            }

            // Check if Gambler has enough Buzz
            if (gamblerCharacter.GetCurrentBuzzPercentage() * gamblerCharacter.MaxBuzz / 100f < buzzCost)
            {
                Debug.Log($"Not enough Buzz to use {abilityName}. Required: {buzzCost}");
                return false;
            }

            // Consume Buzz
            gamblerCharacter.UpdateBuzz(-buzzCost);

            // Trigger animation
            if (characterAnimator != null && !string.IsNullOrEmpty(animationTriggerName))
            {
                characterAnimator.SetTrigger(animationTriggerName);
            }

            // Play sound effect
            if (abilitySound != null)
            {
                AudioSource.PlayClipAtPoint(abilitySound, transform.position);
            }

            // Start cooldown
            StartCooldown();

            // Execute ability implementation
            ExecuteAbilityEffect();

            return true;
        }

        /// <summary>
        /// Implement the actual ability effect in derived classes
        /// </summary>
        protected abstract void ExecuteAbilityEffect();

        /// <summary>
        /// Start the ability cooldown
        /// </summary>
        protected virtual void StartCooldown()
        {
            isOnCooldown = true;
            currentCooldown = cooldownTime;
        }

        /// <summary>
        /// Called when cooldown is complete
        /// </summary>
        protected virtual void OnCooldownComplete()
        {
            // Can be overridden in derived classes
            Debug.Log($"{abilityName} cooldown complete");
        }

        /// <summary>
        /// Calculate if an attack is a critical hit
        /// </summary>
        /// <returns>True if critical hit</returns>
        protected virtual bool RollForCriticalHit()
        {
            return UnityEngine.Random.value < criticalHitChance;
        }

        /// <summary>
        /// Apply damage to a target
        /// </summary>
        /// <param name="target">Target game object with IDamageable interface</param>
        /// <param name="damageAmount">Amount of damage to apply</param>
        /// <param name="isCritical">Is this a critical hit</param>
        protected virtual void ApplyDamage(GameObject target, float damageAmount, bool isCritical = false)
        {
            // Apply critical hit multiplier if applicable
            float finalDamage = isCritical ? damageAmount * criticalHitDamageMultiplier : damageAmount;
            
            // Apply damage to target if it has a damageable component
            IDamageable damageableTarget = target.GetComponent<IDamageable>();
            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(finalDamage);
                
                // If critical hit, return some buzz
                if (isCritical)
                {
                    gamblerCharacter.UpdateBuzz(criticalHitBuzzReturn);
                    Debug.Log($"Critical hit! Gained {criticalHitBuzzReturn} Buzz");
                    
                    // Trigger visual/audio feedback for critical
                    OnCriticalHit(target);
                }
            }
            else
            {
                Debug.Log($"Target does not implement IDamageable interface");
            }
        }
        
        /// <summary>
        /// Called when a critical hit occurs
        /// </summary>
        /// <param name="target">The target that was hit</param>
        protected virtual void OnCriticalHit(GameObject target)
        {
            // Override in derived classes for special critical effects
            Debug.Log($"Critical Hit with {abilityName}!");
            
            // Spawn critical hit VFX if available
            if (abilityVFXPrefab != null)
            {
                GameObject vfx = Instantiate(abilityVFXPrefab, target.transform.position, Quaternion.identity);
                vfx.transform.localScale *= 1.5f; // Make critical VFX larger
                Destroy(vfx, 2f);
            }
        }
        
        /// <summary>
        /// Get the current cooldown remaining as a percentage (0-1)
        /// </summary>
        public float GetCooldownPercentage()
        {
            if (!isOnCooldown || cooldownTime <= 0f)
                return 0f;
                
            return currentCooldown / cooldownTime;
        }
        
        /// <summary>
        /// Is this ability currently on cooldown?
        /// </summary>
        public bool IsOnCooldown => isOnCooldown;
        
        /// <summary>
        /// Get the name of this ability
        /// </summary>
        public string AbilityName => abilityName;
        
        /// <summary>
        /// Get the description of this ability
        /// </summary>
        public string Description => description;
        
        /// <summary>
        /// Get the buzz cost of this ability
        /// </summary>
        public float BuzzCost => buzzCost;
    }

    /// <summary>
    /// Slice ability: dual-wield cards for close quarters melee attack
    /// </summary>
    public class SliceAbility : CombatAbility
    {
        [Header("Slice Ability Properties")]
        [SerializeField] private float sliceAttackArc = 120f; // Attack arc in degrees
        [SerializeField] private int sliceHitCount = 2; // Number of hits in the combo
        [SerializeField] private float sliceComboDelay = 0.2f; // Delay between hits
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private Transform attackOrigin;

        [Header("Slice VFX")]
        [SerializeField] private GameObject sliceVFXPrefab;
        [SerializeField] private Color sliceVFXColor = Color.red;

        protected override void Awake()
        {
            base.Awake();
            
            // Set default animation trigger for slice
            if (string.IsNullOrEmpty(animationTriggerName))
            {
                animationTriggerName = "Slice";
            }
            
            // If no attack origin specified, use transform
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }
        }

        protected override void ExecuteAbilityEffect()
        {
            // Start the slice combo coroutine
            StartCoroutine(PerformSliceCombo());
        }

        private IEnumerator PerformSliceCombo()
        {
            for (int i = 0; i < sliceHitCount; i++)
            {
                // Perform the actual slice attack
                PerformSliceAttack();
                
                // Wait before next hit in combo
                if (i < sliceHitCount - 1)
                {
                    yield return new WaitForSeconds(sliceComboDelay);
                }
            }
        }

        private void PerformSliceAttack()
        {
            // Find targets in range and arc
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin.position, range, targetLayers);
            
            int targetsHit = 0;
            
            foreach (var hitCollider in hitColliders)
            {
                // Check if target is within arc
                Vector3 directionToTarget = (hitCollider.transform.position - attackOrigin.position).normalized;
                float angleToTarget = Vector3.Angle(attackOrigin.forward, directionToTarget);
                
                if (angleToTarget <= sliceAttackArc * 0.5f)
                {
                    // Check for critical hit
                    bool isCritical = RollForCriticalHit();
                    
                    // Apply damage
                    ApplyDamage(hitCollider.gameObject, baseDamage, isCritical);
                    
                    // Spawn slice VFX
                    SpawnSliceVFX(hitCollider.transform.position);
                    
                    targetsHit++;
                }
            }
            
            // If no targets were hit, spawn VFX in front anyway for visual feedback
            if (targetsHit == 0)
            {
                Vector3 forwardPos = attackOrigin.position + attackOrigin.forward * range * 0.7f;
                SpawnSliceVFX(forwardPos);
            }
            
            Debug.Log($"Slice attack hit {targetsHit} targets");
        }

        private void SpawnSliceVFX(Vector3 position)
        {
            if (sliceVFXPrefab != null)
            {
                GameObject vfx = Instantiate(sliceVFXPrefab, position, Quaternion.identity);
                
                // Try to set color if possible
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var mainModule = ps.main;
                    mainModule.startColor = sliceVFXColor;
                }
                
                Destroy(vfx, 1f);
            }
        }

        // Visualize the attack arc in the editor
        private void OnDrawGizmosSelected()
        {
            if (attackOrigin == null)
                return;
                
            Gizmos.color = Color.red;
            
            // Draw attack range
            Gizmos.DrawWireSphere(attackOrigin.position, range);
            
            // Draw attack arc
            float halfArc = sliceAttackArc * 0.5f * Mathf.Deg2Rad;
            Vector3 leftArcDir = Quaternion.Euler(0, -sliceAttackArc * 0.5f, 0) * attackOrigin.forward;
            Vector3 rightArcDir = Quaternion.Euler(0, sliceAttackArc * 0.5f, 0) * attackOrigin.forward;
            
            Gizmos.DrawRay(attackOrigin.position, leftArcDir * range);
            Gizmos.DrawRay(attackOrigin.position, rightArcDir * range);
            
            // Draw arc segments
            int segments = 10;
            Vector3 prevPos = attackOrigin.position + leftArcDir * range;
            
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = -halfArc + t * sliceAttackArc * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                Vector3 arcPos = attackOrigin.position + attackOrigin.TransformDirection(direction) * range;
                
                Gizmos.DrawLine(prevPos, arcPos);
                prevPos = arcPos;
            }
        }
    }

    /// <summary>
    /// Flick ability: ranged attack - throw cards one at a time
    /// </summary>
    public class FlickAbility : CombatAbility
    {
        [Header("Flick Ability Properties")]
        [SerializeField] private GameObject cardProjectilePrefab;
        [SerializeField] private Transform cardSpawnPoint;
        [SerializeField] private float cardSpeed = 20f;
        [SerializeField] private float cardLifetime = 3f;
        [SerializeField] private int cardsPerFlick = 1;
        [SerializeField] private float spreadAngle = 5f;
        [SerializeField] private float cardFireDelay = 0.1f;
        
        [Header("Targeting")]
        [SerializeField] private bool useTargeting = true;
        [SerializeField] private float targetingRange = 20f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private Transform aimTransform;

        protected override void Awake()
        {
            base.Awake();
            
            // Set default animation trigger for flick
            if (string.IsNullOrEmpty(animationTriggerName))
            {
                animationTriggerName = "Flick";
            }
            
            // If no spawn point specified, use transform
            if (cardSpawnPoint == null)
            {
                cardSpawnPoint = transform;
            }
            
            // If no aim transform specified, use parent transform
            if (aimTransform == null)
            {
                aimTransform = transform.parent != null ? transform.parent : transform;
            }
            
            // Make sure we have a projectile prefab
            if (cardProjectilePrefab == null)
            {
                Debug.LogError($"FlickAbility {abilityName} requires a cardProjectilePrefab!");
            }
        }

        protected override void ExecuteAbilityEffect()
        {
            // Start the card throwing coroutine
            StartCoroutine(ThrowCardsSequence());
        }

        private IEnumerator ThrowCardsSequence()
        {
            // Get target position
            Vector3 targetPosition = GetTargetPosition();

            // Throw each card with a slight delay between
            for (int i = 0; i < cardsPerFlick; i++)
            {
                ThrowCard(targetPosition, i);
                
                // Wait before throwing next card
                if (i < cardsPerFlick - 1)
                {
                    yield return new WaitForSeconds(cardFireDelay);
                }
            }
        }

        private Vector3 GetTargetPosition()
        {
            if (useTargeting)
            {
                // Try to find a target using raycasting
                RaycastHit hit;
                if (Physics.Raycast(aimTransform.position, aimTransform.forward, out hit, targetingRange, targetLayers))
                {
                    return hit.point;
                }
            }
            
            // If no target found or not using targeting, use a point in front of the player
            return aimTransform.position + aimTransform.forward * targetingRange;
        }

        private void ThrowCard(Vector3 targetPosition, int cardIndex)
        {
            if (cardProjectilePrefab == null)
                return;
                
            // Calculate direction to target with slight spread
            Vector3 directionToTarget = (targetPosition - cardSpawnPoint.position).normalized;
            
            // Apply spread if throwing multiple cards
            if (cardsPerFlick > 1 && cardIndex > 0)
            {
                // Alternate left/right spread based on card index
                float spreadFactor = ((cardIndex % 2 == 0) ? 1 : -1) * (cardIndex / 2 + 1);
                directionToTarget = Quaternion.Euler(0, spreadFactor * spreadAngle, 0) * directionToTarget;
            }
            
            // Create the card projectile
            GameObject cardObject = Instantiate(cardProjectilePrefab, cardSpawnPoint.position, Quaternion.LookRotation(directionToTarget));
            
            // Check if the projectile has a rigidbody
            Rigidbody rb = cardObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Add force to the card
                rb.velocity = directionToTarget * cardSpeed;
            }
            else
            {
                // If no rigidbody, add a CardProjectile component
                CardProjectile projectile = cardObject.GetComponent<CardProjectile>();
                if (projectile == null)
                {
                    projectile = cardObject.AddComponent<CardProjectile>();
                }
                
                // Configure the projectile
                projectile.Initialize(directionToTarget, cardSpeed, baseDamage, cardLifetime, criticalHitChance, criticalHitDamageMultiplier);
                projectile.OnProjectileHit += OnCardHit;
            }
            
            // Destroy the card after its lifetime
            Destroy(cardObject, cardLifetime);
            
            Debug.Log($"Threw card #{cardIndex+1}");
        }
        
        private void OnCardHit(GameObject target, bool isCritical)
        {
            // Apply damage to the target
            ApplyDamage(target, baseDamage, isCritical);
            
            // Spawn hit VFX
            if (abilityVFXPrefab != null)
            {
                GameObject vfx = Instantiate(abilityVFXPrefab, target.transform.position, Quaternion.identity);
                Destroy(vfx, 1.5f);
            }
        }
        
        // Visualize the targeting in the editor
        private void OnDrawGizmosSelected()
        {
            if (aimTransform == null)
                return;
                
            Gizmos.color = Color.blue;
            
            // Draw targeting range
            Gizmos.DrawRay(aimTransform.position, aimTransform.forward * targetingRange);
            
            // Draw card spread if multiple cards
            if (cardsPerFlick > 1)
            {
                Gizmos.color = Color.cyan;
                
                for (int i = 1; i < cardsPerFlick; i++)
                {
                    // Alternate left/right spread
                    float spreadFactor = ((i % 2 == 0) ? 1 : -1) * (i / 2 + 1);
                    Vector3 spreadDir = Quaternion.Euler(0, spreadFactor * spreadAngle, 0) * aimTransform.forward;
                    Gizmos.DrawRay(aimTransform.position, spreadDir * targetingRange);
                }
            }
        }
    }
    
    /// <summary>
    /// Component to handle card projectile behavior
    /// </summary>
    public class CardProjectile : MonoBehaviour
    {
        // Projectile properties
        private Vector3 direction;
        private float speed;
        private float damage;
        private float lifetime;
        private float criticalHitChance;
        private float criticalHitMultiplier;
        
        // Tracking
        private float elapsedTime = 0f;
        private bool hasHit = false;
        
        // Event for hit notification
        public event Action<GameObject, bool> OnProjectileHit;
        
        public void Initialize(Vector3 direction, float speed, float damage, float lifetime, float critChance, float critMultiplier)
        {
            this.direction = direction;
            this.speed = speed;
            this.damage = damage;
            this.lifetime = lifetime;
            this.criticalHitChance = critChance;
            this.criticalHitMultiplier = critMultiplier;
        }
        
        private void Update()
        {
            if (hasHit)
                return;
                
            // Move the projectile
            transform.position += direction * speed * Time.deltaTime;
            
            // Rotate the card for visual effect
            transform.Rotate(Vector3.forward, 720f * Time.deltaTime);
            
            // Track lifetime
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasHit)
                return;
                
            hasHit = true;
            
            // Roll for critical hit
            bool isCritical = UnityEngine.Random.value < criticalHitChance;
            
            // Notify of hit
            OnProjectileHit?.Invoke(other.gameObject, isCritical);
            
            // Stick to the hit surface or destroy
            StickToSurface(other);
        }
        
        private void StickToSurface(Collider hitCollider)
        {
            // Stop movement
            speed = 0;
            
            // Optional: Parent to hit object to stick to it
            // transform.SetParent(hitCollider.transform);
            
            // Adjust rotation to look like it's stuck in the surface
            transform.rotation = Quaternion.LookRotation(-direction);
            
            // Destroy after a short delay
            Destroy(gameObject, 2f);
        }
    }
    
    /// <summary>
    /// Interface for objects that can take damage
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}
