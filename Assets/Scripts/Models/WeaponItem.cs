using System;
using PDXUnderground.Models;

namespace PDXUnderground.Models
{
    [Serializable]
    public class WeaponItem : Item
    {
        // Weapon-specific properties
        public float BaseDamage { get; set; }
        public float AttackSpeed { get; set; }
        public float Range { get; set; }
        public string DamageType { get; set; }  // physical, fire, ice, etc.
        public string AttackType { get; set; }  // slash, pierce, blunt, etc.
        
        // Special properties
        public float CriticalChance { get; set; }
        public float CriticalMultiplier { get; set; }
        public bool IsTwoHanded { get; set; }
        
        // Override base Use method
        public override bool Use(CharacterData character, float currentTime)
        {
            if (!base.Use(character, currentTime))
                return false;

            // Weapon-specific use logic here
            // In a pure model, this might just track durability and return success
            if (HasDurability)
            {
                CurrentDurability--;
            }

            return true;
        }

        // Override equipment methods
        public override void OnEquip(CharacterData character)
        {
            if (character?.Stats == null) return;

            // Apply weapon stats to character
            character.Stats.PhysicalDamage += BaseDamage;
            // Other stat modifications as needed
        }

        public override void OnUnequip(CharacterData character)
        {
            if (character?.Stats == null) return;

            // Remove weapon stats from character
            character.Stats.PhysicalDamage -= BaseDamage;
            // Remove other stat modifications
        }

        // Override copy method to include weapon-specific properties
        public override Item CreateCopy()
        {
            var copy = (WeaponItem)base.CreateCopy();
            // Weapon-specific properties are copied by MemberwiseClone
            return copy;
        }
    }
}
