using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PDXUnderground.Models;

namespace PDXUnderground.Tests
{
    /// <summary>
    /// Test suite for the Card model class
    /// </summary>
    public class CardTests
    {
        // Tolerance for floating point comparisons
        private const float DELTA = 0.001f;

        [SetUp]
        public void Setup()
        {
            // Reset Time.time for each test to ensure consistent cooldown testing
            // This works for editor tests but would need a different approach for play mode tests
            Time.timeScale = 1.0f;
        }

        #region Constructor Tests

        [Test]
        public void DefaultConstructor_InitializesWithDefaultValues()
        {
            // Arrange & Act
            Card card = new Card();

            // Assert
            Assert.AreEqual("Default Card", card.name);
            Assert.AreEqual(Card.CardType.Utility, card.type);
            Assert.AreEqual(1, card.energyCost);
            Assert.AreEqual(0, card.cooldown);
            Assert.AreEqual(0, card.damage);
            Assert.AreEqual(0, card.lastUseTime);
        }

        [Test]
        public void ParameterizedConstructor_InitializesWithProvidedValues()
        {
            // Arrange & Act
            string cardName = "Fireball";
            Card.CardType cardType = Card.CardType.Attack;
            float energyCost = 2.5f;
            float cooldown = 3.0f;
            float damage = 10.0f;

            Card card = new Card(cardName, cardType, energyCost, cooldown, damage);

            // Assert
            Assert.AreEqual(cardName, card.name);
            Assert.AreEqual(cardType, card.type);
            Assert.AreEqual(energyCost, card.energyCost, DELTA);
            Assert.AreEqual(cooldown, card.cooldown, DELTA);
            Assert.AreEqual(damage, card.damage, DELTA);
            Assert.AreEqual(0, card.lastUseTime);
        }

        [Test]
        public void Constructor_WithZeroOrNegativeValues_InitializesCorrectly()
        {
            // Arrange & Act
            Card card = new Card("Zero Card", Card.CardType.Utility, 0, -1, -5);

            // Assert
            Assert.AreEqual(0, card.energyCost);
            Assert.AreEqual(-1, card.cooldown); // While negative cooldown isn't logical, the class doesn't prevent it
            Assert.AreEqual(-5, card.damage); // Negative damage could represent healing
        }

        #endregion

        #region Cooldown System Tests

        [Test]
        public void IsOnCooldown_WhenJustUsed_ReturnsTrue()
        {
            // Arrange
            Card card = new Card("Test Card", Card.CardType.Attack, 1, 5, 10);
            card.lastUseTime = Time.time; // Just used

            // Act
            bool onCooldown = card.IsOnCooldown();

            // Assert
            Assert.IsTrue(onCooldown);
        }

        [Test]
        public void IsOnCooldown_WhenCooldownPassed_ReturnsFalse()
        {
            // Arrange
            Card card = new Card("Test Card", Card.CardType.Attack, 1, 5, 10);
            card.lastUseTime = Time.time - 10; // Used 10 seconds ago, cooldown is 5 seconds

            // Act
            bool onCooldown = card.IsOnCooldown();

            // Assert
            Assert.IsFalse(onCooldown);
        }

        [Test]
        public void IsOnCooldown_WithZeroCooldown_ReturnsFalse()
        {
            // Arrange
            Card card = new Card("Zero Cooldown", Card.CardType.Attack, 1, 0, 10);
            card.lastUseTime = Time.time; // Just used

            // Act
            bool onCooldown = card.IsOnCooldown();

            // Assert
            Assert.IsFalse(onCooldown); // Zero cooldown means it can be used immediately
        }

        [Test]
        public void GetRemainingCooldown_WhenJustUsed_ReturnsFullCooldown()
        {
            // Arrange
            float cooldown = 5.0f;
            Card card = new Card("Test Card", Card.CardType.Attack, 1, cooldown, 10);
            card.lastUseTime = Time.time; // Just used

            // Act
            float remaining = card.GetRemainingCooldown();

            // Assert
            Assert.AreEqual(cooldown, remaining, DELTA);
        }

        [Test]
        public void GetRemainingCooldown_WhenPartiallyElapsed_ReturnsRemainingTime()
        {
            // Arrange
            float cooldown = 5.0f;
            float elapsed = 2.0f;
            Card card = new Card("Test Card", Card.CardType.Attack, 1, cooldown, 10);
            card.lastUseTime = Time.time - elapsed;

            // Act
            float remaining = card.GetRemainingCooldown();

            // Assert
            Assert.AreEqual(cooldown - elapsed, remaining, DELTA);
        }

