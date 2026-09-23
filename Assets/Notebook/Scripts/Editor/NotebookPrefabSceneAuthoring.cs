using System;
using System.Collections.Generic;
using Peaceland.Notebook;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Peaceland.Notebook.Editor
{
    public static class NotebookPrefabSceneAuthoring
    {
        public const string ProductionPrefabPath =
            "Assets/Notebook/Prefabs/NotebookProductionSceneUI.prefab";
        public const string TestControlsPrefabPath =
            "Assets/Notebook/Prefabs/NotebookTestSceneControls.prefab";

        private static readonly string[] TestScenePaths =
        {
            "Assets/Notebook/Scenes/NoteBookTesting.unity",
            "Assets/Notebook/Scenes/NotebookTest_FloristMinigame.unity",
            "Assets/Notebook/Scenes/NotebookTest_FloristItemCollect.unity",
            "Assets/Notebook/Scenes/NotebookTest_RandJItemCollect.unity",
            "Assets/Notebook/Scenes/NotebookTest_IntroNewspaper.unity",
        };

        private static readonly (string label, string sceneName)[] SceneLinks =
        {
            ("Notebook Home", "NoteBookTesting"),
            ("Florist Minigame", "NotebookTest_FloristMinigame"),
            ("Florist Collect", "NotebookTest_FloristItemCollect"),
            ("R&J Collect", "NotebookTest_RandJItemCollect"),
            ("Intro Newspaper", "NotebookTest_IntroNewspaper"),
        };

        [MenuItem("Peaceland/Notebook/Prefabs/Rebuild Test Controls Prefab")]
        public static void RebuildTestControlsPrefab()
        {
            BuildTestControlsPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log($"Notebook test controls rebuilt: {TestControlsPrefabPath}");
        }

        [MenuItem("Peaceland/Notebook/Prefabs/Rebuild Production Prefab")]
        public static void RebuildProductionPrefab()
        {
            SanitizeProductionPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log($"Notebook production prefab rebuilt: {ProductionPrefabPath}");
        }

        [MenuItem("Peaceland/Notebook/Prefabs/Migrate All Notebook Test Scenes")]
        public static void MigrateAllTestScenesMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            BuildAndMigrateAll();
        }

        /// <summary>Batch-mode entry point used by migration and CI validation.</summary>
        public static void BuildAndMigrateAll()
        {
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                SanitizeProductionPrefab();
                BuildTestControlsPrefab();
                foreach (string scenePath in TestScenePaths)
                {
                    MigrateScene(scenePath);
                }

                AssetDatabase.SaveAssets();
                Debug.Log("Notebook prefab migration complete.");
            }
            finally
            {
                if (!Application.isBatchMode && previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }
        }

        private static void SanitizeProductionPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(ProductionPrefabPath);
            try
            {
                foreach (NotebookTestHarness harness in
                         root.GetComponentsInChildren<NotebookTestHarness>(true))
                {
                    UnityEngine.Object.DestroyImmediate(harness);
                }

                foreach (NotebookTestSceneBootstrap bootstrap in
                         root.GetComponentsInChildren<NotebookTestSceneBootstrap>(true))
                {
                    UnityEngine.Object.DestroyImmediate(bootstrap);
                }

                RepairProductionPrefabLayout(root);
                PrefabUtility.SaveAsPrefabAsset(root, ProductionPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RepairProductionPrefabLayout(GameObject prefabRoot)
        {
            NotebookUIShellReferences shell =
                prefabRoot.GetComponentInChildren<NotebookUIShellReferences>(true);
            if (shell == null || shell.OpenRoot == null)
            {
                throw new InvalidOperationException(
                    "Notebook production prefab is missing its authored UI shell references.");
            }

            RectTransform openRoot = shell.OpenRoot;
            NotebookBookArtLayout artLayout = openRoot.GetComponent<NotebookBookArtLayout>();
            float topInset = artLayout != null
                ? artLayout.BookmarkTopInset
                : NotebookBookShellLayout.BookmarkTopInset;
            float horizontalOffset = artLayout != null
                ? artLayout.BookmarkHorizontalOffset
                : NotebookBookShellLayout.BookmarkHorizontalOffset;

            NotebookBookShellLayout.ApplyOpenRoot(openRoot, true);
            NotebookBookShellLayout.ApplyAnimationOverlay(
                shell.BookBackground != null ? shell.BookBackground.rectTransform : null,
                true);
            NotebookBookShellLayout.ApplyPagesViewport(shell.PagesViewport, true);
            NotebookBookShellLayout.ApplySpreadBody(
                shell.DirectoryPageRoot != null
                    ? shell.DirectoryPageRoot.transform as RectTransform
                    : null,
                true);
            NotebookBookShellLayout.ApplySpreadBody(
                shell.SectionPageRoot != null
                    ? shell.SectionPageRoot.transform as RectTransform
                    : null,
                true);
            RepairDirectorySpread(shell.DirectoryPageRoot);
            RepairSectionSpreads(shell.SectionPageRoot);
            NotebookBookShellLayout.ApplyAnimationOverlay(
                shell.AnimationView != null
                    ? shell.AnimationView.transform as RectTransform
                    : null,
                true);
            NotebookBookShellLayout.ApplyBookmarkRailLeft(
                openRoot.Find("Bookmark Tabs Left") as RectTransform,
                topInset,
                horizontalOffset,
                true);
            NotebookBookShellLayout.ApplyBookmarkRailRight(
                openRoot.Find("Bookmark Tabs Right") as RectTransform,
                topInset,
                horizontalOffset,
                true);

            foreach (NotebookPageTurnHitLayer hitLayer in
                     openRoot.GetComponentsInChildren<NotebookPageTurnHitLayer>(true))
            {
                NotebookBookShellLayout.ApplyPageTurnHitLayer(
                    hitLayer.transform as RectTransform,
                    true);
                NotebookBookShellLayout.ApplyPageTurnHitArea(
                    hitLayer.PreviousPageButton != null
                        ? hitLayer.PreviousPageButton.transform as RectTransform
                        : null,
                    true,
                    hitLayer.HitWidth,
                    true);
                NotebookBookShellLayout.ApplyPageTurnHitArea(
                    hitLayer.NextPageButton != null
                        ? hitLayer.NextPageButton.transform as RectTransform
                        : null,
                    false,
                    hitLayer.HitWidth,
                    true);
            }
        }

        private static void RepairDirectorySpread(GameObject directoryRoot)
        {
            if (directoryRoot == null)
            {
                return;
            }

            RectTransform leftPage = directoryRoot.transform.Find("Left Page") as RectTransform;
            NotebookBookShellLayout.ApplySpreadPage(leftPage, true, true);
            NotebookBookShellLayout.ApplySpreadPage(
                directoryRoot.transform.Find("Right Page") as RectTransform,
                false,
                true);

            RectTransform lines = leftPage != null
                ? leftPage.Find("Directory Lines") as RectTransform
                : null;
            if (lines != null)
            {
                lines.anchorMin = Vector2.zero;
                lines.anchorMax = Vector2.one;
                lines.pivot = new Vector2(0.5f, 0.5f);
                lines.anchoredPosition = Vector2.zero;
                lines.offsetMin = new Vector2(12f, 12f);
                lines.offsetMax = new Vector2(-12f, -52f);
            }
        }

        private static void RepairSectionSpreads(GameObject sectionRoot)
        {
            Transform entriesArea = sectionRoot != null
                ? sectionRoot.transform.Find("Entries Area")
                : null;
            NotebookBookLayoutSettings settings = ReadLayoutSettings(sectionRoot);
            NotebookBookShellLayout.ApplySpreadBody(entriesArea as RectTransform, true);
            RepairTwoPageRoot(
                entriesArea != null ? entriesArea.Find("Index Root") : null,
                settings.indexEntryRowSpacing);
            RepairTwoPageRoot(
                entriesArea != null ? entriesArea.Find("Content Root") : null,
                settings.contentEntrySpacing);
        }

        private static NotebookBookLayoutSettings ReadLayoutSettings(GameObject sectionRoot)
        {
            NotebookBookLayoutSettings settings = new NotebookBookLayoutSettings();
            NotebookController controller = sectionRoot != null
                ? sectionRoot.GetComponentInParent<NotebookController>(true)
                : null;
            if (controller == null)
            {
                return settings;
            }

            SerializedProperty serializedSettings =
                new SerializedObject(controller).FindProperty("layoutSettings");
            if (serializedSettings == null)
            {
                return settings;
            }

            settings.indexEntryRowSpacing =
                serializedSettings.FindPropertyRelative("indexEntryRowSpacing").floatValue;
            settings.contentEntrySpacing =
                serializedSettings.FindPropertyRelative("contentEntrySpacing").floatValue;
            return settings;
        }

        private static void RepairTwoPageRoot(Transform root, float entrySpacing)
        {
            NotebookBookShellLayout.ApplySpreadBody(root as RectTransform, true);
            if (root == null)
            {
                return;
            }

            RepairPage(root.Find("Left Page"), true, entrySpacing);
            RepairPage(root.Find("Right Page"), false, entrySpacing);
        }

        private static void RepairPage(Transform page, bool isLeftPage, float entrySpacing)
        {
            NotebookBookShellLayout.ApplySpreadPage(page as RectTransform, isLeftPage, true);
            RepairPageContainer(page != null ? page.Find("Container") : null, entrySpacing);
        }

        private static void RepairPageContainer(Transform container, float entrySpacing)
        {
            RectTransform rect = container as RectTransform;
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = rect.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.padding = new RectOffset();
            layout.spacing = Mathf.Max(0f, entrySpacing);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static void MigrateScene(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EnsurePrefabsInActiveScene(true);
            EditorSceneManager.SaveScene(scene);
        }

        public static void EnsurePrefabsInActiveScene(bool includeTestControls)
        {
            Scene scene = SceneManager.GetActiveScene();
            RemoveLegacyNotebookObjects();
            GameObject productionPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(ProductionPrefabPath);
            if (productionPrefab == null)
            {
                throw new InvalidOperationException(
                    $"Notebook production prefab is missing: {ProductionPrefabPath}");
            }

            if (FindPrefabInstanceRoot(ProductionPrefabPath) == null)
            {
                PrefabUtility.InstantiatePrefab(productionPrefab, scene);
            }

            if (includeTestControls)
            {
                BuildTestControlsPrefab();
                GameObject controlsPrefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(TestControlsPrefabPath);
                PrefabUtility.InstantiatePrefab(controlsPrefab, scene);
            }

            EnsureEventSystem(scene);

            NotebookController controller =
                UnityEngine.Object.FindFirstObjectByType<NotebookController>(FindObjectsInactive.Include);
            NotebookUIShellReferences shell = NotebookSceneLookup.FindShell();
            NotebookOpenButton openButton = NotebookSceneLookup.FindOpenButton();
            NotebookTestHarness harness =
                UnityEngine.Object.FindFirstObjectByType<NotebookTestHarness>(FindObjectsInactive.Include);

            shell?.ApplyToController(controller);
            openButton?.Configure(controller);
            harness?.Configure(controller, shell);

            EditorSceneManager.MarkSceneDirty(scene);
        }

        public static void RemoveLegacyNotebookObjects()
        {
            var destroy = new HashSet<GameObject>();
            AddComponentHosts<NotebookTestSceneBootstrap>(destroy);
            AddComponentHosts<NotebookPlaytestBar>(destroy);
            AddComponentHosts<NotebookTestHarness>(destroy);
            AddComponentHosts<NotebookOpenButton>(destroy);
            AddComponentHosts<NotebookOverlayView>(destroy);

            foreach (NotebookUIShellReferences shell in
                     UnityEngine.Object.FindObjectsByType<NotebookUIShellReferences>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                Canvas canvas = shell.GetComponentInParent<Canvas>(true);
                GameObject root = canvas != null
                    && canvas.name.IndexOf("Notebook", StringComparison.OrdinalIgnoreCase) >= 0
                        ? canvas.gameObject
                        : shell.gameObject;
                if (!IsInstanceOf(root, ProductionPrefabPath))
                {
                    destroy.Add(root);
                }
            }

            foreach (GameObject target in destroy)
            {
                if (target != null && !IsInstanceOf(target, ProductionPrefabPath))
                {
                    UnityEngine.Object.DestroyImmediate(target);
                }
            }
        }

        private static void AddComponentHosts<T>(ISet<GameObject> targets) where T : Component
        {
            foreach (T component in UnityEngine.Object.FindObjectsByType<T>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                targets.Add(component.gameObject);
            }
        }

        private static bool IsInstanceOf(GameObject target, string prefabPath)
        {
            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(target);
            return root != null
                && PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root) == prefabPath;
        }

        private static GameObject FindPrefabInstanceRoot(string prefabPath)
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (IsInstanceOf(root, prefabPath))
                {
                    return PrefabUtility.GetNearestPrefabInstanceRoot(root);
                }
            }

            return null;
        }

        private static void EnsureEventSystem(Scene scene)
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
        }

        private static void BuildTestControlsPrefab()
        {
            GameObject root = new GameObject(
                "Notebook Test Scene Controls",
                typeof(NotebookTestHarness));
            GameObject canvasRoot = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasRoot.transform.SetParent(root.transform, false);

            try
            {
                ConfigureCanvas(canvasRoot);
                NotebookTestHarness harness = root.GetComponent<NotebookTestHarness>();

                RectTransform dock = CreatePanel(canvasRoot.transform, "Test Tools Dock");
                SetRect(dock, new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-18f, -18f), new Vector2(380f, 0f), new Vector2(1f, 1f));
                VerticalLayoutGroup dockLayout = dock.gameObject.AddComponent<VerticalLayoutGroup>();
                dockLayout.spacing = 4f;
                dockLayout.padding = new RectOffset(6, 6, 6, 6);
                dockLayout.childControlHeight = true;
                dockLayout.childControlWidth = true;
                dockLayout.childForceExpandHeight = false;
                ContentSizeFitter dockFitter = dock.gameObject.AddComponent<ContentSizeFitter>();
                dockFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                Button toggle = CreateButton(dock, "Toggle", "Show notebook test tools");
                GameObject panel = CreatePanel(dock, "Panel").gameObject;
                VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 6f;
                layout.padding = new RectOffset(8, 8, 8, 8);
                layout.childControlHeight = true;
                layout.childControlWidth = true;
                layout.childForceExpandHeight = false;
                ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                TMP_Text status = CreateText(panel.transform, "Status", "Notebook test status");
                status.gameObject.AddComponent<LayoutElement>().preferredHeight = 52f;

                Button runCheck = CreateButton(panel.transform, "Run Content Check", "Run content check");
                Button collectAll = CreateButton(panel.transform, "Collect All", "Collect all gameplay notes");
                Button kindnessPlus = CreateButton(panel.transform, "Kindness Plus", "Kindness +1");
                Button kindnessMinus = CreateButton(panel.transform, "Kindness Minus", "Kindness -1");
                Button save = CreateButton(panel.transform, "Save", "Save game");
                Button load = CreateButton(panel.transform, "Load", "Load game");
                Button toggleFlag = CreateButton(panel.transform, "Toggle Flag", "Toggle test_flag");
                Button logStats = CreateButton(panel.transform, "Log Stats", "Log all stats");
                Button clear = CreateButton(panel.transform, "Clear Notebook", "Clear notebook save");

                var sceneButtons = new List<Button>();
                foreach ((string label, string _) in SceneLinks)
                {
                    sceneButtons.Add(CreateButton(panel.transform, label, label));
                }

                panel.SetActive(false);
                AssignHarnessReferences(
                    harness,
                    toggle,
                    toggle.GetComponentInChildren<TMP_Text>(true),
                    panel,
                    status,
                    runCheck,
                    collectAll,
                    kindnessPlus,
                    kindnessMinus,
                    save,
                    load,
                    toggleFlag,
                    logStats,
                    clear,
                    sceneButtons);

                PrefabUtility.SaveAsPrefabAsset(root, TestControlsPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AssignHarnessReferences(
            NotebookTestHarness harness,
            Button toggle,
            TMP_Text toggleLabel,
            GameObject panel,
            TMP_Text status,
            Button runCheck,
            Button collectAll,
            Button kindnessPlus,
            Button kindnessMinus,
            Button save,
            Button load,
            Button toggleFlag,
            Button logStats,
            Button clear,
            IReadOnlyList<Button> sceneButtons)
        {
            SerializedObject serialized = new SerializedObject(harness);
            Set(serialized, "toggleButton", toggle);
            Set(serialized, "toggleLabel", toggleLabel);
            Set(serialized, "panel", panel);
            Set(serialized, "statusText", status);
            Set(serialized, "runContentCheckButton", runCheck);
            Set(serialized, "collectAllButton", collectAll);
            Set(serialized, "kindnessPlusButton", kindnessPlus);
            Set(serialized, "kindnessMinusButton", kindnessMinus);
            Set(serialized, "saveButton", save);
            Set(serialized, "loadButton", load);
            Set(serialized, "toggleFlagButton", toggleFlag);
            Set(serialized, "logStatsButton", logStats);
            Set(serialized, "clearNotebookButton", clear);

            SerializedProperty links = serialized.FindProperty("sceneButtons");
            links.arraySize = SceneLinks.Length;
            for (int i = 0; i < SceneLinks.Length; i++)
            {
                SerializedProperty link = links.GetArrayElementAtIndex(i);
                link.FindPropertyRelative("button").objectReferenceValue = sceneButtons[i];
                link.FindPropertyRelative("sceneName").stringValue = SceneLinks[i].sceneName;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(SerializedObject target, string propertyName, UnityEngine.Object value)
        {
            target.FindProperty(propertyName).objectReferenceValue = value;
        }

        private static void ConfigureCanvas(GameObject root)
        {
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static RectTransform CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = new Color(0.09f, 0.07f, 0.05f, 0.92f);
            return panel.GetComponent<RectTransform>();
        }

        private static Button CreateButton(Transform parent, string name, string label)
        {
            GameObject buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = new Color(0.42f, 0.31f, 0.21f, 0.96f);
            buttonObject.GetComponent<LayoutElement>().preferredHeight = 34f;

            TMP_Text text = CreateText(buttonObject.transform, "Label", label);
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            SetStretch(text.rectTransform, 8f, 4f);
            return buttonObject.GetComponent<Button>();
        }

        private static TMP_Text CreateText(Transform parent, string name, string value)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.text = value;
            text.fontSize = 14f;
            text.color = new Color(0.95f, 0.91f, 0.82f, 1f);
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private static void SetStretch(RectTransform rect, float horizontal, float vertical)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(horizontal, vertical);
            rect.offsetMax = new Vector2(-horizontal, -vertical);
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 position,
            Vector2 size,
            Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
