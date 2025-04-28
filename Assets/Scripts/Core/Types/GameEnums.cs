namespace PDXUnderground.Core
{
    /// <summary>
    /// Represents the current state of the game
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Game is currently loading
        /// </summary>
        Loading,
        
        /// <summary>
        /// In the main menu
        /// </summary>
        MainMenu,
        
        /// <summary>
        /// Actively playing the game
        /// </summary>
        Playing,
        
        /// <summary>
        /// Game is paused
        /// </summary>
        Paused,
        
        /// <summary>
        /// In inventory/character screen
        /// </summary>
        Inventory,
        
        /// <summary>
        /// Game over state
        /// </summary>
        GameOver
    }
}

