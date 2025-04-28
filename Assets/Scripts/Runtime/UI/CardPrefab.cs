using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.UI
{
    public class CardPrefab : MonoBehaviour
    {
        [Header("Card UI Elements")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image cardImage;
        [SerializeField] private Image frameImage;
        [SerializeField] private Image typeIcon;
        
        [Header("Card Stats")]
        [SerializeField] private TextMeshProUGUI energyCostText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI cooldownText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color attackColor = Color.red;
        [SerializeField] private Color defenseColor = Color.blue;
        [SerializeField] private Color utilityColor = Color.green;
        [SerializeField] private Color specialColor = Color.yellow;

        private ICardSystem.Card cardData;
        private bool isActive = false;

        public void SetCard(ICardSystem.Card card)
        {
            cardData = card;
            isActive = card != null;
            gameObject.SetActive(isActive);
            
            if (isActive)
            {
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (cardData == null) return;

            // Update text elements
            if (cardNameText != null) cardNameText.text = cardData.name;
            if (descriptionText != null) descriptionText.text = cardData.description;
            if (energyCostText != null) energyCostText.text = cardData.energyCost.ToString("F0");
            if (damageText != null) damageText.text = cardData.damage > 0 ? cardData.damage.ToString("F0") : "";
            if (cooldownText != null) cooldownText.text = cardData.cooldown > 0 ? $"CD: {cardData.cooldown:F1}s" : "";

            // Update card color based on type
            Color cardColor = GetCardColor(cardData.type);
            if (frameImage != null) frameImage.color = cardColor;

            // Update type icon if available
            if (typeIcon != null)
            {
                typeIcon.color = cardColor;
            }
        }

        private Color GetCardColor(ICardSystem.CardType type)
        {
            switch (type)
            {
                case ICardSystem.CardType.Attack: return attackColor;
                case ICardSystem.CardType.Defense: return defenseColor;
                case ICardSystem.CardType.Utility: return utilityColor;
                case ICardSystem.CardType.Special: return specialColor;
                default: return Color.white;
            }
        }

        public bool IsActive()
        {
            return isActive;
        }

        public ICardSystem.Card GetCardData()
        {
            return cardData;
        }
    }
}

