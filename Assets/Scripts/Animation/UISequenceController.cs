using UnityEngine;
using DG.Tweening;

namespace TJ.Animation
{
    public class UISequenceController : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private AnimationStep[] _sequence = new AnimationStep[0];

        public AnimationStep[] sequence => _sequence;
        public bool IsTransitioning { get; private set; }
        public event System.Action OnTransitionStarted;
        public event System.Action OnTransitionCompleted;
        
        private Sequence currentSequence;
        private bool isReverse;
        private bool isKilling;

        private void Awake() => SetToStart();

        private void OnDisable()
        {
            // Ensure proper cleanup when disabled
            KillCurrentSequence(true);
        }

        public Sequence PlaySequence(bool reverse = false)
        {
            if (_sequence == null) _sequence = new AnimationStep[0];

            // // Block new sequences during transition in both editor and builds
            // if (IsTransitioning || isKilling) return null;

            isReverse = reverse;
            KillCurrentSequence(false);
            StartTransition();
            
            currentSequence = DOTween.Sequence();
            
            if (reverse)
            {
                PlayReverse();
            }
            else
            {
                PlayForward();
            }
            
            // Add completion and kill callbacks
            if (currentSequence != null)
            {
                currentSequence.OnComplete(() => {
                    if (currentSequence != null)
                    {
                        CompleteTransition();
                        currentSequence = null;
                    }
                });

                currentSequence.OnKill(() => {
                    if (isKilling)
                    {
                        isKilling = false;
                        CompleteTransition();
                        currentSequence = null;
                    }
                });
            }

            return currentSequence;
        }

        private void PlayForward()
        {
            if (currentSequence == null) return;

            foreach (var step in _sequence)
            {
                if (step?.animatable == null) continue;
                
                // Safety check in case sequence was killed
                if (currentSequence == null) return;
                
                currentSequence.Insert(step.forwardDelay, step.animatable.Animate(false));
            }
        }

        private void PlayReverse()
        {
            if (currentSequence == null) return;

            for (int i = _sequence.Length - 1; i >= 0; i--)
            {
                if (_sequence[i]?.animatable == null) continue;
                
                // Safety check in case sequence was killed
                if (currentSequence == null) return;
                
                currentSequence.Insert(_sequence[i].reverseDelay, _sequence[i].animatable.Animate(true));
            }
        }

        public void SetToStart()
        {
            KillCurrentSequence(true);
            SetAnimationState(animatable => animatable.SetToStart());
        }

        public void SetToEnd()
        {
            KillCurrentSequence(true);
            SetAnimationState(animatable => animatable.SetToEnd());
        }

        private void SetAnimationState(System.Action<Animatable> stateAction)
        {
            if (_sequence == null) return;

            foreach (var step in _sequence)
            {
                if (step?.animatable != null)
                {
                    stateAction(step.animatable);
                }
            }
        }

        private void StartTransition()
        {
            IsTransitioning = true;
            OnTransitionStarted?.Invoke();
        }

        private void CompleteTransition()
        {
            if (IsTransitioning)
            {
                IsTransitioning = false;
                OnTransitionCompleted?.Invoke();
            }
        }

        private void KillCurrentSequence(bool immediate)
        {
            if (currentSequence != null && currentSequence.IsActive())
            {
                isKilling = true;
                var sequence = currentSequence;
                currentSequence = null;

                if (immediate)
                {
                    sequence.Kill(false);
                    isKilling = false;
                    CompleteTransition();
                }
                else
                {
                    sequence.Kill(false);
                }
            }
            else
            {
                CompleteTransition();
            }
        }

        private void OnDestroy() 
        {
            KillCurrentSequence(true);
        }
    }

    [System.Serializable]
    public class AnimationStep
    {
        public Animatable animatable;
        [Header("Timing")]
        [Tooltip("Delay before this animation when playing forward")]
        public float forwardDelay = 0.2f;
        [Tooltip("Delay before this animation when playing in reverse")]
        public float reverseDelay = 0.2f;
    }
}