using UnityEngine;

namespace PDXUnderground.Core.Interfaces
{
    public interface IGaslightFlicker
    {
        void SetTimeOfDay(float timeValue);  // 0.0f = midnight, 0.5f = noon
        void SetWeatherEffect(float intensity);  // Weather effect intensity
    }
}

