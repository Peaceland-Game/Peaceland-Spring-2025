#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Peaceland.Notebook.EditableScenePack;

namespace Peaceland.Notebook.Editor
{
    public static class NotebookOverlayAuthoring
    {
        public static NotebookOverlayView EnsureOverlay(Transform canvasTransform)
        {
            Transform overlayRoot = canvasTransform.Find("Notebook Overlay");
            if (overlayRoot == null)
            {
                GameObject overlayObject = new GameObject("Notebook Overlay", typeof(RectTransform));
                overlayObject.transform.SetParent(canvasTransform, false);
                overlayRoot = overlayObject.transform;
            }

            Stretch(overlayRoot as RectTransform);
            overlayRoot.SetAsLastSibling();

            Transform collectibleHintRoot = EnsureChild(overlayRoot, "Collectible Hint");
            Image collectibleHintBackground = GetOrAddComponent<Image>(collectibleHintRoot.gameObject);
            collectibleHintBackground.color = new Color(0.17f, 0.13f, 0.09f, 0.9f);
            SetAnchored(collectibleHintRoot as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -108f), new Vector2(420f, 52f));
            TMP_Text collectibleHintText = EnsureText(collectibleHintRoot, "Label", "Collectable available", 20f, FontStyles.Bold, TextAlignmentOptions.Center);
            collectibleHintText.color = new Color(0.98f, 0.95f, 0.83f, 1f);
            Stretch(collectibleHintText.rectTransform, new Vector2(12f, 6f), new Vector2(-12f, -6f));

            Transform collectedToastRoot = EnsureChild(overlayRoot, "Collected Toast");
            bool createdToast = collectedToastRoot.GetComponent<Image>() == null;
            Image collectedToastBackground = GetOrAddComponent<Image>(collectedToastRoot.gameObject);
            collectedToastBackground.color = new Color(0.74f, 0.67f, 0.54f, 0.97f);
            NotebookNotificationAnchor notificationAnchor = GetOrAddComponent<NotebookNotificationAnchor>(collectedToastRoot.gameObject);
            if (createdToast)
            {
                notificationAnchor.ApplyPresetScreenSafeDefaults();
            }

            CanvasGroup toastCanvasGroup = GetOrAddComponent<CanvasGroup>(collectedToastRoot.gameObject);
            TMP_Text collectedToastText = EnsureText(collectedToastRoot, "Label", "Notebook updated", 22f, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(collectedToastText.rectTransform, new Vector2(18f, 10f), new Vector2(-18f, -10f));

            NotebookOverlayView overlayView = GetOrAddComponent<NotebookOverlayView>(overlayRoot.gameObject);
            overlayView.Configure(
                collectibleHintRoot.gameObject,
                collectibleHintText,
                collectedToastRoot as RectTransform,
                collectedToastText,
                toastCanvasGroup);
            overlayView.SyncNotificationFromScene();
            return overlayView;
        }

        [MenuItem("Peaceland/Notebook/Enable Hand-Placed Notification (Active Scene)")]
        public static void EnableHandPlacedNotificationMenu()
        {
            NotebookOverlayView overlayView = Object.FindFirstObjectByType<NotebookOverlayView>(FindObjectsInactive.Include);
            if (overlayView == null)
            {
                Debug.LogWarning("No NotebookOverlayView in active scene.");
                return;
            }

            SerializedObject serializedOverlay = new SerializedObject(overlayView);
            RectTransform toastRoot = serializedOverlay.FindProperty("collectedToastRoot")?.objectReferenceValue as RectTransform;
            if (toastRoot == null)
            {
                Debug.LogWarning("Notebook Overlay has no Collected Toast root.");
                return;
            }

            if (toastRoot.GetComponent<NotebookNotificationAnchor>() == null)
            {
                toastRoot.gameObject.AddComponent<NotebookNotificationAnchor>();
            }

            toastRoot.GetComponent<NotebookNotificationAnchor>().EnableHandPlacedRestPosition();
            overlayView.SyncNotificationFromScene();
            serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("Hand-placed notification enabled. Drag Collected Toast in Hierarchy, save scene.");
        }

        [MenuItem("Peaceland/Notebook/Move Collected Toast To Screen-Safe Top Left (Active Scene)")]
        public static void MoveCollectedToastToTopRightMenu()
        {
            NotebookOverlayView overlayView = Object.FindFirstObjectByType<NotebookOverlayView>(FindObjectsInactive.Include);
            if (overlayView == null)
            {
                Debug.LogWarning("No NotebookOverlayView in active scene.");
                return;
            }

            SerializedObject serializedOverlay = new SerializedObject(overlayView);
            SerializedProperty toastRootProperty = serializedOverlay.FindProperty("collectedToastRoot");
            RectTransform toastRoot = toastRootProperty?.objectReferenceValue as RectTransform;
            if (toastRoot != null)
            {
                NotebookNotificationAnchor anchor = toastRoot.GetComponent<NotebookNotificationAnchor>();
                if (anchor == null)
                {
                    anchor = toastRoot.gameObject.AddComponent<NotebookNotificationAnchor>();
                }

                anchor.ApplyPresetScreenSafeDefaults();
                overlayView.SyncNotificationFromScene();
            }

            serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("Collected toast moved to the screen-safe top-left in the active scene.");
        }

        [MenuItem("Peaceland/Notebook/Move Collected Toast To Screen-Safe Top Left (All Test Scenes)")]
        public static void MoveCollectedToastToTopRightAllScenesMenu()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Notebook/Scenes" });
            int updated = 0;
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                if (Object.FindFirstObjectByType<NotebookOverlayView>(FindObjectsInactive.Include) == null)
                {
                    continue;
                }

                MoveCollectedToastToTopRightMenu();
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                updated++;
            }

            Debug.Log($"Moved collected toast to the screen-safe top-left in {updated} notebook scene(s).");
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child;
            }

            GameObject childObject = new GameObject(childName, typeof(RectTransform));
            childObject.transform.SetParent(parent, false);
            return childObject.transform;
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static TMP_Text EnsureText(Transform parent, string name, string text, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
        {
            Transform textRoot = EnsureChild(parent, name);
            TMP_Text tmpText = GetOrAddComponent<TextMeshProUGUI>(textRoot.gameObject);
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.fontStyle = fontStyle;
            tmpText.alignment = alignment;
            tmpText.raycastTarget = false;
            return tmpText;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (ShouldPreserveAuthoredRect(rect))
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void SetAnchored(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            if (ShouldPreserveAuthoredRect(rect))
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static bool ShouldPreserveAuthoredRect(RectTransform rect)
        {
            if (rect == null)
            {
                return true;
            }

            return rect.GetComponent<NotebookHandPlacedRect>() != null
                || rect.GetComponent<NotebookNotificationAnchor>() != null;
        }
    }
}
#endif
