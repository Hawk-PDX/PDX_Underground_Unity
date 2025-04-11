using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PDXUnderground.Interaction
{
    /// <summary>
    /// Represents a collectible item in the Shanghai Tunnels with historical significance.
    /// These items can be documents, artifacts, or other objects from Portland's gold rush era.
    /// </summary>
    public class CollectibleItem : MonoBehaviour
    {
        #region Inspector Properties

        [Header("Item Properties")]
        [SerializeField] private string itemName = "Artifact";
        [Tooltip("Historical description of this item")]
        [TextArea(2, 5)]
        [SerializeField] private string itemDescription = "A historical artifact from Portland's past.";
        [SerializeField] private ItemType itemType = ItemType.Artifact;
        [SerializeField] private int value = 10;
        [SerializeField] private Sprite itemIcon;
        [SerializeField] private bool isStoryRequired = false;
        [SerializeField] private string itemID;

        [Header("Visual & Audio Effects")]
        [SerializeField] private GameObject pickupEffect;
        [SerializeField] private Light highlightLight;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private float highlightIntensity = 1.5f;
        [SerializeField] private float highlightRange = 3f;

        [Header("Collection Events")]
        public UnityEvent OnCollected;

        #endregion

        #region Public Properties

        // Reference to the Shanghai Tunnels system
        [HideInInspector] public ShanghaiTunnels tunnels;

        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public ItemType Type => itemType;
        public int Value => value;
        public Sprite Icon => itemIcon;
        public bool IsStoryRequired => isStoryRequired;
        public string ItemID => string.IsNullOrEmpty(itemID) ? itemName : itemID;

        #endregion

        #region Private Variables

        private bool isCollected = false;
        private bool playerInRange = false;
        private Transform visualTransform;
        private float originalIntensity;
        private Material originalMaterial;
        private Material highlightMaterial;
        private Renderer itemRenderer;
        private Collider itemCollider;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Set up initial references
            itemCollider = GetComponent<Collider>();
            if (itemCollider == null)
            {
                itemCollider = gameObject.AddComponent<SphereCollider>();
                ((SphereCollider)itemCollider).radius = 0.5f;
                itemCollider.isTrigger = true;
            }

            // Find the visual part of the collectible
            visualTransform = transform.Find("Visual");
            if (visualTransform == null && transform.childCount > 0)
            {
                visualTransform = transform.GetChild(0);
            }

            // Get renderer for visual effects
            if (visualTransform != null)
            {
                itemRenderer = visualTransform.GetComponent<Renderer>();
                if (itemRenderer != null && itemRenderer.material != null)
                {
                    originalMaterial = itemRenderer.material;
                }
            }

            // Create highlight material
            if (originalMaterial != null)
            {
                highlightMaterial = new Material(originalMaterial);
                highlightMaterial.EnableKeyword("_EMISSION");
                highlightMaterial.SetColor("_EmissionColor", highlightColor * 2f);
            }

            // Set up highlight light
            if (highlightLight == null)
            {
                GameObject lightObj = new GameObject("HighlightLight");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = Vector3.up * 0.5f;
                
                highlightLight = lightObj.AddComponent<Light>();
                highlightLight.type = LightType.Point;
                highlightLight.color = highlightColor;
                highlightLight.intensity = highlightIntensity;
                highlightLight.range = highlightRange;
            }
            
            originalIntensity = highlightLight.intensity;
            
            // Start with light dimmed
            highlightLight.intensity = originalIntensity * 0.2f;
        }

        private void Start()
        {
            // Generate unique ID if needed
            if (string.IsNullOrEmpty(itemID))
            {
                itemID = System.Guid.NewGuid().ToString().Substring(0, 8);
            }
            
            // Start pulse effect
            StartCoroutine(PulseHighlight());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isCollected)
            {
                playerInRange = true;
                HighlightItem(true);
                ShowCollectionPrompt(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                HighlightItem(false);
                ShowCollectionPrompt(false);
            }
        }

        private void Update()
        {
            // Check for collection input
            if (playerInRange && !isCollected && Input.GetKeyDown(KeyCode.E))
            {
                CollectItem();
            }
        }

        #endregion

        #region Collection Methods

        /// <summary>
        /// Collects the item and triggers relevant effects and events
        /// </summary>
        public void CollectItem()
        {
            if (isCollected)
                return;

            isCollected = true;
            
            // Play pickup effect
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            // Play pickup sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Hide the collectible
            if (visualTransform != null)
            {
                visualTransform.gameObject.SetActive(false);
            }
            
            // Disable the collider
            if (itemCollider != null)
            {
                itemCollider.enabled = false;
            }
            
            // Disable the highlight light
            if (highlightLight != null)
            {
                highlightLight.enabled = false;
            }
            
            // Add to player's collection
            if (tunnels != null)
            {
                tunnels.RegisterCollectedItem(this);
            }
            
            // Show collection UI
            ShowCollectionUI();
            
            // Fire collection event
            OnCollected?.Invoke();
            
            // Destroy the object after delay
            StartCoroutine(DestroyAfterDelay(2f));
        }

        /// <summary>
        /// Highlight the item when player is nearby
        /// </summary>
        private void HighlightItem(bool highlight)
        {
            // Adjust light intensity
            if (highlightLight != null)
            {
                highlightLight.intensity = highlight ? originalIntensity : originalIntensity * 0.2f;
            }
            
            // Change material to highlight version
            if (itemRenderer != null && highlightMaterial != null && originalMaterial != null)
            {
                itemRenderer.material = highlight ? highlightMaterial : originalMaterial;
            }
        }

        /// <summary>
        /// Show floating prompt for collection
        /// </summary>
        private void ShowCollectionPrompt(bool show)
        {
            // Implementation would connect to a UI manager
            if (show)
            {
                Debug.Log($"Press E to collect: {itemName}");
            }
        }

        /// <summary>
        /// Show UI with collected item information
        /// </summary>
        private void ShowCollectionUI()
        {
            // Implementation would show UI with item details
            Debug.Log($"Collected: {itemName} - {itemDescription}");
        }

        /// <summary>
        /// Create pulsing effect for the highlight light
        /// </summary>
        private IEnumerator PulseHighlight()
        {
            while (!isCollected && highlightLight != null)
            {
                float baseIntensity = playerInRange ? originalIntensity : originalIntensity * 0.2f;
                float time = 0f;
                float duration = 1.5f;
                float minMultiplier = 0.8f;
                float maxMultiplier = 1.2f;
                
                // Pulse up
                while (time < duration/2 && !isCollected)
                {
                    time += Time.deltaTime;
                    float t = time / (duration/2);
                    float multiplier = Mathf.Lerp(minMultiplier, maxMultiplier, t);
                    highlightLight.intensity = baseIntensity * multiplier;
                    yield return null;
                }
                
                // Pulse down
                while (time < duration && !isCollected)
                {
                    time += Time.deltaTime;
                    float t = (time - duration/2) / (duration/2);
                    float multiplier = Mathf.Lerp(maxMultiplier, minMultiplier, t);
                    highlightLight.intensity = baseIntensity * multiplier;
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Destroy the collectible object after delay
        /// </summary>
        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }

        #endregion
    }

    /// <summary>
    /// Types of collectible items available in the game
    /// </summary>
    public enum ItemType
    {
        Artifact,
        Document,
        Photograph,
        Currency,
        Tool,
        Clothing,
        Jewelry,
        Contraband,
        KeyItem
    }
}

