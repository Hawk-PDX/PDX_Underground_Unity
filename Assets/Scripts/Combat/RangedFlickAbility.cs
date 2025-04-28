using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDXUnderground.Effects;

namespace PDXUnderground.Combat
{
    /// <summary>
    /// Ranged card-throwing ability for the Gambler character.
    /// Allows throwing cards as projectiles with configurable properties.
    /// </summary>
    public class RangedFlickAbility : CombatAbility
    {
        [Header("Flick Ability Properties")]
        [Tooltip("Prefab for card projectile")]
        [SerializeField] private GameObject cardProjectilePrefab;
        
        [Tooltip("Position where cards spawn from")]
        [SerializeField] private Transform cardSpawnPoint;
        
        [Tooltip("Speed of thrown cards")]
        [SerializeField] private float cardSpeed = 20f;
        
        [Tooltip("How long cards exist before self-destructing")]
        [SerializeField] private float cardLifetime = 3f;
        
        [Tooltip("Number of cards thrown per flick")]
        [SerializeField] private int cardsPerFlick = 1;
        
        [Tooltip("Spread angle between multiple cards")]
        [SerializeField] private float spreadAngle = 8f;
        
        [Tooltip("Delay between throwing multiple cards")]
        [SerializeField] private float cardFireDelay = 0.1f;
        
        [Header("Card Targeting")]
        [Tooltip("Whether to use targeting for card throws")]
        [SerializeField] private bool useTargeting = true;
        
        [Tooltip("Maximum range for card targeting")]
        [SerializeField] private float targetingRange = 20f;
        
        [Tooltip("Layers that cards can target")]
        [SerializeField] private LayerMask targetLayers;
        
        [Tooltip("Transform to use for aiming direction")]
        [SerializeField] private Transform aimTransform;

        [Header("Card Damage Properties")]
        [Tooltip("Base damage multiplier based on card value")]
        [SerializeField] private float damageMultiplier = 1.0f;
        
        [Tooltip("Buzz return on successful hit")]
        [SerializeField] private float buzzReturnOnHit = 2f;
        
        [Tooltip("Additional buzz return on critical hit")]
        [SerializeField] private float criticalBuzzReturn = 5f;

        [Header("Card Visuals")]
        [Tooltip("Whether to use random card face visuals")]
        [SerializeField] private bool useRandomCardFaces = true;
        
        [Tooltip("Card sound on throw")]
        [SerializeField] private AudioClip cardThrowSound;
        
        [Tooltip("Card sound on hit")]
        [SerializeField] private AudioClip cardHitSound;

        // References
        private CardEffectsController cardEffects;
        private Camera mainCamera;

        /// <summary>
        /// Initialize references and set defaults
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // Set default animation trigger for flick if not specified
            if (string.IsNullOrEmpty(animationTriggerName))
            {
                animationTriggerName = "Flick";
            }
            
            // If no card spawn point specified, use this transform
            if (cardSpawnPoint == null)
            {
                cardSpawnPoint = transform;
            }
            
            // If no aim transform specified, use parent transform
            if (aimTransform == null)
            {
                aimTransform = transform.parent != null ? transform.parent : transform;
            }
            
            // Find references
            cardEffects = FindObjectOfType<CardEffectsController>();
            mainCamera = Camera.main;
            
            // Check if we have a card projectile prefab
            if (cardProjectilePrefab == null)
            {
                Debug.LogError($"RangedFlickAbility {abilityName} requires a cardProjectilePrefab!");
            }
        }

        /// <summary>
        /// Initialize the ability with the specified parameters
        /// </summary>
        public void Initialize(float range, float damage, float cooldown)
        {
            targetingRange = range;
            baseDamage = damage;
            cooldownTime = cooldown;
            
            // Set some sensible defaults based on parameters
            cardSpeed = range * 2f; // Higher speed for longer range
            cardLifetime = (range / cardSpeed) * 2f; // Double the time it would take to reach max range
            
            Debug.Log($"Initialized RangedFlickAbility: Range={range}, Damage={damage}, Cooldown={cooldown}");
        }

        /// <summary>
        /// Execute the card throwing ability
        /// </summary>
        protected override void ExecuteAbilityEffect()
        {
            // Start the card throwing coroutine
            StartCoroutine(ThrowCardsSequence());
        }

        /// <summary>
        /// Throw a sequence of cards with delay between throws
        /// </summary>
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

