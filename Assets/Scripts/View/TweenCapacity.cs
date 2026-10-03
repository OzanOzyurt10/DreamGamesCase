using DG.Tweening;
using UnityEngine;

namespace DreamCase.View
{
    /// <summary>
    /// Reserves DOTween's pools before the first scene loads. A busy turn on a 10x10 board animates every falling
    /// item with its own sequence, which is more than DOTween's defaults (200 tweens, 50 sequences); reserving up
    /// front avoids growing the pools mid-game.
    /// </summary>
    internal static class TweenCapacity
    {
        private const int Tweeners = 1000;
        private const int Sequences = 250;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Reserve() => DOTween.SetTweensCapacity(Tweeners, Sequences);
    }
}
