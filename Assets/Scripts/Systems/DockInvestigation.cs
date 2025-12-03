using UnityEngine;
using PDXUnderground.Player;
using PDXUnderground.Systems;
using System.Collections;

namespace PDXUnderground.Systems
{
    public class DockInvestigation : MonoBehaviour
    {
        [Header("Investigation Settings")]
        public float searchTimePerCrate = 10f;
        public float trustIncreasePerHelp = 5f;
        public float trustDecreasePerSuspicion = 10f;
        
        [Header("Trafficker Encounters")] 
        public float spottingRange = 15f;
        public float vengeanceDuration = 180f;
        
        private float dockworkerTrust = 50f;
        private bool vengeanceActive = false;

        public void SearchCrate()
        {
            StartCoroutine(SearchCoroutine());
        }

        private IEnumerator SearchCoroutine()
        {
            yield return new WaitForSeconds(searchTimePerCrate);
            
            // 30% chance to find clue
            if (Random.Range(0f, 1f) > 0.7f)
            {
                FindClue();
            }
        }

        private void FindClue()
        {
            dockworkerTrust += trustIncreasePerHelp;
            Debug.Log("Found trafficking clue! Dockworkers trust you more now.");
        }

        public void SpotTrafficker()
        {
            if (!vengeanceActive)
            {
                vengeanceActive = true;
                StartCoroutine(VengeanceCoroutine());
            }
        }

        private IEnumerator VengeanceCoroutine()
        {
            var gambler = FindObjectOfType<GamblerCharacter>();
            
            // +50% damage but -20% defense
            gambler.AddStatBoost("damage", 50, true, vengeanceDuration, "Vengeance");
            gambler.AddStatBoost("defense", -20, true, vengeanceDuration, "Vengeance");
            
            yield return new WaitForSeconds(vengeanceDuration);
            vengeanceActive = false;
        }
    }
}

