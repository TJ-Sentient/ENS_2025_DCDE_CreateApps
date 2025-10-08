using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace TJ.Animation
{
    public abstract class Animatable : MonoBehaviour
    {
        protected Sequence currentSequence;
        
        /// <summary>
        /// Sets a new sequence for the instance, killing any existing sequence.
        /// </summary>
        /// <param name="newSequence">The new animation sequence to set.</param>
        protected void SetSequence(Sequence newSequence)
        {
            currentSequence = newSequence;
        }
        
        /// <summary>
        /// Sets the element to its starting state
        /// </summary>
        [Button]
        public abstract void SetToStart();

        /// <summary>
        /// Sets the element to its end state
        /// </summary>
        [Button]
        public abstract void SetToEnd();

        /// <summary>
        /// Executes the animation sequence
        /// </summary>
        /// <param name="reverse">If true, animates from end to start state</param>
        /// <returns>The animation sequence</returns>
        [Button]
        public Sequence Animate(bool reverse = false)
        {
            // Ensure any existing sequence is killed before creating a new one
            KillSequence();
            currentSequence = DOTween.Sequence();

            // Add animations to the sequence here as needed by derived classes
            SetupSequence(currentSequence, reverse);

            return currentSequence;
        }
        
         protected abstract void SetupSequence(Sequence sequence, bool reverse);

        /// <summary>
        /// Stops any current animation
        /// </summary>
        public virtual void KillSequence()
        {
            if (currentSequence != null && currentSequence.IsActive())
            {
                currentSequence.Kill();
                currentSequence = null;
            }
        }

        protected virtual void OnDestroy()
        {
            KillSequence();
        }

        protected Ease GetFixedEase(Ease originalEase, bool reverse)
        {
            if (!reverse) return originalEase;

            return originalEase switch
            {
                Ease.OutBack => Ease.InQuad,
                Ease.OutBounce => Ease.InQuad,
                Ease.OutElastic => Ease.InQuad,
                Ease.OutCirc => Ease.InCirc,
                Ease.OutCubic => Ease.InCubic,
                Ease.OutQuad => Ease.InQuad,
                Ease.OutQuart => Ease.InQuart,
                Ease.OutQuint => Ease.InQuint,
                Ease.OutSine => Ease.InSine,
                Ease.OutExpo => Ease.InExpo,
                _ => originalEase
            };
        }
    }
}