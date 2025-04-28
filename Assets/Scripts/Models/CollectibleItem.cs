using UnityEngine;

namespace PDXUnderground.Core.Models
{
    /// <summary>
    /// Represents a collectible item required for tunnel access or other game mechanics
    /// </summary>
    [System.Serializable]
    public class CollectibleItem
    {
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;
    }
}

