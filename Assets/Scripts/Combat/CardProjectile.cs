using UnityEngine;
using System;

namespace PDXUnderground.Combat
{
    /// <summary>
    /// Handles the behavior of card projectiles for the Gambler's flick ability
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
        
        // Visual settings
        [SerializeField] private TrailRenderer cardTrail;
        [SerializeField] private float rotationSpeed = 720f; // Degrees per second
        
        // Event for hit notification
        public event Action<GameObject, bool> OnProjectileHit;
        
        /// <summary>
        /// Initialize the projectile with the specified properties
        /// </summary>
        public void Initialize(Vector3 direction, float speed, float damage, float lifetime, float critChance, float critMultiplier)
        {
            this.direction = direction;
            this.speed = speed;
            this.damage = damage;
            this.lifetime = lifetime;
            this.criticalHitChance = critChance;
            this.criticalHitMultiplier = critMultiplier;
            
            // Enable trail if it exists
            if (cardTrail != null)
            {
                cardTrail.enabled = true;
            }
        }
        
        /// <summary>
        /// Update the projectile position and rotation
        /// </summary>
        private void Update()
        {
            if (hasHit)
                return;
                
            // Move the projectile
            transform.position += direction * speed * Time.deltaTime;
            
            // Rotate the card for visual effect
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            
            // Track lifetime
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Handle collision with other objects
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            HandleCollision(other.gameObject);
        }
        
        /// <summary>
        /// Handle collision (also called by OnCollisionEnter to handle different collider types)
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision.gameObject);
        }
        
        /// <summary>
        /// Common collision handling logic
        /// </summary>
        private void HandleCollision(GameObject hitObject)
        {
            if (hasHit)
                return;
                
            hasHit = true;
            
            // Roll for critical hit
            bool isCritical = UnityEngine.Random.value < criticalHitChance;
            
            // Notify of hit
            OnProjectileHit?.Invoke(hitObject, isCritical);
            
            // Stick to the hit surface or destroy
            StickToSurface(hitObject);
        }
        
        /// <summary>
        /// Stick the card to the hit surface
        /// </summary>
        private void StickToSurface(GameObject hitObject)
        {
            // Stop movement
            speed = 0;
            
            // Disable any trail
            if (cardTrail != null)
            {
                cardTrail.enabled = false;
            }
            
            // Adjust rotation to look like it's stuck in the surface
            transform.rotation = Quaternion.LookRotation(-direction);
            
            // Try to parent to hit object if it's not a static object
            if (hitObject != null && !hitObject.isStatic)
            {
                transform.SetParent(hitObject.transform);
            }
            
            // Destroy after a short delay
            Destroy(gameObject, 2f);
        }
    }
}

