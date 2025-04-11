
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace PDXUnderground
{
    /// <summary>
    /// Example MonoBehaviour that demonstrates usage of the BuzzUIController
    /// This can be attached to a GameObject in a test scene to quickly set up
    /// testing of the buzz system functionality.
    /// </summary>
    public class GamblerTestQuickStartExample : MonoBehaviour
    {
        [Header("References")]
        public BuzzUIController buzzUIController;
        public GamblerCharacter gamblerCharacter;
        
        [Header("Test Controls")]
        [SerializeField] private Button increaseBuzzButton;
        [SerializeField] private Button decreaseBuzzButton;
        [SerializeField] private Button criticalBuzzButton;
        [SerializeField] private Button normalBuzzButton;
        
        [Header("Environment Controls")]
        [SerializeField] private Button streetEnvironmentButton;
        [SerializeField] private Button tunnelEnvironmentButton;
        [SerializeField] private Button speakeasyEnvironmentButton;
        
        [Header("Test Cards")]
        [SerializeField] private Button drawCardButton;
        [SerializeField] private Button sliceCardButton;
        [SerializeField] private Button flickCardButton;
        
        private void Start()
        {
            // Initialize references if not set
            if (buzzUIController == null)
                buzzUIController = FindObjectOfType<BuzzUIController>();
                
            if (gamblerCharacter == null)
                gamblerCharacter = FindObjectOfType<GamblerCharacter>();
                
            // Set up button listeners
            SetupButtonListeners();
            
            // Log setup information
            Debug.Log("GamblerTest quick start example initialized!");
            Debug.Log("Use the UI buttons to test the buzz system functionality");
            Debug.Log("=====================================================");
            Debug.Log("Controls Guide:");
            Debug.Log("- Increase/Decrease: Change buzz level by 10");
            Debug.Log("- Critical/Normal: Set buzz to critical or full level");
            Debug.Log("- Environment buttons: Change environment effects");
            Debug.Log("- Card buttons: Test card abilities");
        }
        
        private void SetupButtonListeners()
        {
            // Buzz control buttons
            if (increaseBuzzButton != null)
                increaseBuzzButton.onClick.AddListener(IncreaseBuzz);
                
            if (decreaseBuzzButton != null)
                decreaseBuzzButton.onClick.AddListener(DecreaseBuzz);
                
            if (criticalBuzzButton != null)
                criticalBuzzButton.onClick.AddListener(SetCriticalBuzz);
                
            if (normalBuzzButton != null)
                normalBuzzButton.onClick.AddListener(SetNormalBuzz);
                
            // Environment buttons
            if (streetEnvironmentButton != null)
                streetEnvironmentButton.onClick.AddListener(() => ChangeEnvironment(0));
                
            if (tunnelEnvironmentButton != null)
                tunnelEnvironmentButton.onClick.AddListener(() => ChangeEnvironment(1));
                
            if (speakeasyEnvironmentButton != null)
                speakeasyEnvironmentButton.onClick.AddListener(() => ChangeEnvironment(2));
                
            // Card ability buttons
            if (drawCardButton != null)
                drawCardButton.onClick.AddListener(DrawCard);
                
            if (sliceCardButton != null)
                sliceCardButton.onClick.AddListener(UseSliceAbility);
                
            if (flickCardButton != null)
                flickCardButton.onClick.AddListener(UseFlickAbility);
        }
        
        // Buzz control functions
        private void IncreaseBuzz()
        {
            if (gamblerCharacter != null)
            {
                float newBuzz = Mathf.Min(gamblerCharacter.currentBuzz + 10f, gamblerCharacter.maxBuzz);
                gamblerCharacter.SetBuzzLevel(newBuzz);
                Debug.Log($"Increased Buzz to {newBuzz}");
            }
        }
        
        private void DecreaseBuzz()
        {
            if (gamblerCharacter != null)
            {
                float newBuzz = Mathf.Max(gamblerCharacter.currentBuzz - 10f, 0f);
                gamblerCharacter.SetBuzzLevel(newBuzz);
                Debug.Log($"Decreased Buzz to {newBuzz}");
            }
        }
        
        private void SetCriticalBuzz()
        {
            if (gamblerCharacter != null)
            {
                float criticalLevel = gamblerCharacter.criticalBuzzThreshold * 0.5f;
                gamblerCharacter.SetBuzzLevel(criticalLevel);
                Debug.Log($"Set Critical Buzz level: {criticalLevel}");
            }
        }
        
        private void SetNormalBuzz()
        {
            if (gamblerCharacter != null)
            {
                gamblerCharacter.SetBuzzLevel(gamblerCharacter.maxBuzz);
                Debug.Log($"Reset Buzz to maximum: {gamblerCharacter.maxBuzz}");
            }
        }
        
        // Environment functions
        private void ChangeEnvironment(int environmentType)
        {
            string[] environments = { "Streets", "Tunnels", "Speakeasy" };
            Debug.Log($"Changed environment to: {environments[environmentType]}");
            
            // In a real implementation, this would trigger environment-specific effects
            if (buzzUIController != null)
            {
                // Example of calling a method that would be part of BuzzUIController
                // buzzUIController.ShowEnvironmentNotification(environments[environmentType]);
                Debug.Log($"Changed UI theme to match {environments[environmentType]} environment");
            }
        }
        
        // Card ability functions
        private void DrawCard()
        {
            if (gamblerCharacter != null)
            {
                // In a real implementation, this would call gamblerCharacter.DrawCard()
                Debug.Log("Drew a new card");
            }
        }
        
        private void UseSliceAbility()
        {
            if (gamblerCharacter != null)
            {
                // In a real implementation, this would call gamblerCharacter.UseSliceAbility()
                Debug.Log("Used Slice ability");
                
                // This would normally be handled by events, but for testing:
                if (buzzUIController != null)
                {
                    // Example call to what would be an ability feedback method
                    // buzzUIController.ShowAbilityFeedback(GamblerCharacter.AbilityType.Slice);
                }
            }
        }
        
        private void UseFlickAbility()
        {
            if (gamblerCharacter != null)
            {
                // In a real implementation, this would call gamblerCharacter.UseFlickAbility()
                Debug.Log("Used Flick ability");
                
                // This would normally be handled by events, but for testing:
                if (buzzUIController != null)
                {
                    // Example call to what would be an ability feedback method
                    // buzzUIController.ShowAbilityFeedback(GamblerCharacter.AbilityType.Flick);
                }
            }
        }
    }
}

