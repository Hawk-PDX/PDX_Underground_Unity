using UnityEngine;

namespace PDXUnderground.Core.Interfaces
{
    /// <summary>
    /// Interface for UI management
    /// </summary>
    public interface IUIManager
    {
        void ShowTransitionScreen(string areaName, float duration);
        void HideTransitionScreen();
        void UpdateUIState(GameState state);
        void ShowLoadingScreen(float progress);
        void HideLoadingScreen();
        
        /// <summary>
        /// Shows the current area name in the UI
        /// </summary>
        /// <param name="areaName">Name of the area to display</param>
        void ShowAreaName(string areaName);
    }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Loading,
        GameOver
    }
}