        /// <summary>
        /// Get the target position for the card throw
        /// </summary>
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
                
                // If mouse targeting is enabled and we have a camera
                if (mainCamera != null && Input.GetMouseButton(0))
                {
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, targetingRange, targetLayers))
                    {
                        return hit.point;
                    }
                }
            }
            
            // If no target found or not using targeting, use a point in front of the player
            return aimTransform.position + aimTransform.forward * targetingRange;
        }

        /// <summary>
        /// Throw a single card
        /// </summary>
        /// <param name="targetPosition">Position to aim at</param>
        /// <param name="cardIndex">Index of this card in the sequence (for spread calculation)</param>
        private void ThrowCard(Vector3 targetPosition, int cardIndex)
        {
            if (cardProjectilePrefab == null)
                return;

            // Calculate direction to target
            Vector3 directionToTarget = (targetPosition - cardSpawnPoint.position).normalized;
            
            // Apply spread if throwing multiple cards
            if (cardsPerFlick > 1 && cardIndex > 0)
            {
                // Alternate left/right spread based on card index
                float spreadFactor = ((cardIndex % 2 == 0) ? 1 : -1) * (cardIndex / 2 + 1);
                directionToTarget = Quaternion.Euler(0, spreadFactor * spreadAngle, 0) * directionToTarget;
            }

            // Determine if this will be a critical hit
            bool isCritical = RollForCriticalHit();
            
            // Choose card type (0-3 representing suits)
            int cardType = useRandomCardFaces ? Random.Range(0, 4) : 0;
            
            // Play visual effects if available
            GameObject cardVisual = null;
            if (cardEffects != null)
            {
                cardVisual = cardEffects.PlayCardThrowEffect(
                    cardSpawnPoint.position, 
                    directionToTarget, 
                    cardSpeed,
                    cardType,
                    isCritical
                );
            }
            
            // If card effects are not available, create the card projectile directly
            GameObject cardObject;
            if (cardVisual != null)
            {
                cardObject = cardVisual;
            }
            else
            {
                // Create the card projectile directly
                cardObject = Instantiate(cardProjectilePrefab, cardSpawnPoint.position, Quaternion.LookRotation(directionToTarget));
            }
            
            // Check if the projectile has a rigidbody
            Rigidbody rb = cardObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Add force to the card
                rb.velocity = directionToTarget * cardSpeed;
            }
            
            // Add a CardProjectile component to handle hit detection and damage
            CardProjectile projectile = cardObject.GetComponent<CardProjectile>();
            if (projectile == null)
            {
                projectile = cardObject.AddComponent<CardProjectile>();
            }
            
            // Configure the projectile
            projectile.Initialize(
                directionToTarget, 
                cardSpeed, 
                baseDamage * damageMultiplier,
                cardLifetime, 
                criticalHitChance, 
                criticalHitDamageMultiplier
            );
            
            // Subscribe to hit event
            projectile.OnProjectileHit += OnCardHit;
            
            // Play throw sound if available
            if (cardThrowSound != null)
            {
                AudioSource.PlayClipAtPoint(cardThrowSound, cardSpawnPoint.position);
            }
            
            Debug.Log($"Threw card #{cardIndex+1}, Critical: {isCritical}");
        }

        /// <summary>
        /// Handle a card hitting a target
        /// </summary>
        /// <param name="target">The hit target</param>
        /// <param name="isCritical">Whether this was a critical hit</param>
        private void OnCardHit(GameObject target, bool isCritical)
        {
            // Get the hit position and normal
            Vector3 hitPosition = target.transform.position;
            Vector3 hitNormal = Vector3.up; // Default if we can't determine actual normal
            
            // Try to get better hit info from a collider
            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider != null)
            {
                // Use center of collider for hit position
                hitPosition = targetCollider.bounds.center;
            }
            
            // Apply damage to the target
            ApplyDamage(target, baseDamage * damageMultiplier, isCritical);
            
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
            
            // Play hit sound if available
            if (cardHitSound != null)
            {
                AudioSource.PlayClipAtPoint(cardHitSound, hitPosition);
            }
            
            // Play impact visual effects if available
            if (cardEffects != null)
            {
                cardEffects.PlayCardImpactEffect(hitPosition, hitNormal, isCritical);
            }
            
            Debug.Log($"Card hit {target.name}, Critical: {isCritical}");
        }
        
        /// <summary>
        /// Visualize the flick ability range in the editor
        /// </summary>
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
}

