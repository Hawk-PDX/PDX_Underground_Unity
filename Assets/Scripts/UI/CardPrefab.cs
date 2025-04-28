using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.UI
{
    /// <summary>
    /// Represents a single card UI element in the PDX Underground game.
    /// Handles the visual representation of cards with period-appropriate styling.
    /// </summary>
    public class CardPrefab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Card Visual Elements")]
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private Image cardFrameImage;
        [SerializeField] private Image cardTypeIcon;
        [SerializeField] private Image cardBackground;
        [SerializeField] private TextMeshProUGUI energyCostText;
        [SerializeField] private Image cooldownOverlay;
        
        [Header("Card Type Styles")]
        [SerializeField] private Color attackCardColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color defenseCardColor = new Color(0.2f, 0.4f, 0.8f);
        [SerializeField] private Color utilityCardColor = new Color(0.8f, 0.8f, 0.2f);
        [SerializeField] private Color specialCardColor = new Color(0.6f, 0.2f, 0.8f);
        
        [SerializeField] private Sprite attackCardIcon;
        [SerializeField] private Sprite defenseCardIcon;
        [SerializeField] private Sprite utilityCardIcon;
        [SerializeField] private Sprite specialCardIcon;
        
        [Header("Card Animation")]
        [SerializeField] private float hoverDuration = 0.2f;
        [SerializeField] private float hoverHeight = 20f;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float animationSpeed = 10f;
        [SerializeField] private GameObject selectedIndicator;
        
        private Coroutine hoverCoroutine;
        
        private ICardSystem.Card cardData;
        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Vector2 originalPosition;
        private bool isHovered;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalScale = rectTransform.localScale;
                originalPosition = rectTransform.anchoredPosition;
            }
        }
        
        public void SetupCard(ICardSystem.Card card)
        {
            cardData = card;
            
            if (cardNameText != null)
                cardNameText.text = card.name;
                
            if (cardDescriptionText != null)
                cardDescriptionText.text = card.description;
            
            if (energyCostText != null)
                energyCostText.text = card.energyCost.ToString();
            
            UpdateCardVisuals(card.type);
        }
        
        private void UpdateCardVisuals(ICardSystem.CardType type)
        {
            Color cardColor = Color.white;
            Sprite typeIcon = null;
            
            switch (type)
            {
                case ICardSystem.CardType.Attack:
                    cardColor = attackCardColor;
                    typeIcon = attackCardIcon;
                    break;
                case ICardSystem.CardType.Defense:
                    cardColor = defenseCardColor;
                    typeIcon = defenseCardIcon;
                    break;
                case ICardSystem.CardType.Utility:
                    cardColor = utilityCardColor;
                    typeIcon = utilityCardIcon;
                    break;
                case ICardSystem.CardType.Special:
                    cardColor = specialCardColor;
                    typeIcon = specialCardIcon;
                    break;
            }
            
            if (cardBackground != null)
            {
                cardBackground.color = cardColor;
            }
            
            if (cardTypeIcon != null && typeIcon != null)
            {
                cardTypeIcon.sprite = typeIcon;
            }
            
            ApplyPeriodStyling();
        }
        
        private void ApplyPeriodStyling()
        {
            if (cardImage != null)
            {
                cardImage.color = new Color(0.95f, 0.9f, 0.8f);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            StopAllCoroutines();
            StartCoroutine(AnimateCard(true));
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            StopAllCoroutines();
            StartCoroutine(AnimateCard(false));
        }
        
        private System.Collections.IEnumerator AnimateCard(bool hovering)
        {
            Vector2 targetPos = hovering ? originalPosition + Vector2.up * hoverHeight : originalPosition;
            Vector3 targetScale = hovering ? originalScale * hoverScale : originalScale;
            
            while (true)
            {
                float t = Time.deltaTime * animationSpeed;
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, t);
                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, t);
                
                if (Vector2.Distance(rectTransform.anchoredPosition, targetPos) < 0.01f &&
                    Vector3.Distance(rectTransform.localScale, targetScale) < 0.01f)
                {
                    rectTransform.anchoredPosition = targetPos;
                    rectTransform.localScale = targetScale;
                    break;
                }
                
                yield return null;
            }
        }
        
        public void SetEnabled(bool enabled)
        {
            Color color = enabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
            if (cardFrameImage != null) cardFrameImage.color = color;
            if (cardTypeIcon != null) cardTypeIcon.color = color;
            if (cardNameText != null) cardNameText.color = color;
            if (energyCostText != null) energyCostText.color = color;
        }
        
        public ICardSystem.Card GetCardData()
        {
            return cardData;
        }
    }
}
