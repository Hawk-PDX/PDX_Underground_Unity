using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace PDXUnderground.UI
{
    public class CardPrefab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Card Elements")]
        [SerializeField] private Image frameImage;
        [SerializeField] private Image typeIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI energyCostText;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private RectTransform cardRect;

        [Header("Hover Animation")]
        [SerializeField] private float hoverScale = 1.2f;
        [SerializeField] private float hoverHeight = 30f;
        [SerializeField] private float animationSpeed = 10f;

        private Vector2 originalPosition;
        private Vector3 originalScale;
        private bool isHovered = false;

        private void Awake()
        {
            cardRect = GetComponent<RectTransform>();
            originalPosition = cardRect.anchoredPosition;
            originalScale = cardRect.localScale;
        }

        public void SetCardInfo(string cardName, int energyCost, Sprite typeSprite, float cooldownProgress)
        {
            nameText.text = cardName;
            energyCostText.text = energyCost.ToString();
            typeIcon.sprite = typeSprite;
            cooldownOverlay.fillAmount = cooldownProgress;
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
                cardRect.anchoredPosition = Vector2.Lerp(cardRect.anchoredPosition, targetPos, t);
                cardRect.localScale = Vector3.Lerp(cardRect.localScale, targetScale, t);

                if (Vector2.Distance(cardRect.anchoredPosition, targetPos) < 0.01f &&
                    Vector3.Distance(cardRect.localScale, targetScale) < 0.01f)
                {
                    cardRect.anchoredPosition = targetPos;
                    cardRect.localScale = targetScale;
                    break;
                }

                yield return null;
            }
        }

        public void SetEnabled(bool enabled)
        {
            // Dim the card if disabled
            Color color = enabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
            frameImage.color = color;
            typeIcon.color = color;
            nameText.color = color;
            energyCostText.color = color;
        }
    }
}

