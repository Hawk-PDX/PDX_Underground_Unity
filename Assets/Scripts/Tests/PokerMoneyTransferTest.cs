using NUnit.Framework;
using UnityEngine;
using PDXUnderground.Player;
using PDXUnderground.Minigames;

namespace PDXUnderground.Tests
{
    public class PokerMoneyTransferTest
    {
        private GamblerCharacter gambler;
        private PokerPlayer pokerPlayer;
        
        [SetUp]
        public void Setup()
        {
            // Create test objects
            GameObject go = new GameObject();
            gambler = go.AddComponent&lt;GamblerCharacter&gt;();
            pokerPlayer = go.AddComponent&lt;PokerPlayer&gt;();
            
            // Initialize with test values
            gambler.startingMoney = 500;
            gambler.Start(); // Calls Awake() and Start()
        }

        [Test]
        public void TestValidBuyIn()
        {
            // Initial state check
            Assert.AreEqual(500, gambler.CurrentMoney);
            Assert.AreEqual(0, pokerPlayer.Chips);
            
            // Test valid buy-in
            bool result = pokerPlayer.BuyIn(250);
            Assert.IsTrue(result);
            Assert.AreEqual(250, gambler.CurrentMoney);
            Assert.AreEqual(250, pokerPlayer.Chips);
        }

        [Test]
        public void TestInvalidBuyIn_InsufficientFunds()
        {
            // Test invalid buy-in (too much)
            bool result = pokerPlayer.BuyIn(600);
            Assert.IsFalse(result);
            Assert.AreEqual(500, gambler.CurrentMoney); // Unchanged
            Assert.AreEqual(0, pokerPlayer.Chips); // Unchanged
        }

        [Test]
        public void TestCashOutWithWinnings()
        {
            // Setup initial buy-in
            pokerPlayer.BuyIn(250);
            
            // Simulate winning 150 chips
            pokerPlayer.Chips += 150; // Now 400 chips
            
            // Test cash out
            bool result = pokerPlayer.CashOut();
            Assert.IsTrue(result);
            Assert.AreEqual(650, gambler.CurrentMoney); // Original 500 - 250 + 400
            Assert.AreEqual(0, pokerPlayer.Chips);
        }

        [Test]
        public void TestInvalidNegativeTransfer()
        {
            // Test negative amount
            bool result = pokerPlayer.BuyIn(-100);
            Assert.IsFalse(result);
            Assert.AreEqual(500, gambler.CurrentMoney);
            Assert.AreEqual(0, pokerPlayer.Chips);
        }

        [Test]
        public void TestMoneyChangeEvents()
        {
            int eventCount = 0;
            int lastAmount = 0;
            
            gambler.OnMoneyChanged += (amount) => {
                eventCount++;
                lastAmount = amount;
            };
            
            // Trigger valid transfer
            pokerPlayer.BuyIn(300);
            
            // Verify events
            Assert.AreEqual(1, eventCount);
            Assert.AreEqual(200, lastAmount); // 500 - 300 = 200
        }
    }
}

