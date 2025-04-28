using UnityEngine;
using PDXUnderground.Core;

namespace PDXUnderground.Core.Interfaces
{
    public interface ICardEffectsController
    {
        void PlayCardEffect(ICard card, Vector3 position);
        GameObject PlayCardThrowEffect(Vector3 startPosition, Vector3 direction, float cardSpeed = 20f, int cardType = 0, bool isCritical = false);
        void PlaySliceEffect(Vector3 position, Quaternion rotation, bool isCritical = false);
    }
}

