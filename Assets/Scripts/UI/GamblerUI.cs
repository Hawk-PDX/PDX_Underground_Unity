using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        private List<Card> currentHand = new List<Card>();
        
        // Current buzz state
        private GamblerCharacter.BuzzState currentBuzzState = GamblerCharacter.BuzzState.Normal;

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

            // Subscribe to events
            SubscribeToEvents();
            
            InitializeCardDisplay();
            InitializeCardDisplay();
            
            // Initialize UI with current values
            UpdateHealthDisplay(gambler.GetCurrentHealth(), gambler.GetMaxHealth());
            UpdateBuzzMeter(gambler.GetCurrentBuzzPercentage(), gambler.GetMaxBuzz());
            UpdateBuzzState(gambler.GetCurrentBuzzState());
        
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

        private void Update()
        {
            if (gambler == null) return;
            
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
        
        private void UpdateBuzzState(GamblerCharacter.BuzzState state)
        {
            currentBuzzState = state;
            
            // Update visual appearance based on state
            switch (state)
            {
                case GamblerCharacter.BuzzState.Normal:
                    buzzMeterFill.color = buzzNormalColor;
                    buzzStateText.text = "Normal";
                    buzzStateText.color = buzzNormalColor;
                    break;
                    
                case GamblerCharacter.BuzzState.Low:
                    buzzMeterFill.color = buzzLowColor;
                    buzzStateText.text = "Low Buzz!";
                    buzzStateText.color = buzzLowColor;
                    // Add pulsing effect for low buzz
                    break;
                    
                case GamblerCharacter.BuzzState.Critical:
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
        private void UpdateCardHand(List<Card> hand)
        {
            currentHand = new List<Card>(hand); // Store a copy of the current hand
            
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
                Card card = hand[i];
                
                cardUI.cardObject.SetActive(true);
                cardUI.handIndex = i;
                
                // Update card display
                cardUI.cardName.text = card.name;
                cardUI.energyCost.text = card.buzzCost.ToString();
                cardUI.description.text = card.description;
                
                // Set card color and icon based on type
                SetCardAppearance(cardUI, card);
                
                // Set cooldown overlay
                cardUI.cooldownOverlay.gameObject.SetActive(card.isOnCooldown);
                if (card.isOnCooldown)
                {
                    cardUI.cooldownOverlay.fillAmount = card.remainingCooldown / card.cooldown;
                }
            }
        }
        
        private void SetCardAppearance(CardUI cardUI, Card card)
        {
            // Set card frame color based on card type
            switch (card.cardType)
            {
                case Card.CardType.Attack:
                    cardUI.cardFrame.color = attackCardColor;
                    // Set attack icon
                    SetCardTypeIcon(cardUI, "attack_icon");
                    break;
                    
                case Card.CardType.Defense:
                    cardUI.cardFrame.color = defenseCardColor;
                    // Set defense icon
                    SetCardTypeIcon(cardUI, "defense_icon");
                    break;
                    
                case Card.CardType.Recovery:
                    cardUI.cardFrame.color = recoveryCardColor;
                    // Set recovery icon
                    SetCardTypeIcon(cardUI, "recovery_icon");
                    break;
                    
                case Card.CardType.Special:
                    cardUI.cardFrame.color = specialCardColor;
                    // Set special icon
                    SetCardTypeIcon(cardUI, "special_icon");
                    break;
            }
            
            // If card cost is too high for current buzz, dim the card
            bool canAfford = true;
            if (gambler != null)
            {
                canAfford = gambler.GetCurrentBuzzPercentage() * gambler.GetMaxBuzz() >= card.buzzCost;
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
                    Card card = currentHand[cardUI.handIndex];
                    
                    if (card.isOnCooldown)
                    {
                        // Update cooldown fill amount
                        cardUI.cooldownOverlay.gameObject.SetActive(true);
                        cardUI.cooldownOverlay.fillAmount = card.remainingCooldown / card.cooldown;
                    }
                    else
                    {
                        cardUI.cooldownOverlay.gameObject.SetActive(false);
                    }
                }
            }
        }
        
        private void HandleCardUsed(Card card)
        {
            // This could play a card use animation or effect
            Debug.Log($"Card used: {card.name}");
            
            // You could add additional visual feedback here
            // such as particles, sound effects, or animations
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
