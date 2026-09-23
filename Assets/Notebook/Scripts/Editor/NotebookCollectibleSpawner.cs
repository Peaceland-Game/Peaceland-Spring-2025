#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Peaceland.Notebook.Editor
{
    /// <summary>
    /// Pick an entry from a dropdown and drop a ready-to-play collectible into the scene.
    /// Designers use this instead of hand-adding NotebookCollectTrigger, a collider and a glow.
    /// </summary>
    public sealed class NotebookCollectibleSpawner : EditorWindow
    {
        internal enum SpawnKind
        {
            WorldSprite = 0,
            UIButton = 1,
            AttachToSelection = 2,
        }

        private NotebookEntryDefinition[] entries = new NotebookEntryDefinition[0];
        private string[] entryLabels = new string[0];
        private int selectedIndex;
        private SpawnKind spawnKind = SpawnKind.WorldSprite;
        private bool addGlow = true;
        private bool disableAfterCollect = true;

        [MenuItem("Peaceland/Notebook/Add Collectible To Scene...", false, 20)]
        public static void Open()
        {
            NotebookCollectibleSpawner window = GetWindow<NotebookCollectibleSpawner>(true, "Add Collectible");
            window.minSize = new Vector2(420f, 260f);
            window.ReloadEntries();
            window.Show();
        }

        private void OnFocus()
        {
            ReloadEntries();
        }

        private void ReloadEntries()
        {
            entries = AssetDatabase.FindAssets("t:NotebookEntryDefinition")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>)
                .Where(entry => entry != null)
                .OrderBy(entry => entry.Section)
                .ThenBy(entry => entry.EntryId)
                .ToArray();

            entryLabels = entries
                .Select(entry => entry.Section + "/" + entry.EntryId + "  —  " + entry.Title)
                .ToArray();

            selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, entries.Length - 1));
        }

        private void OnGUI()
        {
            if (entries.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No NotebookEntryDefinition assets found.\n"
                    + "Create one with Assets > Create > Peaceland > Notebook > Entry.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("1. Which note does this unlock?", EditorStyles.boldLabel);
            selectedIndex = EditorGUILayout.Popup(selectedIndex, entryLabels);

            NotebookEntryDefinition entry = entries[selectedIndex];
            WarnIfNotInDatabase(entry);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("2. What kind of object?", EditorStyles.boldLabel);
            spawnKind = (SpawnKind)EditorGUILayout.EnumPopup(spawnKind);
            switch (spawnKind)
            {
                case SpawnKind.WorldSprite:
                    EditorGUILayout.HelpBox(
                        "Creates a sprite in the world with a collider. The player clicks it to collect.",
                        MessageType.None);
                    break;
                case SpawnKind.UIButton:
                    EditorGUILayout.HelpBox(
                        "Creates a UI button under the selected Canvas (or a new Canvas).",
                        MessageType.None);
                    break;
                case SpawnKind.AttachToSelection:
                    EditorGUILayout.HelpBox(
                        Selection.activeGameObject != null
                            ? "Adds collect behaviour to '" + Selection.activeGameObject.name + "'."
                            : "Select a GameObject in the Hierarchy first.",
                        Selection.activeGameObject != null ? MessageType.None : MessageType.Warning);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("3. Options", EditorStyles.boldLabel);
            addGlow = EditorGUILayout.Toggle("Pulsing glow", addGlow);
            disableAfterCollect = EditorGUILayout.Toggle("Hide once collected", disableAfterCollect);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(
                spawnKind == SpawnKind.AttachToSelection && Selection.activeGameObject == null))
            {
                if (GUILayout.Button("Add To Scene", GUILayout.Height(32f)))
                {
                    Spawn(entry, spawnKind, Selection.activeGameObject, addGlow, disableAfterCollect);
                }
            }
        }

        private void WarnIfNotInDatabase(NotebookEntryDefinition entry)
        {
            NotebookDatabase database = AssetDatabase.FindAssets("t:NotebookDatabase")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<NotebookDatabase>)
                .FirstOrDefault(item => item != null);

            if (database == null)
            {
                EditorGUILayout.HelpBox("No NotebookDatabase asset found in the project.", MessageType.Error);
                return;
            }

            if (database.GetEntry(entry.EntryId) != null)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                "'" + entry.EntryId + "' is not in NotebookDatabase yet, so collecting it would do nothing.",
                MessageType.Warning);
            if (GUILayout.Button("Sync Notebook Database From Assets"))
            {
                EditorApplication.ExecuteMenuItem("Peaceland/Notebook/Sync Notebook Database From Assets");
            }
        }

        internal static GameObject Spawn(
            NotebookEntryDefinition entry,
            SpawnKind spawnKind,
            GameObject attachTarget,
            bool addGlow,
            bool disableAfterCollect)
        {
            GameObject target = spawnKind == SpawnKind.AttachToSelection
                ? attachTarget
                : CreateHost(entry, spawnKind);

            if (target == null)
            {
                return null;
            }

            ConfigureTrigger(target, entry, spawnKind, disableAfterCollect);

            if (addGlow && target.GetComponent<SpriteRenderer>() != null
                && target.GetComponent<NotebookCollectableGlowView>() == null)
            {
                Undo.AddComponent<NotebookCollectableGlowView>(target);
            }

            Selection.activeGameObject = target;
            EditorSceneManager.MarkSceneDirty(target.scene);
            Debug.Log("Collectible ready: '" + target.name + "' unlocks '" + entry.EntryId + "'.", target);
            return target;
        }

        private static GameObject CreateHost(NotebookEntryDefinition entry, SpawnKind spawnKind)
        {
            string hostName = "Collectible - " + entry.EntryId;

            if (spawnKind == SpawnKind.UIButton)
            {
                GameObject uiHost = new GameObject(hostName, typeof(RectTransform), typeof(CanvasRenderer));
                Undo.RegisterCreatedObjectUndo(uiHost, "Add Notebook Collectible");

                Canvas canvas = ResolveOrCreateCanvas();
                Undo.SetTransformParent(uiHost.transform, canvas.transform, "Add Notebook Collectible");
                NotebookContentAuthoring.EnsureEventSystem();

                UnityEngine.UI.Image image = Undo.AddComponent<UnityEngine.UI.Image>(uiHost);
                image.sprite = entry.Image;
                ((RectTransform)uiHost.transform).sizeDelta = new Vector2(160f, 160f);
                ((RectTransform)uiHost.transform).anchoredPosition = Vector2.zero;
                return uiHost;
            }

            GameObject host = new GameObject(hostName, typeof(SpriteRenderer));
            Undo.RegisterCreatedObjectUndo(host, "Add Notebook Collectible");

            SpriteRenderer renderer = host.GetComponent<SpriteRenderer>();
            renderer.sprite = entry.Image;

            BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(host);
            collider.size = renderer.sprite != null ? renderer.sprite.bounds.size : Vector2.one;

            host.transform.position = SceneViewCenter();
            return host;
        }

        /// <summary>
        /// A UI collectible only reacts to clicks when it sits under a Canvas that has a
        /// GraphicRaycaster, so build one when the scene has none rather than dropping an
        /// invisible object into the hierarchy.
        /// </summary>
        private static Canvas ResolveOrCreateCanvas()
        {
            Canvas canvas = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponentInParent<Canvas>()
                : null;
            if (canvas == null)
            {
                canvas = Object.FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                GameObject canvasObject = new GameObject(
                    "Canvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(UnityEngine.UI.CanvasScaler),
                    typeof(UnityEngine.UI.GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasObject, "Add Notebook Collectible");

                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                UnityEngine.UI.CanvasScaler scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
            else if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                Undo.AddComponent<UnityEngine.UI.GraphicRaycaster>(canvas.gameObject);
            }

            return canvas;
        }

        private static void ConfigureTrigger(
            GameObject target,
            NotebookEntryDefinition entry,
            SpawnKind spawnKind,
            bool disableAfterCollect)
        {
            NotebookCollectTrigger trigger = target.GetComponent<NotebookCollectTrigger>();
            if (trigger == null)
            {
                trigger = Undo.AddComponent<NotebookCollectTrigger>(target);
            }

            SerializedObject serialized = new SerializedObject(trigger);
            SerializedProperty entryList = serialized.FindProperty("entries");
            if (!ContainsEntry(entryList, entry))
            {
                int index = entryList.arraySize;
                entryList.InsertArrayElementAtIndex(index);
                entryList.GetArrayElementAtIndex(index).objectReferenceValue = entry;
            }

            serialized.FindProperty("disableAfterCollect").boolValue = disableAfterCollect;
            serialized.FindProperty("collectOnPointerClick").boolValue = spawnKind == SpawnKind.UIButton;
            serialized.FindProperty("collectOnMouseDown").boolValue = spawnKind != SpawnKind.UIButton;
            serialized.ApplyModifiedProperties();
        }

        private static bool ContainsEntry(SerializedProperty entryList, NotebookEntryDefinition entry)
        {
            for (int i = 0; i < entryList.arraySize; i++)
            {
                if (entryList.GetArrayElementAtIndex(i).objectReferenceValue == entry)
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector3 SceneViewCenter()
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null || view.camera == null)
            {
                return Vector3.zero;
            }

            Vector3 center = view.camera.transform.position + view.camera.transform.forward * 10f;
            center.z = 0f;
            return center;
        }
    }
}
#endif
