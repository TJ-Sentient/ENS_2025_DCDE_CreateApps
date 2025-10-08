using UnityEngine;
using DG.Tweening;

namespace TJ.Animation
{
    public class UIAnimatable : Animatable
    {
        [Header("Components")]
        [SerializeField] private RectTransform rect;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Scale")]
        public bool useScale = false;
        public Vector3 startScale    = new Vector3(0, 0, 0);
        public Vector3 targetScale   = Vector3.one;
        public float   scaleDuration = 0.5f;
        public Ease    scaleEase     = Ease.OutBack;

        [Header("Fade")]
        public bool useFade = false;
        public float startAlpha   = 0f;
        public float targetAlpha  = 1f;
        public float fadeDuration = 0.5f;
        public Ease  fadeEase     = Ease.OutQuad;

        [Header("RectTransform")]
        public bool useRect = false;

        [System.Serializable]
        public class RectSettings
        {
            [Header("Anchored Position")]
            public Vector2 startAnchoredPosition;
            public Vector2 targetAnchoredPosition;

            [Header("Size Delta")]
            public Vector2 startSizeDelta;
            public Vector2 targetSizeDelta;

            [Header("Pivot")]
            public Vector2 startPivot;
            public Vector2 targetPivot;

            [Header("Anchors")]
            public Vector2 startAnchorMin;
            public Vector2 targetAnchorMin;
            public Vector2 startAnchorMax;
            public Vector2 targetAnchorMax;
        }

        public RectSettings rectSettings;
        public float        rectDuration = 0.5f;
        public Ease         rectEase     = Ease.OutQuad;

        private void Reset()
        {
            rect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (rect != null)
            {
                rectSettings = new RectSettings
                {
                    startAnchoredPosition = rect.anchoredPosition,
                    targetAnchoredPosition = rect.anchoredPosition,
                    startSizeDelta = rect.sizeDelta,
                    targetSizeDelta = rect.sizeDelta,
                    startPivot = rect.pivot,
                    targetPivot = rect.pivot,
                    startAnchorMin = rect.anchorMin,
                    targetAnchorMin = rect.anchorMin,
                    startAnchorMax = rect.anchorMax,
                    targetAnchorMax = rect.anchorMax
                };
            }
        }

        [ContextMenu("Copy Current to Start")]
        public void CopyCurrentToStart()
        {
            if (rect != null)
            {
                rectSettings.startAnchoredPosition = rect.anchoredPosition;
                rectSettings.startSizeDelta = rect.sizeDelta;
                rectSettings.startPivot = rect.pivot;
                rectSettings.startAnchorMin = rect.anchorMin;
                rectSettings.startAnchorMax = rect.anchorMax;
            }
        }

        [ContextMenu("Copy Current to Target")]
        public void CopyCurrentToTarget()
        {
            if (rect != null)
            {
                rectSettings.targetAnchoredPosition = rect.anchoredPosition;
                rectSettings.targetSizeDelta = rect.sizeDelta;
                rectSettings.targetPivot = rect.pivot;
                rectSettings.targetAnchorMin = rect.anchorMin;
                rectSettings.targetAnchorMax = rect.anchorMax;
            }
        }

        public override void SetToStart()
        {
            KillSequence();
            if (useScale) rect.localScale = startScale;
            if (useFade && canvasGroup != null) canvasGroup.alpha = startAlpha;
            if (useRect)
            {
                rect.anchoredPosition = rectSettings.startAnchoredPosition;
                rect.sizeDelta = rectSettings.startSizeDelta;
                rect.pivot = rectSettings.startPivot;
                rect.anchorMin = rectSettings.startAnchorMin;
                rect.anchorMax = rectSettings.startAnchorMax;
            }
        }

        public override void SetToEnd()
        {
            KillSequence();
            if (useScale) rect.localScale = targetScale;
            if (useFade && canvasGroup != null) canvasGroup.alpha = targetAlpha;
            if (useRect)
            {
                rect.anchoredPosition = rectSettings.targetAnchoredPosition;
                rect.sizeDelta = rectSettings.targetSizeDelta;
                rect.pivot = rectSettings.targetPivot;
                rect.anchorMin = rectSettings.targetAnchorMin;
                rect.anchorMax = rectSettings.targetAnchorMax;
            }
        }

        protected override void SetupSequence(Sequence sequence, bool reverse)
        {
            if (useScale)
            {
                var targetScaleValue = reverse ? startScale : targetScale;
                currentSequence.Join(rect.DOScale(targetScaleValue, scaleDuration)
                    .SetEase(GetFixedEase(scaleEase, reverse)));
            }
        
            if (useFade && canvasGroup != null)
            {
                var targetAlphaValue = reverse ? startAlpha : targetAlpha;
                currentSequence.Join(canvasGroup.DOFade(targetAlphaValue, fadeDuration)
                    .SetEase(GetFixedEase(fadeEase, reverse)));
            }
        
            if (useRect)
            {
                var ease = GetFixedEase(rectEase, reverse);
                var targetPos = reverse ? rectSettings.startAnchoredPosition : rectSettings.targetAnchoredPosition;
                var targetSize = reverse ? rectSettings.startSizeDelta : rectSettings.targetSizeDelta;
                var targetPivot = reverse ? rectSettings.startPivot : rectSettings.targetPivot;
                var targetAnchorMin = reverse ? rectSettings.startAnchorMin : rectSettings.targetAnchorMin;
                var targetAnchorMax = reverse ? rectSettings.startAnchorMax : rectSettings.targetAnchorMax;
        
                currentSequence.Join(rect.DOAnchorPos(targetPos, rectDuration).SetEase(ease));
                currentSequence.Join(rect.DOSizeDelta(targetSize, rectDuration).SetEase(ease));
                currentSequence.Join(rect.DOPivot(targetPivot, rectDuration).SetEase(ease));
                currentSequence.Join(rect.DOAnchorMin(targetAnchorMin, rectDuration).SetEase(ease));
                currentSequence.Join(rect.DOAnchorMax(targetAnchorMax, rectDuration).SetEase(ease));
            }
        }
    }
}