using UnityEngine;
using System.Collections;
using System;

namespace PDXUnderground.Combat
{
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
        
        [Header("VFX")]
        [SerializeField] private GameObject impactVFXPrefab;
        [SerializeField] private float impactVFXDuration = 1.5f;

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
        
        /// <summary>
        /// Callback when a card projectile hits a target
        /// </summary>
        /// <param name="target">The hit target</param>
        /// <param name="isCritical">Whether it was a critical hit</param>
        private void OnCardHit(GameObject target, bool isCritical)
        {
            // Apply damage to the target
            ApplyDamage(target, baseDamage, isCritical);
            
            // Spawn impact VFX
            SpawnImpactVFX(target.transform.position, isCritical);
            
            // Log the hit
            Debug.Log($"Card hit {target.name} for {baseDamage * (isCritical ? criticalHitDamageMultiplier : 1f)} damage");
        }
        
        /// <summary>
        /// Spawn impact visual effects at the hit position
        /// </summary>
        private void SpawnImpactVFX(Vector3 position, bool isCritical)
        {
            // Use either the specific impact VFX or the general ability VFX
            GameObject vfxPrefab = impactVFXPrefab != null ? impactVFXPrefab : abilityVFXPrefab;
            
            if (vfxPrefab != null)
            {
                // Create the VFX
                GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);
                
                // Scale up for critical hits
                if (isCritical)
                {
                    vfx.transform.localScale *= 1.5f;
                }
                
                // Destroy after duration
                Destroy(vfx, impactVFXDuration);
            }
        }
        
        /// <summary>
        /// Validate that the ability has all required components
        /// </summary>
        private void OnValidate()
        {
            if (cardProjectilePrefab != null)
            {
                // Check if projectile has either a rigidbody or a CardProjectile component
                bool hasRigidbody = cardProjectilePrefab.GetComponent<Rigidbody>() != null;
                bool hasCardProjectile = cardProjectilePrefab.GetComponent<CardProjectile>() != null;
                
                if (!hasRigidbody && !hasCardProjectile)
                {
                    Debug.LogWarning($"Card projectile prefab should have either a Rigidbody or a CardProjectile component");
                }
            }
        }
        
        /// <summary>
        /// Draw targeting visualization in the editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Ensure we have a valid aim transform
            Transform aim = aimTransform != null ? aimTransform : transform;
            
            // Draw the targeting range
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(aim.position, aim.forward * targetingRange);
            
            // Draw a sphere at the end of the targeting ray
            Gizmos.DrawWireSphere(aim.position + aim.forward * targetingRange, 0.5f);
            
            // Draw the card spread if multiple cards
            if (cardsPerFlick > 1)
            {
                Gizmos.color = Color.cyan;
                
                for (int i = 1; i < cardsPerFlick; i++)
                {
                    // Alternate left/right spread
                    float spreadFactor = ((i % 2 == 0) ? 1 : -1) * (i / 2 + 1);
                    Vector3 spreadDir = Quaternion.Euler(0, spreadFactor * spreadAngle, 0) * aim.forward;
                    Gizmos.DrawRay(aim.position, spreadDir * targetingRange);
                }
            }
            
            // If spawn point is defined, show it
            if (cardSpawnPoint != null && cardSpawnPoint != transform)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(cardSpawnPoint.position, 0.2f);
            }
        }
    }
}

