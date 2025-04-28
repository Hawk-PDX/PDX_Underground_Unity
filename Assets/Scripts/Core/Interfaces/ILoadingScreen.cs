using UnityEngine;

namespace PDXUnderground.Core.Interfaces
{
    public interface ILoadingScreen
    {
        void Show(bool visible);
        void UpdateProgress(float progress);
        void SetLoadingText(string text);
    }
}

