using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PDXUnderground.Core;
using PDXUnderground.Core.Interfaces;
using PDXUnderground.Player;

namespace PDXUnderground.UI
{
    public class GamblerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image buzzMeterFill;
        [SerializeField] private Image buzzMeterFrame;
        [SerializeField] private Transform cardHandContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private TextMeshProUGUI buzzStateText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private CardEffectsController cardEffectsController;
        
        [Header("UI Style")]
        [SerializeField] private Color buzzNormalColor = new Color(0.8f, 0.6f, 0.2f); // Golden
        [SerializeField] private Color buzzLowColor = new Color(0.7f, 0.3f, 0.1f);  // Reddish-brown
        [SerializeField] private Color buzzCriticalColor = new Color(0.8f, 0.1f, 0.1f);  // Red
        
        [Header("Card Colors")]
        [SerializeField] private Color attackCardColor = new Color(0.8f, 0.2f, 0.2f); // Red
        [SerializeField] private Color defenseCardColor = new Color(0.2f, 0.4f, 0.8f); // Blue
        [SerializeField] private Color recoveryCardColor = new Color(0.2f, 0.8f, 0.4f); // Green
        [SerializeField] private Color specialCardColor = new Color(0.8f, 0.6f, 0.0f); // Gold

        [Header("Card Display")]
        [SerializeField] private int maxCardsVisible = 3;
        [SerializeField] private float cardSpacing = 120f;
        
        // Reference to the Gambler character
        // Reference to the Gambler character
        private GamblerCharacter gambler;
        private CardUI[] displayedCards;
        private List<ICardSystem.Card> currentHand = new List<ICardSystem.Card>();
        
        // Current buzz state
        private IBuzzSystem.BuzzState currentBuzzState = IBuzzSystem.BuzzState.Normal;

        [System.Serializable]
        private class CardUI
        {
            public GameObject cardObject;
            public Image cardFrame;
            public Image cardTypeIcon;
            public TextMeshProUGUI cardName;
            public TextMeshProUGUI energyCost;
            public TextMeshProUGUI description;
            public Image cooldownOverlay;
            public int handIndex = -1; // Which card in the hand this UI represents
        }
        private void Start()
        {
            // Find the Gambler character in the scene
            gambler = FindObjectOfType<GamblerCharacter>();
            if (gambler == null)
            {
                Debug.LogError("No GamblerCharacter found in scene!");
                return;
            }
            
            // Find card effects controller if not set in inspector
            if (cardEffectsController == null)
            {
                cardEffectsController = FindObjectOfType<CardEffectsController>();
            }
            
            InitializeCardDisplay();
            
            // Initialize UI with current values
            UpdateHealthDisplay(gambler.GetCurrentHealth(), gambler.GetMaxHealth());
            
            // Get initial buzz state
            if (gambler is IBuzzSystem buzzSystem)
            {
                currentBuzzState = buzzSystem.CurrentBuzzState;
                UpdateBuzzState(currentBuzzState);
            }
            
            // Subscribe to events
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events when this component is destroyed
            if (gambler != null)
            {
                UnsubscribeFromEvents();
            }
        }
        
        private void SubscribeToEvents()
        {
            gambler.OnBuzzChanged += UpdateBuzzMeter;
            gambler.OnHealthChanged += UpdateHealthDisplay;
            gambler.OnBuzzStateChanged += UpdateBuzzState;
            gambler.OnHandChanged += UpdateCardHand;
            gambler.OnCardUsed += HandleCardUsed;
        }
        
        private void UnsubscribeFromEvents()
        {
            gambler.OnBuzzChanged -= UpdateBuzzMeter;
            gambler.OnHealthChanged -= UpdateHealthDisplay;
            gambler.OnBuzzStateChanged -= UpdateBuzzState;
            gambler.OnHandChanged -= UpdateCardHand;
            gambler.OnCardUsed -= HandleCardUsed;
        }
        public void UpdatePlayerUI(PokerPlayer player)
        {
            // Display whiskey effects
            if (player.isAI == false)
            {
                var cask = FindObjectOfType<SacredCaskSystem>();
                whiskeyQualityText.text = $"Whiskey: {cask.currentQuality}";
                
                // Show vengeance mode indicator
                vengeanceIndicator.SetActive(player.vengeanceBluffBonus > 0);
                
                // Display bilingual tells
                if (CulturalBridge.instance != null)
                {
                    tellText.text = CulturalBridge.instance.GetCurrentTells();
                }
            }
            
            // Update cooldown displays - other updates are handled by events
            UpdateCardCooldowns();
        }

