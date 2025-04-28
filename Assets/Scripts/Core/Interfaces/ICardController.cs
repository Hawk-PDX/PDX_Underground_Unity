using UnityEngine;
using System;
using PDXUnderground.Core;

namespace PDXUnderground.Core.Interfaces
{
    public interface ICardController
    {
        void DrawCard(ICard card);
        void UseCard(ICard card);
        void UseCard(ICard card, Vector3 direction);
        event Action<ICard> OnCardUsed;
        event Action<ICard> OnCardDrawn;
    }
}

