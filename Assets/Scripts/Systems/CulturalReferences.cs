
using System.Collections.Generic;
using UnityEngine;

namespace PDXUnderground.Systems
{
    public class CulturalReferences : MonoBehaviour
    {
        [Header("1920s Slang Dictionary")]
        public List<string> greetings = new List<string> {
            "What's the rumpus?", 
            "How's the bees knees?",
            "Heard any good dish lately?"
        };
        
        public List<string> winPhrases = new List<string> {
            "That's the cat's pajamas!",
            "Now you're on the trolley!",
            "That's banana oil!"
        };
        
        [Header("Jazz Triggers")]
        public AudioClip[] jazzTracks;
        public float jazzVolume = 0.7f;
        
        [Header("Historical Events")]
        public string[] historicalEvents = {
            "Al Capone moves to Chicago (1920)",
            "Volstead Act enacted (1920)",
            "Wall Street Bombing (1920)",
            "Tutankhamun's tomb discovered (1922)"
        };
        
        public string GetRandomGreeting()
        {
            return greetings[Random.Range(0, greetings.Count)];
        }
        
        public void PlayRandomJazz(AudioSource source)
        {
            if (jazzTracks.Length > 0)
            {
                source.PlayOneShot(
                    jazzTracks[Random.Range(0, jazzTracks.Length)],
                    jazzVolume
                );
            }
        }
    }
}

