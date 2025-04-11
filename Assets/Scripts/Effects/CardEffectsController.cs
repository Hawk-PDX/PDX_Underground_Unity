using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PDXUnderground
{
    /// <summary>
    /// Controls visual effects for card-based abilities in the game.
    /// This class manages all throwing animations, impact effects, and critical hit visuals
    /// with period-appropriate styling for 1800s Portland setting.
    /// </summary>
    public class CardEffectsController : MonoBehaviour
    {
        #region Inspector Properties
        [Header("Card Visual Prefabs")]
        [Tooltip("Prefab for the standard playing card visual")]
        [SerializeField] private GameObject cardPrefab;
        [Tooltip("Array of card face textures (0-spades, 1-hearts, 2-diamonds, 3-clubs)")]
        [SerializeField] private Texture2D[] cardFaceTextures;
        [Tooltip("Texture for card backs")]
        [SerializeField] private Texture2D cardBackTexture;
        
        [Header("Card Trails")]
        [Tooltip("Trail renderer prefab for standard cards")]
        [SerializeField] private TrailRenderer standardCardTrail;
        [Tooltip("Trail renderer prefab for critical hit cards")]
        [SerializeField] private TrailRenderer criticalCardTrail;
        [Tooltip("Trail renderer prefab for low buzz cards")]
        [SerializeField] private TrailRenderer lowBuzzCardTrail;
        
        [Header("Impact Effects")]
        [Tooltip("Particle effect for standard card impacts")]
        [SerializeField] private ParticleSystem standardImpactEffect;
        [Tooltip("Particle effect for critical hit impacts")]
        [SerializeField] private ParticleSystem criticalImpactEffect;
        [Tooltip("Particle effect for card slicing")]
        [SerializeField] private ParticleSystem sliceEffect;
        
        [Header("Critical Hit Effects")]
        [Tooltip("Main particle effect for critical hits")]
        [SerializeField] private ParticleSystem criticalHitEffect;
        [Tooltip("Text popup prefab for critical hit")]
        [SerializeField] private GameObject criticalHitTextPrefab;
        
        [Header("Buzz Integration")]
        [Tooltip("Particle effect for high buzz card throws")]
        [SerializeField] private ParticleSystem highBuzzCardEffect;
        [Tooltip("Particle effect for low buzz card throws")]
        [SerializeField] private ParticleSystem lowBuzzCardEffect;
        [Tooltip("Particle effect for critical buzz card throws")]
        [SerializeField] private ParticleSystem criticalBuzzCardEffect;
        
        [Header("Environment Effects")]
        [Tooltip("Particle effect modification for tunnel environments")]
        [SerializeField] private Material tunnelCardMaterial;
        [Tooltip("Particle effect modification for speakeasy environments")]
        [SerializeField] private Material speakeasyCardMaterial;
        
        [Header("Pool Settings")]
        [Tooltip("Size of object pool for card visuals")]
        [SerializeField] private int cardPoolSize = 20;
        [Tooltip("Size of object pool for impact effects")]
        [SerializeField] private int impactPoolSize = 10;
        #endregion
        
        #region Private Variables
        // Object Pools
        private Queue<GameObject> cardPool = new Queue<GameObject>();
        private Queue<ParticleSystem> standardImpactPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> criticalImpactPool = new Queue<ParticleSystem>();
        private Queue<ParticleSystem> sliceEffectPool = new Queue<ParticleSystem>();
        private Queue<GameObject> criticalTextPool = new Queue<GameObject>();
        
        // Parent transform for organization
        private Transform cardPoolParent;
        private Transform effectPoolParent;
        
        // State tracking
        private int currentEnvironmentType = 0; // 0=streets, 1=tunnels, 2=speakeasy
        private GamblerCharacter.BuzzState currentBuzzState = GamblerCharacter.BuzzState.Normal;
        private GamblerCharacter gamblerCharacter;
        private Material defaultCardMaterial;
        
        // Cache
        private Dictionary<string, AudioClip> cardSounds = new Dictionary<string, AudioClip>();
        private Camera mainCamera;
        #endregion
        
        #region Unity Lifecycle Methods
        private void Awake()
        {
            // Create parent objects for pool organization
            cardPoolParent = new GameObject("Card_Pool").transform;
            cardPoolParent.SetParent(transform);
            
            effectPoolParent = new GameObject("Effect_Pool").transform;
            effectPoolParent.SetParent(transform);
            
            // Cache references
            mainCamera = Camera.main;
            gamblerCharacter = FindObjectOfType<GamblerCharacter>();
            
            if (cardPrefab != null && cardPrefab.GetComponent<Renderer>() != null)
            {
                defaultCardMaterial = cardPrefab.GetComponent<Renderer>().sharedMaterial;
            }
            
            // Initialize pools
            InitializeCardPool();
            InitializeEffectPools();
            
            // Load sound effects
            LoadCardSounds();
        }
        
        private void Start()
        {
            // Register with GamblerCharacter events if it exists
            if (gamblerCharacter != null)
            {
                gamblerCharacter.OnBuzzStateChanged += UpdateBuzzState;
                gamblerCharacter.OnCardUsed += HandleCardUsed;
                gamblerCharacter.OnCriticalHit += HandleCriticalHit;
                gamblerCharacter.OnAbilityUsed += HandleAbilityUsed;
            }
            else
            {
                Debug.LogWarning("CardEffectsController couldn't find GamblerCharacter in scene");
            }
        }
        
        private void OnDestroy()
        {
            // Unregister from GamblerCharacter events
            if (gamblerCharacter != null)
            {
                gamblerCharacter.OnBuzzStateChanged -= UpdateBuzzState;
                gamblerCharacter.OnCardUsed -= HandleCardUsed;
                gamblerCharacter.OnCriticalHit -= HandleCriticalHit;
                gamblerCharacter.OnAbilityUsed -= HandleAbilityUsed;
            }
        }
        #endregion
        
        #region Pool Initialization
        /// <summary>
        /// Initializes the pool of card game objects for reuse
        /// </summary>
        private void InitializeCardPool()
        {
            if (cardPrefab == null)
            {
                Debug.LogError("Card prefab is missing. Cannot initialize card pool.");
                return;
            }
            
            for (int i = 0; i < cardPoolSize; i++)
            {
                GameObject card = Instantiate(cardPrefab, cardPoolParent);
                card.SetActive(false);
                cardPool.Enqueue(card);
                
                // Add trail renderer if needed
                if (standardCardTrail != null && card.GetComponent<TrailRenderer>() == null)
                {
                    TrailRenderer trail = Instantiate(standardCardTrail, card.transform);
                    trail.enabled = false; // Will be enabled when used
                }
            }
            
            Debug.Log($"Initialized card pool with {cardPoolSize} cards");
        }
        
        /// <summary>
        /// Initializes pools for various particle effects
        /// </summary>
        private void InitializeEffectPools()
        {
            // Initialize standard impact effect pool
            if (standardImpactEffect != null)
            {
                for (int i = 0; i < impactPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(standardImpactEffect, effectPoolParent);
                    effect.gameObject.SetActive(false);
                    standardImpactPool.Enqueue(effect);
                }
            }
            
            // Initialize critical impact effect pool
            if (criticalImpactEffect != null)
            {
                for (int i = 0; i < impactPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(criticalImpactEffect, effectPoolParent);
                    effect.gameObject.SetActive(false);
                    criticalImpactPool.Enqueue(effect);
                }
            }
            
            // Initialize slice effect pool
            if (sliceEffect != null)
            {
                for (int i = 0; i < impactPoolSize; i++)
                {
                    ParticleSystem effect = Instantiate(sliceEffect, effectPoolParent);
                    effect.gameObject.SetActive(false);
                    sliceEffectPool.Enqueue(effect);
                }
            }
            
            // Initialize critical text pool
            if (criticalHitTextPrefab != null)
            {
                for (int i = 0; i < impactPoolSize; i++)
                {
                    GameObject critText = Instantiate(criticalHitTextPrefab, effectPoolParent);
                    critText.SetActive(false);
                    criticalTextPool.Enqueue(critText);
                }
            }
            
            Debug.Log("Initialized effect pools");
        }
        
        /// <summary>
        /// Loads card-related sound effects
        /// </summary>
        private void LoadCardSounds()
        {
            // In a complete implementation, would load from resources or addressables
            // For now, placeholder to demonstrate the approach
            AudioClip[] sounds = Resources.LoadAll<AudioClip>("Audio/CardSounds");
            foreach (AudioClip clip in sounds)
            {
                cardSounds[clip.name] = clip;
            }
            
            Debug.Log($"Loaded {cardSounds.Count} card sound effects");
        }
        #endregion
        
        #region Public Effect Methods
        /// <summary>
        /// Plays the card throw effect at the specified position and direction
        /// </summary>
        /// <param name="startPosition">Starting position for the card</param>
        /// <param name="direction">Direction to throw the card</param>
        /// <param name="cardSpeed">Speed of the card</param>
        /// <param name="cardType">Type of card (suit index 0-3)</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        /// <returns>Reference to the card gameObject (can be null)</returns>
        public GameObject PlayCardThrowEffect(Vector3 startPosition, Vector3 direction, float cardSpeed = 20f, int cardType = 0, bool isCritical = false)
        {
            if (cardPool.Count == 0)
            {
                Debug.LogWarning("Card pool empty, cannot play card throw effect");
                return null;
            }
            
            // Get a card from the pool
            GameObject card = cardPool.Dequeue();
            card.transform.position = startPosition;
            card.transform.rotation = Quaternion.LookRotation(direction);
            card.SetActive(true);
            
            // Set the card texture based on type
            SetCardAppearance(card, cardType, isCritical);
            
            // Set up the trail based on buzz state and critical status
            ConfigureCardTrail(card, isCritical);
            
            // Launch the card
            StartCoroutine(AnimateCardThrow(card, direction, cardSpeed, isCritical));
            
            // Play appropriate sound effect
            PlayCardSound(isCritical ? "card_critical" : "card_throw");
            
            return card;
        }
        
        /// <summary>
        /// Plays the slice effect at the specified position
        /// </summary>
        /// <param name="position">Position to play the effect</param>
        /// <param name="rotation">Rotation of the effect</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        public void PlaySliceEffect(Vector3 position, Quaternion rotation, bool isCritical = false)
        {
            if (sliceEffectPool.Count == 0)
            {
                Debug.LogWarning("Slice effect pool empty");
                return;
            }
            
            // Get slice effect from pool
            ParticleSystem effect = sliceEffectPool.Dequeue();
            effect.transform.position = position;
            effect.transform.rotation = rotation;
            
            // Configure effect based on buzz and environment
            ConfigureParticleEffect(effect, isCritical);
            
            // Play the effect
            effect.gameObject.SetActive(true);
            effect.Play();
            
            // Play sound effect
            PlayCardSound(isCritical ? "slice_critical" : "slice_standard");
            
            // Return to pool after duration
            StartCoroutine(ReturnToPoolAfterDuration(effect.gameObject, effect.main.duration + 0.5f, () => sliceEffectPool.Enqueue(effect)));
            
            // Display critical text if needed
            if (isCritical)
            {
                ShowCriticalText(position + Vector3.up * 0.5f);
            }
        }
        
        /// <summary>
        /// Plays impact effect when a card hits something
        /// </summary>
        /// <param name="position">Position of the impact</param>
        /// <param name="normal">Surface normal of the impact</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        public void PlayCardImpactEffect(Vector3 position, Vector3 normal, bool isCritical = false)
        {
            // Select appropriate pool
            Queue<ParticleSystem> effectPool = isCritical ? criticalImpactPool : standardImpactPool;
            
            if (effectPool.Count == 0)
            {
                Debug.LogWarning($"{(isCritical ? "Critical" : "Standard")} impact effect pool empty");
                return;
            }
            
            // Get effect from pool
            ParticleSystem effect = effectPool.Dequeue();
            effect.transform.position = position;
            effect.transform.rotation = Quaternion.LookRotation(normal);
            
            // Configure effect based on buzz and environment
            ConfigureParticleEffect(effect, isCritical);
            
            // Play the effect
            effect.gameObject.SetActive(true);
            effect.Play();
            
            // Play sound effect
            PlayCardSound(isCritical ? "impact_critical" : "impact_standard");
            
            // Return to pool after duration
            StartCoroutine(ReturnToPoolAfterDuration(effect.gameObject, effect.main.duration + 0.5f, () => effectPool.Enqueue(effect)));
            
            // Display critical text if needed
            if (isCritical)
            {
                ShowCriticalText(position + Vector3.up * 0.5f);
            }
        }
        
        /// <summary>
        /// Play critical hit effect at specified position
        /// </summary>
        /// <param name="position">Position to show the effect</param>
        public void PlayCriticalHitEffect(Vector3 position)
        {
            if (criticalHitEffect == null)
                return;
                
            // Create the effect at position
            ParticleSystem effect = Instantiate(criticalHitEffect, position, Quaternion.identity);
            
            // Configure based on environment
            ConfigureParticleEffect(effect, true);
            
            // Play effect 
            effect.Play();
            
            // Play sound
            PlayCardSound("critical_hit");
            
            // Clean up after effect duration
            Destroy(effect.gameObject, effect.main.duration + 0.5f);
            
            //