        private void InitializeCardDisplay()
        {
            displayedCards = new CardUI[maxCardsVisible];

            // Create card UI elements
            for (int i = 0; i < maxCardsVisible; i++)
            {
                GameObject cardObj = Instantiate(cardPrefab, cardHandContainer);
                cardObj.GetComponent<RectTransform>().anchoredPosition = 
                    new Vector2(i * cardSpacing - (cardSpacing * (maxCardsVisible - 1) / 2f), 0);

                CardUI card = new CardUI
                {
                    cardObject = cardObj,
                    cardFrame = cardObj.GetComponent<Image>(),
                    cardTypeIcon = cardObj.transform.Find("TypeIcon").GetComponent<Image>(),
                    cardName = cardObj.transform.Find("Name").GetComponent<TextMeshProUGUI>(),
                    energyCost = cardObj.transform.Find("EnergyCost").GetComponent<TextMeshProUGUI>(),
                    description = cardObj.transform.Find("Description").GetComponent<TextMeshProUGUI>(),
                    cooldownOverlay = cardObj.transform.Find("CooldownOverlay").GetComponent<Image>()
                };

                // Add click handler
                int index = i; // Capture index for lambda
                cardObj.GetComponent<Button>().onClick.AddListener(() => OnCardClicked(index));

                displayedCards[i] = card;
                cardObj.SetActive(false); // Hide cards initially
            }
        }

        private void UpdateBuzzMeter(float currentBuzz, float maxBuzz)
        {
            float buzzPercentage = currentBuzz / maxBuzz;
            buzzMeterFill.fillAmount = buzzPercentage;

            // Color is now handled by the buzz state update
        }
        
        private void UpdateBuzzState(IBuzzSystem.BuzzState state)
        {
            currentBuzzState = state;
            
            // Update visual appearance based on state
            switch (state)
            {
                case IBuzzSystem.BuzzState.Normal:
                    buzzMeterFill.color = buzzNormalColor;
                    buzzStateText.text = "Normal";
                    buzzStateText.color = buzzNormalColor;
                    break;
                    
                case IBuzzSystem.BuzzState.Low:
                    buzzMeterFill.color = buzzLowColor;
                    buzzStateText.text = "Low Buzz!";
                    buzzStateText.color = buzzLowColor;
                    // Add pulsing effect for low buzz
                    break;
                    
                case IBuzzSystem.BuzzState.Critical:
                    buzzMeterFill.color = buzzCriticalColor;
                    buzzStateText.text = "CRITICAL!";
                    buzzStateText.color = buzzCriticalColor;
                    // Add stronger pulsing effect for critical buzz
                    break;
            }
        }
        
        private void UpdateHealthDisplay(float currentHealth, float maxHealth)
        {
            if (healthText != null)
            {
                healthText.text = $"Health: {Mathf.Round(currentHealth)}/{Mathf.Round(maxHealth)}";
            }
        }
        private void UpdateCardHand(List<ICardSystem.Card> hand)
        {
            currentHand = new List<ICardSystem.Card>(hand); // Store a copy of the current hand
            
            // Hide all card displays first
            foreach (var cardUI in displayedCards)
            {
                cardUI.cardObject.SetActive(false);
                cardUI.handIndex = -1;
            }
            
            // Show cards based on the current hand
            int cardCount = Mathf.Min(hand.Count, displayedCards.Length);
            for (int i = 0; i < cardCount; i++)
            {
                CardUI cardUI = displayedCards[i];
                ICardSystem.Card card = hand[i];
                
                cardUI.cardObject.SetActive(true);
                cardUI.handIndex = i;
                
                // Update card display
                cardUI.cardName.text = card.name;
                cardUI.energyCost.text = card.energyCost.ToString();
                cardUI.description.text = card.description;
                
                // Set card color and icon based on type
                SetCardAppearance(cardUI, card);
                
                // Set cooldown overlay
                // Update cooldown display based on ability cooldown
                float cooldown = gambler.GetAbilityCooldown(card.name);
                cardUI.cooldownOverlay.gameObject.SetActive(cooldown > 0);
                if (cooldown > 0)
                {
                    cardUI.cooldownOverlay.fillAmount = cooldown;
                }
            }
        }
        
