using UnityEngine;

namespace PDXUnderground.Core.Interfaces
{
    public interface IGameManager
    {
        void InitializeGame();
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void EndGame();
        bool IsGamePaused { get; }
    }
}

