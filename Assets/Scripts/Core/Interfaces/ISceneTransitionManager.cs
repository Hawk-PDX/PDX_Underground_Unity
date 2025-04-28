using UnityEngine;
using UnityEngine.Events;

namespace PDXUnderground.Core.Interfaces
{
    public interface ISceneTransitionManager
    {
        void LoadNextScene();
        void LoadScene(string sceneName);
        void TransitionToTunnels(Vector3 playerPosition);
        void TransitionToPort(Vector3 playerPosition);
        void TransitionToStreets(Vector3 playerPosition);
        Vector3 GetSpawnPointForScene(string sceneName, Vector3 nearPosition);
        void UpdateAreaName(string areaName);
        bool IsTransitioning { get; }
    }
}

