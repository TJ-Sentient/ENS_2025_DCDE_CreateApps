#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DG.DOTweenEditor;
using System.Threading.Tasks;

namespace TJ.Animation
{
    [CustomEditor(typeof(UIAnimatable))]
    public class UIAnimatableEditor : Editor
    {
        private bool     isPreviewPlaying = false;
        private double   startTime;
        private bool     showStartSettings  = true;
        private bool     showTargetSettings = true;
        private GUIStyle headerStyle;
        private Color    startColor  = new Color(0.8f, 0.9f, 1f);
        private Color    targetColor = new Color(0.8f, 1f, 0.9f);

        public override void OnInspectorGUI()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.foldout);
                headerStyle.fontStyle = FontStyle.Bold;
            }

            UIAnimatable animatable = (UIAnimatable)target;
            serializedObject.Update();

            // Components
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rect"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("canvasGroup"));
            }

            // Animation Types
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Animation Types", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawAnimationTypeToggle("Scale", "useScale");
                DrawAnimationTypeToggle("Fade", "useFade");
                DrawAnimationTypeToggle("RectTransform", "useRect");
            }

            // Scale Settings
            if (animatable.useScale)
                DrawScaleSettings();

            // Fade Settings
            if (animatable.useFade)
                DrawFadeSettings();

            // RectTransform Settings
            if (animatable.useRect)
                DrawRectTransformSettings();

            // Preview Controls
            EditorGUILayout.Space(10);
            DrawPreviewControls(animatable);

            serializedObject.ApplyModifiedProperties();

            // Keep repainting while preview is active
            if (isPreviewPlaying)
                Repaint();
        }

        private void DrawAnimationTypeToggle(string label, string propertyName)
        {
            var prop = serializedObject.FindProperty(propertyName);
            prop.boolValue = EditorGUILayout.ToggleLeft(label, prop.boolValue);
        }

        private void DrawScaleSettings()
        {
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Scale Settings", EditorStyles.boldLabel);

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUI.backgroundColor = startColor;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startScale"),
                        new GUIContent("Start Scale"));
                    GUI.backgroundColor = Color.white;
                }

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUI.backgroundColor = targetColor;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("targetScale"),
                        new GUIContent("Target Scale"));
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("scaleDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("scaleEase"));
            }
        }

        private void DrawFadeSettings()
        {
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Fade Settings", EditorStyles.boldLabel);

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUI.backgroundColor = startColor;
                    EditorGUILayout.Slider(serializedObject.FindProperty("startAlpha"), 0f, 1f,
                        new GUIContent("Start Alpha"));
                    GUI.backgroundColor = Color.white;
                }

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUI.backgroundColor = targetColor;
                    EditorGUILayout.Slider(serializedObject.FindProperty("targetAlpha"), 0f, 1f,
                        new GUIContent("Target Alpha"));
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeEase"));
            }
        }

        private void DrawRectTransformSettings()
        {
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("RectTransform Settings", EditorStyles.boldLabel);
                SerializedProperty rectSettings = serializedObject.FindProperty("rectSettings");

                // Start Settings
                GUI.backgroundColor = startColor;
                showStartSettings = EditorGUILayout.Foldout(showStartSettings, "Start Values", true, headerStyle);
                if (showStartSettings)
                {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("startAnchoredPosition"));
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("startSizeDelta"),
                            new GUIContent("Start Size (Width, Height)"));
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("startAnchorMin"));
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("startAnchorMax"));
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("startPivot"));

                        if (GUILayout.Button("Copy Current Values to Start"))
                        {
                            ((UIAnimatable)target).CopyCurrentToStart();
                        }
                    }
                }

                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space(5);

                // Target Settings
                GUI.backgroundColor = targetColor;
                showTargetSettings = EditorGUILayout.Foldout(showTargetSettings, "Target Values", true, headerStyle);
                if (showTargetSettings)
                {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("targetAnchoredPosition"));
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("targetSizeDelta"),
                            new GUIContent("Target Size (Width, Height)"));
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("targetAnchorMin"));
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("targetAnchorMax"));
                        EditorGUILayout.PropertyField(rectSettings.FindPropertyRelative("targetPivot"));

                        if (GUILayout.Button("Copy Current Values to Target"))
                        {
                            ((UIAnimatable)target).CopyCurrentToTarget();
                        }
                    }
                }

                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rectDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rectEase"));
            }
        }

        private void DrawPreviewControls(UIAnimatable animatable)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Preview Controls", EditorStyles.boldLabel);

                if (!isPreviewPlaying)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Start → End"))
                        {
                            animatable.SetToStart();
                            PlayPreview(animatable, false);
                        }

                        if (GUILayout.Button("End → Start"))
                        {
                            animatable.SetToEnd();
                            PlayPreview(animatable, true);
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Preview"))
                    {
                        StopPreview(false);
                    }

                    float totalDuration = GetLongestDuration(animatable);
                    float currentTime = (float)(EditorApplication.timeSinceStartup - startTime);
                    float progress = Mathf.Clamp01(currentTime / totalDuration);

                    EditorGUI.ProgressBar(
                        EditorGUILayout.GetControlRect(false, 20f),
                        progress,
                        $"Preview Progress: {currentTime:F1}s / {totalDuration:F1}s"
                    );
                }
            }
        }

        private async void PlayPreview(UIAnimatable animatable, bool reverse)
        {
            StopPreview(false);
            isPreviewPlaying = true;
            startTime = EditorApplication.timeSinceStartup;

            var sequence = animatable.Animate(reverse);
            DOTweenEditorPreview.PrepareTweenForPreview(sequence);
            DOTweenEditorPreview.Start();

            float duration = GetLongestDuration(animatable);
            await Task.Delay((int)(duration * 1000));

            StopPreview(false);
        }

        private void StopPreview(bool reset)
        {
            if (!isPreviewPlaying) return;

            DOTweenEditorPreview.Stop(reset);
            isPreviewPlaying = false;
        }

        private float GetLongestDuration(UIAnimatable animatable)
        {
            float maxDuration = 0f;
            if (animatable.useScale) maxDuration = Mathf.Max(maxDuration, animatable.scaleDuration);
            if (animatable.useFade) maxDuration = Mathf.Max(maxDuration, animatable.fadeDuration);
            if (animatable.useRect) maxDuration = Mathf.Max(maxDuration, animatable.rectDuration);
            return maxDuration;
        }

        private void OnDisable()
        {
            StopPreview(false);
        }
    }
}
#endif