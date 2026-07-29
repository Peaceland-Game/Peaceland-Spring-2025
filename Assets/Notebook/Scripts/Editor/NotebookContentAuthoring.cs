#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Peaceland.Notebook.EditableScenePack;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Peaceland.Notebook.Editor
{
    public static class NotebookContentAuthoring
    {
        private const string DataFolder = "Assets/Notebook/Data";
        private const string DatabasePath = DataFolder + "/NotebookDatabase.asset";
        private const string FlowerSpritePath = "Assets/Art/flowers_interactable.PNG";
        private const string FloristScenePath = "Assets/Notebook/Scenes/NotebookTest_FloristItemCollect.unity";
        private const string FloristMinigameScenePath = "Assets/Notebook/Scenes/NotebookTest_FloristMinigame.unity";
        private const string RandJScenePath = "Assets/Notebook/Scenes/NotebookTest_RandJItemCollect.unity";
        private const string NewspaperScenePath = "Assets/Notebook/Scenes/NotebookTest_IntroNewspaper.unity";
        private const string HomeScenePath = "Assets/Notebook/Scenes/NoteBookTesting.unity";
        private const string DummyEntryPrefix = "NotebookEntry_DummyPage_";
        private const string FloristFlowerPrefix = "NotebookEntry_FloristFlower_";

        [MenuItem("Peaceland/Notebook/Sync Notebook Database From Assets")]
        public static void SyncNotebookDatabaseMenu()
        {
            SyncNotebookDatabaseFromAssets();
            AssetDatabase.SaveAssets();
            Debug.Log("NotebookDatabase synced from all entry assets under Assets/Notebook/Data.");
        }

        [MenuItem("Peaceland/Notebook/Prepare Playable Tonight")]
        public static void PreparePlayableTonightMenu()
        {
            AuthorAllNotebookTestScenesMenu();
            Debug.Log(
                "Notebook playtest ready. Press Play on NoteBookTesting. Top bar auto-appears in all test scenes for navigation.");
        }

        [MenuItem("Peaceland/Notebook/Author All Notebook Test Scenes")]
        public static void AuthorAllNotebookTestScenesMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            GenerateDummyPaginationEntries();
            GenerateFloristFlowerEntries();
            SyncNotebookDatabaseFromAssets();

            EditorSceneManager.OpenScene(HomeScenePath);
            NotebookOpenUIAuthoring.AuthorInActiveScene();
            NotebookScenePlayabilityEditor.EnsurePlayableActiveScene();
            EditorSceneManager.SaveOpenScenes();

            OpenAuthorScene(FloristScenePath, AuthorFloristItemCollectScene);
            OpenAuthorScene(NewspaperScenePath, AuthorIntroNewspaperScene);
            OpenAuthorScene(RandJScenePath, AuthorRandJItemCollectScene);
            OpenAuthorScene(FloristMinigameScenePath, AuthorFloristMinigameScene);

            AssetDatabase.SaveAssets();
            Debug.Log("All notebook test scenes authored. Home scene includes open-book UI; satellite scenes are collect-only with overlay + HUD.");
        }

        [MenuItem("Peaceland/Notebook/Author Intro Newspaper Scene")]
        public static void AuthorIntroNewspaperSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(NewspaperScenePath);
            AuthorIntroNewspaperScene();
            EditorSceneManager.SaveOpenScenes();
        }

        [MenuItem("Peaceland/Notebook/Author R&J Item Collect Scene")]
        public static void AuthorRandJItemCollectSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(RandJScenePath);
            AuthorRandJItemCollectScene();
            EditorSceneManager.SaveOpenScenes();
        }

        [MenuItem("Peaceland/Notebook/Author Florist Minigame Scene")]
        public static void AuthorFloristMinigameSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(FloristMinigameScenePath);
            AuthorFloristMinigameScene();
            EditorSceneManager.SaveOpenScenes();
        }

        [MenuItem("Peaceland/Notebook/Generate Dummy Pagination Entries")]
        public static void GenerateDummyPaginationEntriesMenu()
        {
            GenerateDummyPaginationEntries();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created 20 dummy pagination notebook entries and updated NotebookDatabase.");
        }

        [MenuItem("Peaceland/Notebook/Author Florist Item Collect Scene")]
        public static void AuthorFloristItemCollectSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(FloristScenePath);
            AuthorFloristItemCollectScene();
            EditorSceneManager.SaveOpenScenes();
        }

        [MenuItem("Peaceland/Notebook/Ensure Collect Overlay In Active Scene")]
        public static void EnsureCollectOverlayMenu()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("No canvas found in active scene.");
                return;
            }

            NotebookOverlayView overlay = NotebookOverlayAuthoring.EnsureOverlay(canvas.transform);
            NotebookCollectHintHost host = Object.FindFirstObjectByType<NotebookCollectHintHost>();
            if (host == null)
            {
                GameObject hostObject = new GameObject("Notebook Collect Hint Host", typeof(NotebookCollectHintHost));
                host = hostObject.GetComponent<NotebookCollectHintHost>();
            }

            SerializedObject hostObjectSerialized = new SerializedObject(host);
            hostObjectSerialized.FindProperty("overlayView").objectReferenceValue = overlay;
            hostObjectSerialized.ApplyModifiedPropertiesWithoutUndo();

            NotebookController controller = NotebookSceneLookup.FindController();
            if (controller != null)
            {
                SerializedObject controllerSerialized = new SerializedObject(controller);
                controllerSerialized.FindProperty("overlayView").objectReferenceValue = overlay;
                controllerSerialized.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("Collect overlay and hint host wired in active scene.");
        }

        public static void GenerateDummyPaginationEntries()
        {
            EnsureDataFolder();
            NotebookDatabase database = LoadDatabase();
            List<NotebookEntryDefinition> entries = database.Entries.ToList();

            float[] heights = { 180f, 220f, 260f, 300f, 240f };
            for (int i = 1; i <= 20; i++)
            {
                string assetName = DummyEntryPrefix + i.ToString("00");
                string assetPath = DataFolder + "/" + assetName + ".asset";
                NotebookEntryDefinition entry = AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(assetPath);
                if (entry == null)
                {
                    entry = ScriptableObject.CreateInstance<NotebookEntryDefinition>();
                    AssetDatabase.CreateAsset(entry, assetPath);
                }

                SerializedObject entryObject = new SerializedObject(entry);
                entryObject.FindProperty("entryId").stringValue = assetName;
                entryObject.FindProperty("section").enumValueIndex = (int)NotebookSection.Present;
                entryObject.FindProperty("categoryId").stringValue = "scene-collected";
                entryObject.FindProperty("categoryDisplayName").stringValue = "scene collected";
                entryObject.FindProperty("categorySortOrder").intValue = 10;
                entryObject.FindProperty("title").stringValue = "Pagination " + i.ToString("00");
                entryObject.FindProperty("bodyText").stringValue = string.Empty;
                entryObject.FindProperty("theoreticalOrder").intValue = 100 + (i * 10);
                entryObject.FindProperty("layoutHeight").floatValue = heights[(i - 1) % heights.Length];
                entryObject.FindProperty("sortOrder").intValue = 100 + (i * 10);
                entryObject.ApplyModifiedPropertiesWithoutUndo();

                if (!entries.Any(existing => existing != null && existing.EntryId == entry.EntryId))
                {
                    entries.Add(entry);
                }
            }

            WriteDatabaseEntries(database, entries);
        }

        public static void AuthorFloristItemCollectScene()
        {
            GenerateFloristFlowerEntries();
            CleanupLegacyNavigatorUi();

            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();
            NotebookOverlayView overlay = NotebookOverlayAuthoring.EnsureOverlay(canvas.transform);

            NotebookCollectHintHost hintHost = Object.FindFirstObjectByType<NotebookCollectHintHost>();
            if (hintHost == null)
            {
                GameObject hostObject = new GameObject("Notebook Collect Hint Host", typeof(NotebookCollectHintHost));
                hintHost = hostObject.GetComponent<NotebookCollectHintHost>();
            }

            SerializedObject hostSerialized = new SerializedObject(hintHost);
            hostSerialized.FindProperty("overlayView").objectReferenceValue = overlay;
            hostSerialized.ApplyModifiedPropertiesWithoutUndo();

            EnsureHudButton(canvas.transform, satelliteScene: true);
            EnsureReturnHomeButton(canvas.transform);
            EnsureWorldCollectCamera();
            EnsureCollectibleFlowers();

            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("Florist item collect scene authored with glow flowers and cross-scene collect wiring.");
        }

        public static void AuthorIntroNewspaperScene()
        {
            CleanupLegacyNavigatorUi();
            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();
            NotebookOverlayView overlay = NotebookOverlayAuthoring.EnsureOverlay(canvas.transform);
            EnsureCollectHintHost(overlay);
            EnsureHudButton(canvas.transform, satelliteScene: true);
            EnsureReturnHomeButton(canvas.transform);
            EnsureUiCollectible(
                canvas.transform,
                "Newspaper Collectible",
                "Click to collect newspaper note",
                new Vector2(0.5f, 0.5f),
                new Vector2(520f, 680f),
                LoadEntry("NotebookEntry_PresentNewspaper"));
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("Intro newspaper scene authored with clickable newspaper collectible.");
        }

        public static void AuthorRandJItemCollectScene()
        {
            CleanupLegacyNavigatorUi();
            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();
            NotebookOverlayView overlay = NotebookOverlayAuthoring.EnsureOverlay(canvas.transform);
            EnsureCollectHintHost(overlay);
            EnsureHudButton(canvas.transform, satelliteScene: true);
            EnsureReturnHomeButton(canvas.transform);
            EnsureWorldCollectCamera();
            EnsureWorldCollectible(
                "R&J Collectibles",
                "R&J Balcony Fragment",
                new Vector3(-3f, 0.2f, 0f),
                LoadEntry("NotebookEntry_Memory2RJBalcony"));
            EnsureWorldCollectible(
                "R&J Collectibles",
                "R&J Letter Draft",
                new Vector3(3.2f, -0.3f, 0f),
                LoadEntry("NotebookEntry_Memory2RJLetter"));
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("R&J item collect scene authored with two glowing collectibles.");
        }

        public static void AuthorFloristMinigameScene()
        {
            CleanupLegacyNavigatorUi();
            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();
            NotebookOverlayView overlay = NotebookOverlayAuthoring.EnsureOverlay(canvas.transform);
            EnsureCollectHintHost(overlay);
            EnsureHudButton(canvas.transform, satelliteScene: true);
            EnsureReturnHomeButton(canvas.transform);
            EnsureMinigameCompleteButton(canvas.transform, LoadEntry("NotebookEntry_Memory1FloristFlower"));
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("Florist minigame scene authored with minigame-complete collect button.");
        }

        private static void OpenAuthorScene(string scenePath, System.Action authorAction)
        {
            EditorSceneManager.OpenScene(scenePath);
            authorAction();
            NotebookScenePlayabilityEditor.EnsurePlayableActiveScene();
            EditorSceneManager.SaveOpenScenes();
        }

        private static void SyncNotebookDatabaseFromAssets()
        {
            EnsureDataFolder();
            NotebookDatabase database = LoadDatabase();
            string[] guids = AssetDatabase.FindAssets("t:NotebookEntryDefinition", new[] { DataFolder });
            List<NotebookEntryDefinition> entries = new List<NotebookEntryDefinition>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                NotebookEntryDefinition entry = AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(path);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }

            entries = entries
                .OrderBy(entry => entry.TheoreticalOrder)
                .ThenBy(entry => entry.EntryId)
                .ToList();
            WriteDatabaseEntries(database, entries);
        }

        private static NotebookEntryDefinition LoadEntry(string assetName)
        {
            return AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(DataFolder + "/" + assetName + ".asset");
        }

        private static void EnsureCollectHintHost(NotebookOverlayView overlay)
        {
            NotebookCollectHintHost hintHost = Object.FindFirstObjectByType<NotebookCollectHintHost>();
            if (hintHost == null)
            {
                GameObject hostObject = new GameObject("Notebook Collect Hint Host", typeof(NotebookCollectHintHost));
                hintHost = hostObject.GetComponent<NotebookCollectHintHost>();
            }

            SerializedObject hostSerialized = new SerializedObject(hintHost);
            hostSerialized.FindProperty("overlayView").objectReferenceValue = overlay;
            hostSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureUiCollectible(
            Transform canvas,
            string objectName,
            string label,
            Vector2 anchorCenter,
            Vector2 size,
            NotebookEntryDefinition entry)
        {
            Transform existing = canvas.Find(objectName);
            GameObject collectibleObject = existing != null ? existing.gameObject : new GameObject(objectName, typeof(RectTransform));
            collectibleObject.transform.SetParent(canvas, false);
            RectTransform rect = collectibleObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorCenter;
            rect.anchorMax = anchorCenter;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Image image = collectibleObject.GetComponent<Image>();
            if (image == null)
            {
                image = collectibleObject.AddComponent<Image>();
            }

            image.color = new Color(0.92f, 0.88f, 0.78f, 1f);
            image.raycastTarget = true;

            NotebookCollectableGlowView glow = collectibleObject.GetComponent<NotebookCollectableGlowView>();
            if (glow == null)
            {
                glow = collectibleObject.AddComponent<NotebookCollectableGlowView>();
            }

            NotebookCollectTrigger trigger = collectibleObject.GetComponent<NotebookCollectTrigger>();
            if (trigger == null)
            {
                trigger = collectibleObject.AddComponent<NotebookCollectTrigger>();
            }

            WireCollectTrigger(trigger, entry, collectOnPointerClick: true, collectOnMouseDown: false);

            Transform labelTransform = collectibleObject.transform.Find("Label");
            if (labelTransform == null)
            {
                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(collectibleObject.transform, false);
                labelTransform = labelObject.transform;
            }

            TMP_Text labelText = labelTransform.GetComponent<TMP_Text>();
            labelText.text = label;
            labelText.fontSize = 28f;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = new Color(0.2f, 0.16f, 0.12f, 1f);
            Stretch(labelTransform as RectTransform);
        }

        private static void EnsureWorldCollectible(
            string rootName,
            string objectName,
            Vector3 position,
            NotebookEntryDefinition entry)
        {
            Sprite flowerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FlowerSpritePath);
            if (flowerSprite == null)
            {
                Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(FlowerSpritePath);
                flowerSprite = sprites.OfType<Sprite>().FirstOrDefault();
            }

            Transform root = GameObject.Find(rootName) != null
                ? GameObject.Find(rootName).transform
                : new GameObject(rootName).transform;

            Transform collectibleTransform = root.Find(objectName);
            GameObject collectibleObject = collectibleTransform != null
                ? collectibleTransform.gameObject
                : new GameObject(objectName);

            collectibleObject.transform.SetParent(root, false);
            collectibleObject.transform.position = position;
            collectibleObject.transform.localScale = Vector3.one * 0.45f;

            SpriteRenderer renderer = collectibleObject.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = collectibleObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = flowerSprite;
            renderer.sortingOrder = 10;

            CircleCollider2D collider = collectibleObject.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = collectibleObject.AddComponent<CircleCollider2D>();
            }

            collider.radius = 1.1f;

            NotebookCollectableGlowView glow = collectibleObject.GetComponent<NotebookCollectableGlowView>();
            if (glow == null)
            {
                glow = collectibleObject.AddComponent<NotebookCollectableGlowView>();
            }

            NotebookCollectTrigger trigger = collectibleObject.GetComponent<NotebookCollectTrigger>();
            if (trigger == null)
            {
                trigger = collectibleObject.AddComponent<NotebookCollectTrigger>();
            }

            WireCollectTrigger(trigger, entry, collectOnPointerClick: true, collectOnMouseDown: true);
        }

        private static void EnsureWorldCollectCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = Object.FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
            }

            if (camera.GetComponent<Physics2DRaycaster>() == null)
            {
                camera.gameObject.AddComponent<Physics2DRaycaster>();
            }
        }

        private static void EnsureMinigameCompleteButton(Transform canvas, NotebookEntryDefinition entry)
        {
            Transform existing = canvas.Find("Minigame Complete Button");
            GameObject buttonObject = existing != null
                ? existing.gameObject
                : new GameObject("Minigame Complete Button", typeof(RectTransform), typeof(Image), typeof(Button));

            buttonObject.transform.SetParent(canvas, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 72f);
            rect.anchoredPosition = Vector2.zero;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.35f, 0.55f, 0.38f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            NotebookCollectTrigger trigger = buttonObject.GetComponent<NotebookCollectTrigger>();
            if (trigger == null)
            {
                trigger = buttonObject.AddComponent<NotebookCollectTrigger>();
            }

            WireCollectTrigger(trigger, entry, collectOnPointerClick: true, collectOnMouseDown: false);

            Transform labelTransform = buttonObject.transform.Find("Label");
            if (labelTransform == null)
            {
                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(buttonObject.transform, false);
                labelTransform = labelObject.transform;
            }

            TMP_Text labelText = labelTransform.GetComponent<TMP_Text>();
            labelText.text = "Complete Florist Minigame";
            labelText.fontSize = 24f;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            Stretch(labelTransform as RectTransform);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(trigger.Collect);
        }

        private static void EnsureReturnHomeButton(Transform canvas)
        {
            Transform existing = canvas.Find("Return Home");
            GameObject buttonObject = existing != null
                ? existing.gameObject
                : new GameObject("Return Home", typeof(RectTransform), typeof(Image), typeof(Button));

            buttonObject.transform.SetParent(canvas, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(220f, 56f);
            rect.anchoredPosition = new Vector2(32f, 32f);

            Image image = buttonObject.GetComponent<Image>();
            if (image == null)
            {
                image = buttonObject.AddComponent<Image>();
            }

            image.color = new Color(0.18f, 0.14f, 0.1f, 0.88f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            if (buttonObject.GetComponent<NotebookReturnHomeButton>() == null)
            {
                buttonObject.AddComponent<NotebookReturnHomeButton>();
            }

            NotebookSceneInteractMarker marker = buttonObject.GetComponent<NotebookSceneInteractMarker>();
            if (marker == null)
            {
                marker = buttonObject.AddComponent<NotebookSceneInteractMarker>();
            }

            marker.Configure(NotebookSceneInteractKind.ReturnHome, "Return to home scene");

            Transform labelTransform = buttonObject.transform.Find("Label");
            if (labelTransform == null)
            {
                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(buttonObject.transform, false);
                labelTransform = labelObject.transform;
            }

            TMP_Text labelText = labelTransform.GetComponent<TMP_Text>();
            if (labelText == null)
            {
                labelText = labelTransform.gameObject.AddComponent<TextMeshProUGUI>();
            }

            labelText.text = "Return Home";
            labelText.fontSize = 18f;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            Stretch(labelTransform as RectTransform);
        }

        private static void WireCollectTrigger(
            NotebookCollectTrigger trigger,
            NotebookEntryDefinition entry,
            bool collectOnPointerClick,
            bool collectOnMouseDown)
        {
            SerializedObject triggerObject = new SerializedObject(trigger);
            SerializedProperty entriesProperty = triggerObject.FindProperty("entries");
            entriesProperty.arraySize = 1;
            entriesProperty.GetArrayElementAtIndex(0).objectReferenceValue = entry;
            triggerObject.FindProperty("collectOnPointerClick").boolValue = collectOnPointerClick;
            triggerObject.FindProperty("collectOnMouseDown").boolValue = collectOnMouseDown;
            triggerObject.FindProperty("disableAfterCollect").boolValue = true;
            triggerObject.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Peaceland/Notebook/Generate Florist Flower Entries")]
        public static void GenerateFloristFlowerEntriesMenu()
        {
            GenerateFloristFlowerEntries();
            AssetDatabase.SaveAssets();
            Debug.Log("Florist flower notebook entries generated and added to database.");
        }

        private static void GenerateFloristFlowerEntries()
        {
            EnsureDataFolder();
            NotebookDatabase database = LoadDatabase();
            List<NotebookEntryDefinition> entries = database.Entries.ToList();

            for (int i = 1; i <= 5; i++)
            {
                string assetName = FloristFlowerPrefix + i.ToString("00");
                string assetPath = DataFolder + "/" + assetName + ".asset";
                NotebookEntryDefinition entry = AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(assetPath);
                if (entry == null)
                {
                    entry = ScriptableObject.CreateInstance<NotebookEntryDefinition>();
                    AssetDatabase.CreateAsset(entry, assetPath);
                }

                SerializedObject entryObject = new SerializedObject(entry);
                entryObject.FindProperty("entryId").stringValue = assetName;
                entryObject.FindProperty("section").enumValueIndex = (int)NotebookSection.Memory1;
                entryObject.FindProperty("categoryId").stringValue = "scene-collected";
                entryObject.FindProperty("categoryDisplayName").stringValue = "scene collected";
                entryObject.FindProperty("categorySortOrder").intValue = 20;
                entryObject.FindProperty("title").stringValue = "Florist Flower " + i.ToString("00");
                entryObject.FindProperty("bodyText").stringValue = "A flower collected from the florist scene.";
                entryObject.FindProperty("theoreticalOrder").intValue = 200 + i;
                entryObject.FindProperty("layoutHeight").floatValue = 220f;
                entryObject.FindProperty("sortOrder").intValue = 200 + i;
                entryObject.ApplyModifiedPropertiesWithoutUndo();

                if (!entries.Any(existing => existing != null && existing.EntryId == entry.EntryId))
                {
                    entries.Add(entry);
                }
            }

            WriteDatabaseEntries(database, entries);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureCollectibleFlowers()
        {
            Sprite flowerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FlowerSpritePath);
            if (flowerSprite == null)
            {
                Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(FlowerSpritePath);
                flowerSprite = sprites.OfType<Sprite>().FirstOrDefault();
            }

            if (flowerSprite == null)
            {
                Debug.LogWarning("Flower sprite not found at " + FlowerSpritePath);
                return;
            }

            Transform root = GameObject.Find("Florist Collectibles") != null
                ? GameObject.Find("Florist Collectibles").transform
                : new GameObject("Florist Collectibles").transform;

            Vector3[] positions =
            {
                new Vector3(-4.5f, 0.5f, 0f),
                new Vector3(-2f, -0.8f, 0f),
                new Vector3(0.5f, 0.2f, 0f),
                new Vector3(2.8f, -0.4f, 0f),
                new Vector3(4.8f, 0.8f, 0f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                string objectName = "Florist Flower " + (i + 1).ToString("00");
                Transform flowerTransform = root.Find(objectName);
                GameObject flowerObject = flowerTransform != null
                    ? flowerTransform.gameObject
                    : new GameObject(objectName);

                flowerObject.transform.SetParent(root, false);
                flowerObject.transform.position = positions[i];
                flowerObject.transform.localScale = Vector3.one * 0.45f;

                SpriteRenderer renderer = flowerObject.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = flowerObject.AddComponent<SpriteRenderer>();
                }

                renderer.sprite = flowerSprite;
                renderer.sortingOrder = 10 + i;

                CircleCollider2D collider = flowerObject.GetComponent<CircleCollider2D>();
                if (collider == null)
                {
                    collider = flowerObject.AddComponent<CircleCollider2D>();
                }

                collider.isTrigger = false;
                collider.radius = 1.1f;

                NotebookCollectableGlowView glow = flowerObject.GetComponent<NotebookCollectableGlowView>();
                if (glow == null)
                {
                    glow = flowerObject.AddComponent<NotebookCollectableGlowView>();
                }

                NotebookCollectTrigger trigger = flowerObject.GetComponent<NotebookCollectTrigger>();
                if (trigger == null)
                {
                    trigger = flowerObject.AddComponent<NotebookCollectTrigger>();
                }

                string entryAssetPath = DataFolder + "/" + FloristFlowerPrefix + (i + 1).ToString("00") + ".asset";
                NotebookEntryDefinition entry = AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(entryAssetPath);
                SerializedObject triggerObject = new SerializedObject(trigger);
                SerializedProperty entriesProperty = triggerObject.FindProperty("entries");
                entriesProperty.arraySize = 1;
                entriesProperty.GetArrayElementAtIndex(0).objectReferenceValue = entry;
                triggerObject.FindProperty("collectOnPointerClick").boolValue = true;
                triggerObject.FindProperty("collectOnMouseDown").boolValue = true;
                triggerObject.FindProperty("disableAfterCollect").boolValue = true;
                triggerObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void EnsureHudButton(Transform canvasTransform, bool satelliteScene)
        {
            Transform hud = canvasTransform.Find("Notebook HUD Button");
            if (hud == null)
            {
                GameObject buttonObject = new GameObject(
                    "Notebook HUD Button",
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Button),
                    satelliteScene ? typeof(NotebookReturnHomeButton) : typeof(NotebookOpenButton));
                hud = buttonObject.transform;
                hud.SetParent(canvasTransform, false);
                RectTransform rect = hud as RectTransform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(24f, 24f);
                rect.sizeDelta = new Vector2(satelliteScene ? 168f : 148f, 52f);
                hud.GetComponent<Image>().color = new Color(0.42f, 0.31f, 0.21f, 0.95f);
                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(hud, false);
                TMP_Text label = labelObject.GetComponent<TextMeshProUGUI>();
                label.text = satelliteScene ? "Return Home" : "Notebook";
                label.fontSize = 20f;
                label.alignment = TextAlignmentOptions.Center;
                label.color = Color.white;
                Stretch(label.rectTransform);
            }
            else
            {
                TMP_Text label = hud.Find("Label")?.GetComponent<TMP_Text>();
                if (label != null)
                {
                    label.text = satelliteScene ? "Return Home" : "Notebook";
                }

                NotebookOpenButton openButton = hud.GetComponent<NotebookOpenButton>();
                NotebookReturnHomeButton returnButton = hud.GetComponent<NotebookReturnHomeButton>();
                if (satelliteScene)
                {
                    if (openButton != null)
                    {
                        Object.DestroyImmediate(openButton);
                    }

                    if (returnButton == null)
                    {
                        hud.gameObject.AddComponent<NotebookReturnHomeButton>();
                    }
                }
                else
                {
                    if (returnButton != null)
                    {
                        Object.DestroyImmediate(returnButton);
                    }

                    if (openButton == null)
                    {
                        hud.gameObject.AddComponent<NotebookOpenButton>();
                    }
                }
            }
        }

        private static void CleanupLegacyNavigatorUi()
        {
            string[] legacyNames = { "Notebook Test Navigator", "Notebook Test Navigator Canvas", "Notebook Test UI" };
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            for (int c = 0; c < canvases.Length; c++)
            {
                for (int i = 0; i < legacyNames.Length; i++)
                {
                    Transform legacy = canvases[c].transform.Find(legacyNames[i]);
                    if (legacy != null)
                    {
                        Object.DestroyImmediate(legacy.gameObject);
                    }
                }
            }

            NotebookTestSceneNavigator[] navigators = Object.FindObjectsByType<NotebookTestSceneNavigator>(FindObjectsSortMode.None);
            for (int i = 0; i < navigators.Length; i++)
            {
                Object.DestroyImmediate(navigators[i].gameObject);
            }
        }

        private static void EnsureEventSystem()
        {
            EventSystem existing = Object.FindFirstObjectByType<EventSystem>();
            if (existing != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Canvas EnsureCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                if (canvas.gameObject.name != "Notebook Canvas")
                {
                    canvas.gameObject.name = "Notebook Canvas";
                }

                return canvas;
            }

            GameObject canvasObject = new GameObject("Notebook Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas created = canvasObject.GetComponent<Canvas>();
            created.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return created;
        }

        private static void EnsureDataFolder()
        {
            if (!AssetDatabase.IsValidFolder(DataFolder))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Notebook/Data"));
                AssetDatabase.Refresh();
            }
        }

        private static NotebookDatabase LoadDatabase()
        {
            NotebookDatabase database = AssetDatabase.LoadAssetAtPath<NotebookDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<NotebookDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            return database;
        }

        private static void WriteDatabaseEntries(NotebookDatabase database, List<NotebookEntryDefinition> entries)
        {
            SerializedObject databaseObject = new SerializedObject(database);
            SerializedProperty entriesProperty = databaseObject.FindProperty("entries");
            entriesProperty.arraySize = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                entriesProperty.GetArrayElementAtIndex(i).objectReferenceValue = entries[i];
            }

            databaseObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
