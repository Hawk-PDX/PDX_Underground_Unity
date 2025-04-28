using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.UI
{
    public class BuzzUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider buzzMeter;
        [SerializeField] private TextMeshProUGUI buzzLevelText;
        [SerializeField] private Image buzzStateIndicator;
        [SerializeField] private CardPrefab[] cardSlots;

        [Header("Visual Settings")]
        [SerializeField] private Color normalStateColor = Color.green;
        [SerializeField] private Color lowStateColor = Color.yellow;
        [SerializeField] private Color criticalStateColor = Color.red;

        private IBuzzSystem buzzSystem;

        private void Start()
        {
            // Find buzz system in scene
            buzzSystem = FindObjectOfType<PDXUnderground.Player.GamblerCharacter>();
            
            if (buzzSystem != null)
            {
                buzzSystem.OnBuzzChanged += UpdateBuzzUI;
                buzzSystem.OnBuzzStateChanged += UpdateBuzzState;
            }
        }

        private void OnDestroy()
        {
            if (buzzSystem != null)
            {
                buzzSystem.OnBuzzChanged -= UpdateBuzzUI;
                buzzSystem.OnBuzzStateChanged -= UpdateBuzzState;
            }
        }

        public void UpdateBuzzUI(float currentBuzz, float maxBuzz)
        {
            if (buzzMeter != null)
                buzzMeter.value = currentBuzz / maxBuzz;

            if (buzzLevelText != null)
                buzzLevelText.text = $"Buzz: {currentBuzz:F0}/{maxBuzz:F0}";
        }

        public void UpdateBuzzState(IBuzzSystem.BuzzState state)
        {
            if (buzzStateIndicator != null)
            {
                Color stateColor = normalStateColor;
                switch (state)
                {
                    case IBuzzSystem.BuzzState.Low:
                        stateColor = lowStateColor;
                        break;
                    case IBuzzSystem.BuzzState.Critical:
                        stateColor = criticalStateColor;
                        break;
                }
                buzzStateIndicator.color = stateColor;
            }
        }
    }
}

