
using UnityEngine;

namespace PDXUnderground.UI
{
    [CreateAssetMenu(menuName = "PDX Underground/Card Designs")]
    public class PeriodCardDesigns : ScriptableObject
    {
        [Header("Art Deco Designs")]
        public Sprite[] artDecoCardBacks;
        
        [Header("Character Suits")]
        public Sprite gangsterSuit;
        public Sprite flapperSuit;
        public Sprite bootleggerSuit;
        public Sprite policemanSuit;
        
        [Header("Poker Chips")] 
        public GameObject vintageChip1; // $0.25
        public GameObject vintageChip5; // $1
        public GameObject vintageChip20; // $5
        
        public Sprite GetRandomCardBack()
        {
            if (artDecoCardBacks.Length == 0) return null;
            return artDecoCardBacks[Random.Range(0, artDecoCardBacks.Length)];
        }
        
        public Sprite GetSuitForCharacterType(string type)
        {
            switch (type.ToLower())
            {
                case "gangster": return gangsterSuit;
                case "flapper": return flapperSuit;
                case "bootlegger": return bootleggerSuit;
                case "policeman": return policemanSuit;
                default: return null;
            }
        }
    }
}

