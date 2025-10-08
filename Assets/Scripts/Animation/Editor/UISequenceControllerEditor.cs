#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using DG.DOTweenEditor;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.EditorCoroutines.Editor;

namespace TJ.Animation
{
    [CustomEditor(typeof(UISequenceController))]
    public class UISequenceControllerEditor : Editor
    {
        private UISequenceController sequenceController;
        private SerializedProperty sequenceProperty;
        private ReorderableList reorderableList;
        
        private bool isPreviewPlaying = false;
        private double startTime;
        private float totalDuration;
        private EditorCoroutine previewCoroutine;

        private void OnEnable()
        {
            sequenceController = (UISequenceController)target;
            sequenceProperty = serializedObject.FindProperty("_sequence");
            SetupReorderableList();
        }

        private void SetupReorderableList()
        {
            if (sequenceProperty == null) return;
            
            reorderableList = new ReorderableList(
                serializedObject,
                sequenceProperty,
                true,  // draggable
                true,  // displayHeader
                true,  // add
                true   // remove
            );

            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                float animatableWidth = rect.width * 0.5f;
                float delayWidth = rect.width * 0.25f;
                
                EditorGUI.LabelField(new Rect(rect.x, rect.y, animatableWidth, rect.height), "Animatable");
                EditorGUI.LabelField(new Rect(rect.x + animatableWidth, rect.y, delayWidth, rect.height), "Forward Delay");
                EditorGUI.LabelField(new Rect(rect.x + animatableWidth + delayWidth, rect.y, delayWidth, rect.height), "Reverse Delay");
            };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index < 0 || index >= sequenceProperty.arraySize) return;
                
                var element = sequenceProperty.GetArrayElementAtIndex(index);
                if (element == null) return;

                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                // Calculate rects for the three fields
                float animatableWidth = rect.width * 0.5f;
                float delayWidth = rect.width * 0.25f - 5f;
                
                var animatableRect = new Rect(rect.x, rect.y, animatableWidth, rect.height);
                var forwardDelayRect = new Rect(rect.x + animatableWidth + 5f, rect.y, delayWidth, rect.height);
                var reverseDelayRect = new Rect(rect.x + animatableWidth + delayWidth + 10f, rect.y, delayWidth, rect.height);

                var animatableProp = element.FindPropertyRelative("animatable");
                var forwardDelayProp = element.FindPropertyRelative("forwardDelay");
                var reverseDelayProp = element.FindPropertyRelative("reverseDelay");

                // Draw the fields
                EditorGUI.PropertyField(animatableRect, animatableProp, GUIContent.none);
                
                float forwardDelay = forwardDelayProp.floatValue;
                float reverseDelay = reverseDelayProp.floatValue;
                
                forwardDelay = EditorGUI.FloatField(forwardDelayRect, forwardDelay);
                reverseDelay = EditorGUI.FloatField(reverseDelayRect, reverseDelay);
                
                if (!Mathf.Approximately(forwardDelay, forwardDelayProp.floatValue))
                    forwardDelayProp.floatValue = forwardDelay;
                if (!Mathf.Approximately(reverseDelay, reverseDelayProp.floatValue))
                    reverseDelayProp.floatValue = reverseDelay;
            };

            reorderableList.elementHeightCallback = (int index) => EditorGUIUtility.singleLineHeight + 4;
            
            reorderableList.onAddCallback = (ReorderableList list) =>
            {
                list.serializedProperty.arraySize++;
                var element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                element.FindPropertyRelative("forwardDelay").floatValue = 0.2f;
                element.FindPropertyRelative("reverseDelay").floatValue = 0.2f;
            };
        }

        public override void OnInspectorGUI()
        {
            if (sequenceProperty == null || reorderableList == null)
            {
                EditorGUILayout.HelpBox("Failed to initialize sequence property. Please check the component.", MessageType.Error);
                return;
            }

            serializedObject.Update();

            EditorGUILayout.Space(10);
            
            if (!Application.isPlaying)
            {
                DrawPreviewControls();
            }
            else
            {
                DrawRuntimeControls();
            }
            
            EditorGUILayout.Space(10);
            DrawSequenceList();

            serializedObject.ApplyModifiedProperties();

            if (isPreviewPlaying)
                Repaint();
        }

        private void DrawPreviewControls()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Preview Controls", EditorStyles.boldLabel);

                if (!isPreviewPlaying)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Set To Start", EditorStyles.miniButtonLeft))
                        {
                            SafeSetToStart();
                        }
                        
                        if (GUILayout.Button("Set To End", EditorStyles.miniButtonRight))
                        {
                            SafeSetToEnd();
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Preview Forward"))
                        {
                            SafeSetToStart();
                            PlayPreview(false);
                        }

                        if (GUILayout.Button("Preview Reverse"))
                        {
                            SafeSetToEnd();
                            PlayPreview(true);
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Preview"))
                    {
                        StopPreview();
                    }

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

        private void DrawRuntimeControls()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

                if (sequenceController.IsTransitioning)
                {
                    EditorGUI.ProgressBar(
                        EditorGUILayout.GetControlRect(false, 20f),
                        1f,
                        "Animation In Progress..."
                    );
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Set To Start", EditorStyles.miniButtonLeft))
                        {
                            sequenceController.SetToStart();
                        }
                        
                        if (GUILayout.Button("Set To End", EditorStyles.miniButtonRight))
                        {
                            sequenceController.SetToEnd();
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Play Forward"))
                        {
                            sequenceController.PlaySequence(false);
                        }

                        if (GUILayout.Button("Play Reverse"))
                        {
                            sequenceController.PlaySequence(true);
                        }
                    }
                }
            }
        }

        private void DrawSequenceList()
        {
            EditorGUILayout.LabelField("Sequence Steps", EditorStyles.boldLabel);
            reorderableList.DoLayoutList();
        }

        private async void PlayPreview(bool reverse)
        {
            StopPreview();
            isPreviewPlaying = true;
            startTime = EditorApplication.timeSinceStartup;

            var sequence = sequenceController.PlaySequence(reverse);
            if (sequence == null) return;
            
            totalDuration = CalculateSequenceDuration(sequence);

            DOTweenEditorPreview.PrepareTweenForPreview(sequence);
            DOTweenEditorPreview.Start();

            try 
            {
                await Task.Delay((int)(totalDuration * 1000));
            }
            catch (System.OperationCanceledException)
            {
                // Preview was stopped, no need to handle
            }
            finally 
            {
                StopPreview();
            }
        }

        private float CalculateSequenceDuration(Sequence sequence)
        {
            if (sequence == null) return 0f;
            return sequence.Duration();
        }

        private void SafeSetToStart()
        {
            StopPreview();
            sequenceController.SetToStart();
        }

        private void SafeSetToEnd()
        {
            StopPreview();
            sequenceController.SetToEnd();
        }

        private void StopPreview()
        {
            if (!isPreviewPlaying) return;

            DOTweenEditorPreview.Stop();
            isPreviewPlaying = false;
            EditorUtility.SetDirty(target);
        }

        private void OnDisable()
        {
            StopPreview();
        }
    }
}
#endif