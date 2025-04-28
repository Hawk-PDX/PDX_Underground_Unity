using System.Collections.Generic;
using PDXUnderground.Core;

namespace PDXUnderground.Core.Interfaces
{
    /// <summary>
    /// Interface for managing a deck of cards
    /// </summary>
    public interface IDeckManager
    {
        /// <summary>
        /// Draw a card from the deck
        /// </summary>
        /// <returns>A card drawn from the deck</returns>
        ICard DrawCard();
        
        /// <summary>
        /// Add a card to the deck
        /// </summary>
        /// <param name="card">Card to add</param>
        void AddCardToDeck(ICard card);
        
        /// <summary>
        /// Get all cards in the current deck
        /// </summary>
        /// <returns>List of all cards in the deck</returns>
        List<ICard> GetDeck();
        
        /// <summary>
        /// Get the number of cards remaining in the deck
        /// </summary>
        /// <returns>Number of cards in the deck</returns>
        int GetDeckCount();
    }
}
