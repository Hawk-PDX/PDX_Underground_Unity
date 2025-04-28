
using UnityEngine;
using DG.Tweening;
using PDXUnderground.Minigames;

namespace PDXUnderground.UI
{
    public class PokerTableVisuals : MonoBehaviour
    {
        [Header("References")]
        public PokerTable pokerTable;
        public PokerRules rules;
        public GameObject chipPrefab;
        public Transform[] playerChipStacks;
        public Transform potLocation;
        
        [Header("Effects")]
        public ParticleSystem chipSfx;
        public ParticleSystem winSfx;
        public AudioSource audioSource;
        public AudioClip chipSound;
        public AudioClip winSound;
        public AudioClip loseSound;

        private GameObject[] currentChipStacks;
        private GameObject currentPot;

        private void Start()
        {
            currentChipStacks = new GameObject[playerChipStacks.Length];
            pokerTable.OnPotChanged += UpdatePotVisual;
            pokerTable.OnPlayerBet += UpdatePlayerStack;
            pokerTable.OnHandComplete += ShowHandResult;
        }

        private void UpdatePlayerStack(int playerIndex, int amount)
        {
            if (currentChipStacks[playerIndex] != null)
            {
                Destroy(currentChipStacks[playerIndex]);
            }

            // Calculate stack height based on chip count
            float height = Mathf.Min(amount / 50f * rules.chipStackHeight, 1f);
            
            // Create new stack
            currentChipStacks[playerIndex] = Instantiate(
                chipPrefab, 
                playerChipStacks[playerIndex].position + Vector3.up * height,
                Quaternion.identity
            );

            // Play chip sound
            audioSource.PlayOneShot(chipSound);
            chipSfx.Play();
        }

        private void UpdatePotVisual(int amount)
        {
            if (currentPot != null)
            {
                Destroy(currentPot);
            }

            if (amount > 0)
            {
                // Calculate pot height
                float height = Mathf.Min(amount / 100f * rules.chipStackHeight, 1.5f);
                
                // Create pot visualization
                currentPot = Instantiate(
                    chipPrefab,
                    potLocation.position + Vector3.up * height,
                    Quaternion.identity
                );

                // Animate chip movement
                currentPot.transform.DOShakePosition(
                    rules.chipMoveDuration, 
                    strength: 0.1f, 
                    vibrato: 10
                );
            }
        }

        private void ShowHandResult(PokerPlayer winner, int amount)
        {
            if (winner.isAI)
            {
                audioSource.PlayOneShot(loseSound);
            }
            else
            {
                audioSource.PlayOneShot(winSound);
                winSfx.Play();
                
                // Animate chips moving to winner
                if (currentPot != null)
                {
                    currentPot.transform.DOMove(
                        playerChipStacks[winner.SeatPosition].position,
                        rules.chipMoveDuration
                    ).OnComplete(() => Destroy(currentPot));
                }
            }
        }
    }
}