        [Test]
        public void GetRemainingCooldown_WhenFullyElapsed_ReturnsZero()
        {
            // Arrange
            float cooldown = 5.0f;
            Card card = new Card("Test Card", Card.CardType.Attack, 1, cooldown, 10);
            card.lastUseTime = Time.time - (cooldown + 1.0f); // Cooldown fully elapsed

            // Act
            float remaining = card.GetRemainingCooldown();

            // Assert
            Assert.AreEqual(0, remaining, DELTA);
        }

        #endregion

        #region Card Description Tests

        [Test]
        public void GetDescription_ForAttackCard_IncludesDamageAndEnergyCost()
        {
            // Arrange
            Card card = new Card("Attack Card", Card.CardType.Attack, 2, 5, 15);

            // Act
            string description = card.GetDescription();

            // Assert
            StringAssert.Contains("Deals 15 damage", description);
            StringAssert.Contains("Costs 2 energy", description);
        }

        [Test]
        public void GetDescription_ForDefenseCard_IncludesEnergyInfo()
        {
            // Arrange
            Card card = new Card("Defense Card", Card.CardType.Defense, 3, 5, 0);

            // Act
            string description = card.GetDescription();

            // Assert
            StringAssert.Contains("Provides defensive bonus", description);
            StringAssert.Contains("Costs 3 energy", description);
        }

        [Test]
        public void GetDescription_ForUtilityCard_ReturnsUtilityDescription()
        {
            // Arrange
            Card card = new Card("Utility Card", Card.CardType.Utility, 1, 2, 0);

            // Act
            string description = card.GetDescription();

            // Assert
            StringAssert.Contains("Utility effect", description);
            StringAssert.Contains("Costs 1 energy", description);
        }

        [Test]
        public void GetDescription_ForSpecialCard_ReturnsSpecialDescription()
        {
            // Arrange
            Card card = new Card("Special Card", Card.CardType.Special, 4, 10, 0);

            // Act
            string description = card.GetDescription();

            // Assert
            StringAssert.Contains("Special effect", description);
            StringAssert.Contains("Costs 4 energy", description);
        }

        #endregion

        #region Card Type Tests

        [Test]
        public void CardType_VerifyAllEnumValues()
        {
            // This test verifies that all expected enum values exist and haven't been modified
            
            // Verify each enum value
            Assert.AreEqual(0, (int)Card.CardType.Attack);
            Assert.AreEqual(1, (int)Card.CardType.Defense);
            Assert.AreEqual(2, (int)Card.CardType.Utility);
            Assert.AreEqual(3, (int)Card.CardType.Special);
            
            // Verify enum count (in case new values are added)
            Assert.AreEqual(4, System.Enum.GetValues(typeof(Card.CardType)).Length);
        }

        [Test]
        public void CardType_AttackCard_HasDamageValue()
        {
            // Arrange
            float expectedDamage = 25.0f;
            Card attackCard = new Card("Power Attack", Card.CardType.Attack, 3, 5, expectedDamage);
            
            // Act & Assert
            Assert.AreEqual(Card.CardType.Attack, attackCard.type);
            Assert.AreEqual(expectedDamage, attackCard.damage, DELTA);
        }

        [Test]
        public void CardType_DefenseCard_CanHaveZeroDamage()
        {
            // Arrange
            Card defenseCard = new Card("Shield", Card.CardType.Defense, 2, 3, 0);
            
            // Act & Assert
            Assert.AreEqual(Card.CardType.Defense, defenseCard.type);
            Assert.AreEqual(0, defenseCard.damage, DELTA);
        }
        
        [Test]
        public void CardType_SpecialCard_CanHaveCustomValues()
        {
            // Arrange - Creating a special card with unique values
            Card specialCard = new Card("Unique Special", Card.CardType.Special, 5, 10, 7.5f);
            
            // Act & Assert
            Assert.AreEqual(Card.CardType.Special, specialCard.type);
            Assert.AreEqual(5, specialCard.energyCost, DELTA);
            Assert.AreEqual(10, specialCard.cooldown, DELTA);
            Assert.AreEqual(7.5f, specialCard.damage, DELTA);
        }

        #endregion
    }
}