        private void SetCardAppearance(CardUI cardUI, ICardSystem.Card card)
        {
            // Set card frame color based on card type
            switch (card.type)
            {
                case ICardSystem.CardType.Attack:
                    cardUI.cardFrame.color = attackCardColor;
                    // Set attack icon
                    SetCardTypeIcon(cardUI, "attack_icon");
                    break;
                    
                case ICardSystem.CardType.Defense:
                    cardUI.cardFrame.color = defenseCardColor;
                    // Set defense icon
                    SetCardTypeIcon(cardUI, "defense_icon");
                    break;
                    
                case ICardSystem.CardType.Utility:
                    cardUI.cardFrame.color = recoveryCardColor;
                    // Set recovery icon
                    SetCardTypeIcon(cardUI, "recovery_icon");
                    break;
                    
                case ICardSystem.CardType.Special:
                    cardUI.cardFrame.color = specialCardColor;
                    // Set special icon
                    SetCardTypeIcon(cardUI, "special_icon");
                    break;
            }
            
            // If card cost is too high for current buzz, dim the card
            bool canAfford = true;
            if (gambler != null)
            {
                canAfford = gambler.GetCurrentBuzzPercentage() * gambler.GetMaxBuzz() >= card.energyCost;
            }
            
            // Apply visual feedback for unplayable cards
            float alpha = canAfford ? 1.0f : 0.6f;
            Color frameColor = cardUI.cardFrame.color;
            cardUI.cardFrame.color = new Color(frameColor.r, frameColor.g, frameColor.b, alpha);
            cardUI.cardName.alpha = alpha;
            cardUI.description.alpha = alpha;
            cardUI.energyCost.alpha = alpha;
            cardUI.cardTypeIcon.color = new Color(cardUI.cardTypeIcon.color.r, 
                                               cardUI.cardTypeIcon.color.g, 
                                               cardUI.cardTypeIcon.color.b, 
                                               alpha);
        }
        
        private void SetCardTypeIcon(CardUI cardUI, string iconName)
        {
            // In a real implementation, you would load the appropriate icon from resources
            // For this example, we'll just set the color to indicate the card type
            Sprite iconSprite = Resources.Load<Sprite>($"UI/Icons/{iconName}");
            if (iconSprite != null)
            {
                cardUI.cardTypeIcon.sprite = iconSprite;
            }
        }
        private void UpdateCardCooldowns()
        {
            for (int i = 0; i < displayedCards.Length; i++)
            {
                CardUI cardUI = displayedCards[i];
                
                if (cardUI.handIndex >= 0 && cardUI.handIndex < currentHand.Count)
                {
                    ICardSystem.Card card = currentHand[cardUI.handIndex];
                    float cooldown = gambler.GetAbilityCooldown(card.name);
                    
                    if (cooldown > 0)
                    {
                        // Update cooldown fill amount
                        cardUI.cooldownOverlay.gameObject.SetActive(true);
                        cardUI.cooldownOverlay.fillAmount = cooldown;
                    }
                    else
                    {
                        cardUI.cooldownOverlay.gameObject.SetActive(false);
                    }
                }
            }
        }
        
        private void HandleCardUsed(ICardSystem.Card card)
        {
            // Play card use animation/effect if available
            if (cardEffectsController != null)
            {
                cardEffectsController.PlayCardUseEffect(card.type switch {
                    CardType.Attack => CardEffectType.Attack,
                    CardType.Defense => CardEffectType.Defense,
                    CardType.Utility => CardEffectType.Utility,
                    CardType.Special => CardEffectType.Special,
                    _ => CardEffectType.None
                });
            }
            
            Debug.Log($"Card used: {card.name}");
        }

        private void OnCardClicked(int index)
        {
            if (gambler == null) return;
            
            CardUI cardUI = displayedCards[index];
            if (cardUI.handIndex >= 0 && cardUI.handIndex < currentHand.Count)
            {
                // Use the correct handIndex from the UI element
                gambler.UseCard(cardUI.handIndex);
            }
        }
    }
}
