#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Peaceland.Notebook.EditableScenePack;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Peaceland.Notebook.Editor
{
    public static class NotebookOpenUIAuthoring
    {
        private const string CanvasName = "Notebook Canvas";
        private const string OpenRootName = "Notebook Open Root";
        private const string HudButtonName = "Notebook HUD Button";
        private const string NotebookArtOpened = "Assets/Notebook/notebook-art/notebookOpened.png";
        private const string HomeScenePath = "Assets/Notebook/Scenes/NoteBookTesting.unity";

        [MenuItem("Peaceland/Notebook/Author Open UI In Active Scene")]
        public static void AuthorInActiveSceneMenu()
        {
            AuthorInActiveScene();
        }

        [MenuItem("Peaceland/Notebook/Wire Controller To Shell")]
        public static void WireControllerMenu()
        {
            WireControllerToShell();
        }

        [MenuItem("Peaceland/Notebook/Lock UI Layout For Hand Editing")]
        public static void LockUILayoutForHandEditing()
        {
            SetLayoutLockOnActiveScene(true);
        }

        [MenuItem("Peaceland/Notebook/Unlock UI Layout (Allow Auto Repair)")]
        public static void UnlockUILayoutForAutoRepair()
        {
            SetLayoutLockOnActiveScene(false);
        }

        private static void SetLayoutLockOnActiveScene(bool locked)
        {
            NotebookBookArtLayout[] layouts = Object.FindObjectsByType<NotebookBookArtLayout>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (layouts.Length == 0)
            {
                Debug.LogWarning("No NotebookBookArtLayout found. Select Notebook Open Root or run Author Open UI first.");
                return;
            }

            foreach (NotebookBookArtLayout layout in layouts)
            {
                Undo.RecordObject(layout, locked ? "Lock Notebook UI Layout" : "Unlock Notebook UI Layout");
                layout.SetLockLayout(locked);
                EditorUtility.SetDirty(layout);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(locked
                ? "Notebook UI layout locked — Scene-view moves will stick in Edit mode."
                : "Notebook UI layout unlocked — authoring menus may reset rects again.");
        }

        [MenuItem("Peaceland/Notebook/Cleanup Legacy Test UI")]
        public static void CleanupLegacyMenu()
        {
            CleanupLegacyTestUi();
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        private static void RemoveMisplacedHomeCollectibles()
        {
            if (!EditorSceneManager.GetActiveScene().path.Replace('\\', '/').EndsWith(HomeScenePath))
            {
                return;
            }

            GameObject floristRoot = GameObject.Find("Florist Collectibles");
            if (floristRoot != null)
            {
                Object.DestroyImmediate(floristRoot);
            }
        }

        public static void CleanupLegacyTestUi()
        {
            string[] legacyRootNames =
            {
                "Notebook Test UI",
                "Notebook Test Navigator",
            };

            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            for (int c = 0; c < canvases.Length; c++)
            {
                Transform canvasTransform = canvases[c].transform;
                for (int i = 0; i < legacyRootNames.Length; i++)
                {
                    Transform legacy = canvasTransform.Find(legacyRootNames[i]);
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

            Transform openRoot = GameObject.Find(OpenRootName) != null
                ? GameObject.Find(OpenRootName).transform
                : null;
            if (openRoot != null)
            {
                Transform header = openRoot.Find("Header");
                if (header != null)
                {
                    Object.DestroyImmediate(header.gameObject);
                }

                Transform sectionRoot = openRoot.Find("Content Root/Section Page Root");
                Transform sectionHeader = sectionRoot != null ? sectionRoot.Find("Section Header") : null;
                if (sectionHeader != null)
                {
                    Object.DestroyImmediate(sectionHeader.gameObject);
                }
            }
        }

        public static void AuthorInActiveScene()
        {
            CleanupLegacyTestUi();
            RemoveMisplacedHomeCollectibles();
            NotebookScenePlayabilityEditor.EnsurePlayableActiveScene();
            Canvas canvas = EnsureCanvas();
            NotebookController controller = Object.FindFirstObjectByType<NotebookController>();
            if (controller == null)
            {
                GameObject controllerObject = new GameObject("Notebook Test Bootstrap", typeof(NotebookController), typeof(NotebookTestSceneBootstrap));
                controller = controllerObject.GetComponent<NotebookController>();
            }
            else if (controller.GetComponent<NotebookTestSceneBootstrap>() == null)
            {
                controller.gameObject.AddComponent<NotebookTestSceneBootstrap>();
            }

            if (controller.gameObject.name == "Notebook System")
            {
                controller.gameObject.name = "Notebook Test Bootstrap";
            }

            Transform openRootTransform = canvas.transform.Find(OpenRootName);
            GameObject openRootObject = openRootTransform != null
                ? openRootTransform.gameObject
                : new GameObject(OpenRootName, typeof(RectTransform), typeof(NotebookUIShellReferences), typeof(NotebookUIScaleDriver));

            openRootObject.transform.SetParent(canvas.transform, false);
            RectTransform openRoot = openRootObject.GetComponent<RectTransform>();

            NotebookBookArtLayout artLayout = openRootObject.GetComponent<NotebookBookArtLayout>();
            if (artLayout == null)
            {
                artLayout = openRootObject.AddComponent<NotebookBookArtLayout>();
            }

            if (!artLayout.LockLayout)
            {
                NotebookBookShellLayout.ApplyOpenRoot(openRoot);
            }

            Image rootImage = openRootObject.GetComponent<Image>();
            if (rootImage != null)
            {
                Object.DestroyImmediate(rootImage);
            }

            Image bookImage = EnsureBookBackground(openRoot, artLayout);

            NotebookUIShellReferences shell = openRootObject.GetComponent<NotebookUIShellReferences>();
            NotebookUIScaleDriver scaleDriver = openRootObject.GetComponent<NotebookUIScaleDriver>();
            if (scaleDriver != null)
            {
                SerializedObject scaleObject = new SerializedObject(scaleDriver);
                scaleObject.FindProperty("fitOpenRootToCanvas").boolValue = false;
                scaleObject.ApplyModifiedPropertiesWithoutUndo();
            }

            Transform legacyRail = openRoot.Find("Bookmark Tabs");
            if (legacyRail != null && legacyRail.GetComponent<NotebookBookmarkTabBar>() != null)
            {
                Object.DestroyImmediate(legacyRail.gameObject);
            }

            Transform pagesViewport = EnsureChild(openRoot, "Content Root");
            if (!artLayout.LockLayout)
            {
                NotebookBookShellLayout.ApplyPagesViewport(pagesViewport as RectTransform);
            }

            Transform directoryRoot = EnsureChild(pagesViewport, "Directory Page Root");
            Stretch(directoryRoot as RectTransform);
            Transform directoryLeft = EnsureChild(directoryRoot, "Left Page");
            NotebookBookShellLayout.ApplySpreadPage(directoryLeft as RectTransform, true);
            Transform directoryRight = EnsureChild(directoryRoot, "Right Page");
            NotebookBookShellLayout.ApplySpreadPage(directoryRight as RectTransform, false);
            EnsureText(directoryLeft, "Directory Title", "Directory", 28f, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -44f), new Vector2(-12f, -8f));

            Transform linesRoot = EnsureChild(directoryLeft, "Directory Lines");
            SetupVerticalLayout(linesRoot.gameObject, 14f, new RectOffset(8, 8, 56, 56));
            Stretch(linesRoot as RectTransform);
            ReconcileNamedChildren(linesRoot, "Present Line", "Memory 1 Line", "Memory 2 Line");

            List<NotebookDirectoryLineView> lines = new List<NotebookDirectoryLineView>
            {
                EnsureDirectoryLine(linesRoot, "Present Line", NotebookSection.Present, "Present"),
                EnsureDirectoryLine(linesRoot, "Memory 1 Line", NotebookSection.Memory1, "Memory 1 (Florist)"),
                EnsureDirectoryLine(linesRoot, "Memory 2 Line", NotebookSection.Memory2, "Memory 2 (R&J)"),
                EnsureDirectoryLine(linesRoot, "Hidden Stats Line", NotebookSection.HiddenStats, "Hidden Stats"),
            };

            Transform testToolsAnchor = EnsureChild(directoryLeft, "Test Tools Anchor");
            SetAnchored(testToolsAnchor as RectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 8f), new Vector2(-8f, 44f));

            Transform sectionRoot = EnsureChild(pagesViewport, "Section Page Root");
            Stretch(sectionRoot as RectTransform);
            Transform entriesArea = EnsureChild(sectionRoot, "Entries Area");
            Stretch(entriesArea as RectTransform);
            Image entriesBg = entriesArea.GetComponent<Image>();
            if (entriesBg == null)
            {
                entriesBg = entriesArea.gameObject.AddComponent<Image>();
            }

            entriesBg.color = new Color(1f, 1f, 1f, 0f);
            entriesBg.raycastTarget = false;

            Transform indexRoot = EnsureChild(entriesArea, "Index Root");
            Stretch(indexRoot as RectTransform);
            Transform indexLeft = EnsureChild(indexRoot, "Left Page");
            NotebookBookShellLayout.ApplySpreadPage(indexLeft as RectTransform, true);
            EnsurePageClipMask(indexLeft);
            Transform indexLeftContainer = EnsureChild(indexLeft, "Container");
            SetupVerticalLayout(indexLeftContainer.gameObject, 10f, new RectOffset(8, 8, 8, 8));
            Stretch(indexLeftContainer as RectTransform);
            Transform indexRight = EnsureChild(indexRoot, "Right Page");
            NotebookBookShellLayout.ApplySpreadPage(indexRight as RectTransform, false);
            EnsurePageClipMask(indexRight);
            Transform indexRightContainer = EnsureChild(indexRight, "Container");
            SetupVerticalLayout(indexRightContainer.gameObject, 10f, new RectOffset(8, 8, 8, 8));
            Stretch(indexRightContainer as RectTransform);
            NotebookIndexEntryView indexTemplate = EnsureIndexTemplate(indexLeftContainer);

            Transform contentPagesRoot = EnsureChild(entriesArea, "Content Root");
            Stretch(contentPagesRoot as RectTransform);
            Transform contentLeft = EnsureChild(contentPagesRoot, "Left Page");
            NotebookBookShellLayout.ApplySpreadPage(contentLeft as RectTransform, true);
            EnsurePageClipMask(contentLeft);
            Transform contentLeftContainer = EnsureChild(contentLeft, "Container");
            SetupVerticalLayout(contentLeftContainer.gameObject, 14f, new RectOffset(8, 8, 8, 8));
            Stretch(contentLeftContainer as RectTransform);
            Transform contentRight = EnsureChild(contentPagesRoot, "Right Page");
            NotebookBookShellLayout.ApplySpreadPage(contentRight as RectTransform, false);
            EnsurePageClipMask(contentRight);
            Transform contentRightContainer = EnsureChild(contentRight, "Container");
            SetupVerticalLayout(contentRightContainer.gameObject, 14f, new RectOffset(8, 8, 8, 8));
            Stretch(contentRightContainer as RectTransform);
            NotebookEntryView entryTemplate = EnsureEntryTemplate(contentLeftContainer);

            TMP_Text leftPageNumber = EnsureText(pagesViewport, "Left Page Number", "1", 15f, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(16f, 6f), new Vector2(-8f, 26f));
            leftPageNumber.color = new Color(0.19f, 0.15f, 0.11f, 0.7f);
            TMP_Text rightPageNumber = EnsureText(pagesViewport, "Right Page Number", "2", 15f, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(8f, 6f), new Vector2(-16f, 26f));
            rightPageNumber.color = leftPageNumber.color;

            RemoveLegacyPageButtons(openRoot);
            NotebookPageTurnHitLayer pageTurnLayer = EnsurePageTurnLayer(openRoot, out Button previousButton, out Button nextButton);

            Transform animationRoot = EnsureChild(openRoot, "Animation Root");
            NotebookBookShellLayout.ApplyAnimationOverlay(animationRoot as RectTransform);
            Image animationImage = animationRoot.GetComponent<Image>();
            if (animationImage == null)
            {
                animationImage = animationRoot.gameObject.AddComponent<Image>();
            }

            animationImage.color = Color.white;
            animationImage.preserveAspect = true;
            animationImage.raycastTarget = false;
            animationRoot.gameObject.SetActive(false);
            NotebookAnimationView animationView = animationRoot.GetComponent<NotebookAnimationView>();
            if (animationView == null)
            {
                animationView = animationRoot.gameObject.AddComponent<NotebookAnimationView>();
            }

            NotebookEditorArtUtility.WireOpenAnimation(animationView, animationImage);
            NotebookArtSpriteCropper.CropAllInFolder();

            Transform leftRail = EnsureChild(openRoot, "Bookmark Tabs Left");
            if (!artLayout.LockLayout)
            {
                NotebookBookShellLayout.ApplyBookmarkRailLeft(leftRail as RectTransform);
            }

            Transform rightRail = EnsureChild(openRoot, "Bookmark Tabs Right");
            if (!artLayout.LockLayout)
            {
                NotebookBookShellLayout.ApplyBookmarkRailRight(rightRail as RectTransform);
            }

            ReconcileBookmarkTabs(openRoot, leftRail, rightRail);

            Transform tabBarRoot = EnsureChild(openRoot, "Bookmark Tab Bar");
            NotebookBookmarkTabBar tabBar = tabBarRoot.GetComponent<NotebookBookmarkTabBar>();
            if (tabBar == null)
            {
                tabBar = tabBarRoot.gameObject.AddComponent<NotebookBookmarkTabBar>();
            }

            List<NotebookBookmarkTabView> tabs = new List<NotebookBookmarkTabView>
            {
                EnsureBookmarkTab(leftRail, "Directory Tab", NotebookSection.Directory, "Dir", 0, NotebookBookmarkSide.Left, artLayout),
                EnsureBookmarkTab(rightRail, "Present Tab", NotebookSection.Present, "Present", 1, NotebookBookmarkSide.Right, artLayout),
                EnsureBookmarkTab(rightRail, "Memory 1 Tab", NotebookSection.Memory1, "Mem 1", 2, NotebookBookmarkSide.Right, artLayout),
                EnsureBookmarkTab(rightRail, "Memory 2 Tab", NotebookSection.Memory2, "Mem 2", 3, NotebookBookmarkSide.Right, artLayout),
                EnsureBookmarkTab(rightRail, "Hidden Stats Tab", NotebookSection.HiddenStats, "Stats", 4, NotebookBookmarkSide.Right, artLayout),
            };
            tabBar.Configure(controller, tabs, leftRail as RectTransform, rightRail as RectTransform);

            bookImage.transform.SetAsFirstSibling();
            pageTurnLayer.transform.SetSiblingIndex(Mathf.Max(0, leftRail.GetSiblingIndex()));
            leftRail.SetAsLastSibling();
            rightRail.SetAsLastSibling();

            NotebookOverlayView overlay = NotebookOverlayAuthoring.EnsureOverlay(canvas.transform);

            NotebookCollectHintHost hintHost = Object.FindFirstObjectByType<NotebookCollectHintHost>();
            if (hintHost == null)
            {
                GameObject hostObject = new GameObject("Notebook Collect Hint Host", typeof(NotebookCollectHintHost));
                hintHost = hostObject.GetComponent<NotebookCollectHintHost>();
            }

            SerializedObject hintHostObject = new SerializedObject(hintHost);
            hintHostObject.FindProperty("overlayView").objectReferenceValue = overlay;
            hintHostObject.ApplyModifiedPropertiesWithoutUndo();

            Transform hudButton = canvas.transform.Find(HudButtonName);
            if (hudButton == null)
            {
                hudButton = CreateHudButton(canvas.transform);
            }

            SerializedObject shellObject = new SerializedObject(shell);
            shellObject.FindProperty("openRoot").objectReferenceValue = openRoot;
            shellObject.FindProperty("bookBackground").objectReferenceValue = bookImage;
            shellObject.FindProperty("pagesViewport").objectReferenceValue = pagesViewport;
            shellObject.FindProperty("animationView").objectReferenceValue = animationView;
            shellObject.FindProperty("bookmarkTabBar").objectReferenceValue = tabBar;
            shellObject.FindProperty("directoryPageRoot").objectReferenceValue = directoryRoot.gameObject;
            shellObject.FindProperty("sectionPageRoot").objectReferenceValue = sectionRoot.gameObject;
            shellObject.FindProperty("testToolsAnchor").objectReferenceValue = testToolsAnchor;
            shellObject.FindProperty("indexRoot").objectReferenceValue = indexRoot.gameObject;
            shellObject.FindProperty("indexLeftContainer").objectReferenceValue = indexLeftContainer;
            shellObject.FindProperty("indexRightContainer").objectReferenceValue = indexRightContainer;
            shellObject.FindProperty("indexEntryTemplate").objectReferenceValue = indexTemplate;
            shellObject.FindProperty("contentPagesRoot").objectReferenceValue = contentPagesRoot.gameObject;
            shellObject.FindProperty("contentLeftContainer").objectReferenceValue = contentLeftContainer;
            shellObject.FindProperty("contentRightContainer").objectReferenceValue = contentRightContainer;
            shellObject.FindProperty("entryTemplate").objectReferenceValue = entryTemplate;
            shellObject.FindProperty("leftPageNumberText").objectReferenceValue = leftPageNumber;
            shellObject.FindProperty("rightPageNumberText").objectReferenceValue = rightPageNumber;
            shellObject.FindProperty("previousSpreadButton").objectReferenceValue = previousButton;
            shellObject.FindProperty("nextSpreadButton").objectReferenceValue = nextButton;
            shellObject.FindProperty("overlayView").objectReferenceValue = overlay;

            SerializedProperty linesProperty = shellObject.FindProperty("directoryLines");
            linesProperty.arraySize = lines.Count;
            for (int i = 0; i < lines.Count; i++)
            {
                linesProperty.GetArrayElementAtIndex(i).objectReferenceValue = lines[i];
            }

            shellObject.ApplyModifiedPropertiesWithoutUndo();

            WireControllerToShell();

            NotebookTestSceneBootstrap bootstrap = controller.GetComponent<NotebookTestSceneBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.EditorEnsureSetup();
            }

            openRootObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(openRootObject.scene);
            Debug.Log(
                "Notebook open UI authored. Tune NotebookBookArtLayout on Notebook Open Root, "
                + "then enable Lock Layout. See Assets/Notebook/NOTEBOOK_WORKFLOW.md");
        }

        public static void WireControllerToShell()
        {
            NotebookController controller = Object.FindFirstObjectByType<NotebookController>();
            NotebookUIShellReferences shell = NotebookSceneLookup.FindShell();
            if (controller == null || shell == null)
            {
                Debug.LogWarning("Notebook wire skipped: missing NotebookController or NotebookUIShellReferences.");
                return;
            }

            shell.ApplyToController(controller);

            Transform hud = null;
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                hud = canvas.transform.Find(HudButtonName);
            }

            if (hud == null)
            {
                NotebookOpenButton existingOpenButton = Object.FindFirstObjectByType<NotebookOpenButton>();
                if (existingOpenButton != null)
                {
                    hud = existingOpenButton.transform;
                }
            }
            if (hud != null)
            {
                NotebookOpenButton openButton = hud.GetComponent<NotebookOpenButton>();
                if (openButton == null)
                {
                    openButton = hud.gameObject.AddComponent<NotebookOpenButton>();
                }

                openButton.Configure(controller);

                Button hudButton = hud.GetComponent<Button>();
                if (hudButton != null)
                {
                    SerializedObject controllerObject = new SerializedObject(controller);
                    controllerObject.FindProperty("notebookButton").objectReferenceValue = hudButton;
                    controllerObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(shell);
        }

        private static Canvas EnsureCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                if (canvas.gameObject.name != CanvasName)
                {
                    canvas.gameObject.name = CanvasName;
                }

                return canvas;
            }

            GameObject canvasObject = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas created = canvasObject.GetComponent<Canvas>();
            created.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return created;
        }

        private static Transform CreateHudButton(Transform parent)
        {
            GameObject buttonObject = new GameObject(HudButtonName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(NotebookOpenButton));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(24f, 24f);
            rect.sizeDelta = new Vector2(148f, 52f);
            buttonObject.GetComponent<Image>().color = new Color(0.18f, 0.31f, 0.52f, 1f);
            TMP_Text label = EnsureText(buttonObject.transform, "Label", "Notebook", 20f, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f));
            label.color = Color.white;
            return buttonObject.transform;
        }

        private static NotebookBookmarkTabView EnsureBookmarkTab(
            Transform parent,
            string name,
            NotebookSection section,
            string label,
            int index,
            NotebookBookmarkSide side,
            NotebookBookArtLayout artLayout = null)
        {
            Transform tab = EnsureChild(parent, name);
            if (artLayout == null || !artLayout.LockLayout)
            {
                if (side == NotebookBookmarkSide.Left)
                {
                    NotebookBookShellLayout.ApplyBookmarkTabLeft(tab as RectTransform, index);
                }
                else
                {
                    NotebookBookShellLayout.ApplyBookmarkTabRight(tab as RectTransform, index);
                }
            }

            Image bg = tab.GetComponent<Image>();
            if (bg == null)
            {
                bg = tab.gameObject.AddComponent<Image>();
            }

            bg.color = new Color(0.93f, 0.86f, 0.74f, 1f);
            bg.raycastTarget = true;
            Button button = tab.GetComponent<Button>();
            if (button == null)
            {
                button = tab.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = bg;

            TMP_Text text = EnsureText(tab, "Label", label, 13f, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f));
            NotebookBookmarkTabView view = tab.GetComponent<NotebookBookmarkTabView>();
            if (view == null)
            {
                view = tab.gameObject.AddComponent<NotebookBookmarkTabView>();
            }

            view.Configure(section, label, button, text);
            view.SetSide(side);
            return view;
        }

        private static NotebookDirectoryLineView EnsureDirectoryLine(Transform parent, string name, NotebookSection section, string label)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                NotebookDirectoryLineView existingView = existing.GetComponent<NotebookDirectoryLineView>();
                if (existingView != null)
                {
                    TMP_Text existingLabel = existing.Find("Label")?.GetComponent<TMP_Text>();
                    TMP_Text existingBadge = existing.Find("New Count")?.GetComponent<TMP_Text>();
                    existingView.Configure(section, existingLabel, existingBadge, existing.GetComponent<Button>());
                    return existingView;
                }
            }

            GameObject lineObject = new GameObject(name, typeof(RectTransform), typeof(LayoutElement), typeof(Button), typeof(NotebookDirectoryLineView));
            lineObject.transform.SetParent(parent, false);
            lineObject.GetComponent<LayoutElement>().preferredHeight = 34f;
            Button button = lineObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.08f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.12f);
            button.colors = colors;

            TMP_Text title = EnsureText(lineObject.transform, "Label", label, 22f, FontStyles.Normal, TextAlignmentOptions.Left,
                Vector2.zero, Vector2.one, new Vector2(4f, 0f), new Vector2(-48f, 0f));
            TMP_Text badge = EnsureText(lineObject.transform, "New Count", string.Empty, 16f, FontStyles.Bold, TextAlignmentOptions.Right,
                Vector2.zero, Vector2.one, new Vector2(0f, 0f), new Vector2(-4f, 0f));

            NotebookDirectoryLineView view = lineObject.GetComponent<NotebookDirectoryLineView>();
            view.Configure(section, title, badge, button);
            return view;
        }

        private static NotebookIndexEntryView EnsureIndexTemplate(Transform parent)
        {
            Transform existing = parent.Find("Index Entry Template");
            if (existing != null)
            {
                return existing.GetComponent<NotebookIndexEntryView>();
            }

            GameObject root = new GameObject("Index Entry Template", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(Button), typeof(NotebookIndexEntryView));
            root.transform.SetParent(parent, false);
            root.GetComponent<LayoutElement>().preferredHeight = 34f;
            root.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
            TMP_Text title = EnsureText(root.transform, "Title", "Entry", 16f, FontStyles.Normal, TextAlignmentOptions.Left,
                Vector2.zero, Vector2.one, new Vector2(8f, 0f), new Vector2(-40f, 0f));
            TMP_Text page = EnsureText(root.transform, "Page Number", "1", 16f, FontStyles.Bold, TextAlignmentOptions.Right,
                Vector2.zero, Vector2.one, new Vector2(0f, 0f), new Vector2(-8f, 0f));
            NotebookIndexEntryView view = root.GetComponent<NotebookIndexEntryView>();
            view.Configure(root.GetComponent<Button>(), title, page, null, null);
            root.SetActive(false);
            return view;
        }

        private static NotebookEntryView EnsureEntryTemplate(Transform parent)
        {
            Transform existing = parent.Find("Entry Template");
            GameObject root = existing != null
                ? existing.gameObject
                : new GameObject("Entry Template", typeof(RectTransform), typeof(LayoutElement), typeof(Image));
            root.transform.SetParent(parent, false);
            root.GetComponent<LayoutElement>().preferredHeight = NotebookEntryCellLayout.DefaultPreferredHeight;
            root.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.06f);

            EnsureText(root.transform, "Title", "Title", 22f, FontStyles.Bold, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(10f, -36f), new Vector2(-10f, -6f));
            Transform bodyTransform = root.transform.Find("Body")
                ?? root.transform.Find("Content Row/Body");
            if (bodyTransform == null)
            {
                EnsureText(root.transform, "Body", "Body", 18f, FontStyles.Normal, TextAlignmentOptions.TopLeft,
                    new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(10f, 10f), new Vector2(-10f, -40f));
            }

            Transform imageRoot = root.transform.Find("Image Root")
                ?? root.transform.Find("Content Row/Image Root")
                ?? EnsureChild(root.transform, "Image Root");
            GetOrAddComponent<Image>(imageRoot.gameObject).color = new Color(0.9f, 0.85f, 0.76f, 1f);
            GetOrAddComponent<Image>(EnsureChild(imageRoot, "Image").gameObject);

            NotebookEntryCellLayout cellLayout = GetOrAddComponent<NotebookEntryCellLayout>(root);
            cellLayout.ApplyTo(root.transform as RectTransform);
            NotebookEntryView view = GetOrAddComponent<NotebookEntryView>(root);
            view.ConfigureFromHierarchy();
            root.SetActive(false);
            return view;
        }

        private static NotebookPageTurnHitLayer EnsurePageTurnLayer(Transform openRoot, out Button previousButton, out Button nextButton)
        {
            Transform layerRoot = EnsureChild(openRoot, "Page Turn Layer");
            NotebookPageTurnHitLayer hitLayer = layerRoot.GetComponent<NotebookPageTurnHitLayer>();
            if (hitLayer == null)
            {
                hitLayer = layerRoot.gameObject.AddComponent<NotebookPageTurnHitLayer>();
            }

            previousButton = EnsurePageTurnButton(layerRoot, "Previous Page Hit", "<", true);
            nextButton = EnsurePageTurnButton(layerRoot, "Next Page Hit", ">", false);
            hitLayer.Configure(previousButton, nextButton, NotebookBookShellLayout.DefaultPageTurnHitWidth);
            return hitLayer;
        }

        private static void RemoveLegacyPageButtons(Transform openRoot)
        {
            RemoveIfDirectChild(openRoot, "Previous Spread Button");
            RemoveIfDirectChild(openRoot, "Next Spread Button");
        }

        private static void RemoveIfDirectChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null && child.parent == parent)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void EnsurePageClipMask(Transform pageTransform)
        {
            if (pageTransform == null)
            {
                return;
            }

            if (pageTransform.GetComponent<RectMask2D>() == null)
            {
                pageTransform.gameObject.AddComponent<RectMask2D>();
            }
        }

        private static Button EnsurePageTurnButton(Transform parent, string name, string label, bool previous)
        {
            Transform root = EnsureChild(parent, name);
            NotebookBookShellLayout.ApplyPageTurnHitArea(root as RectTransform, previous);
            Image bg = root.GetComponent<Image>();
            if (bg == null)
            {
                bg = root.gameObject.AddComponent<Image>();
            }

            bg.color = new Color(1f, 1f, 1f, 0.04f);
            bg.raycastTarget = true;
            Button button = root.GetComponent<Button>();
            if (button == null)
            {
                button = root.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = bg;
            EnsureText(root, "Label", label, 30f, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static Button EnsurePageEdgeButton(Transform parent, string name, string label, bool previous)
        {
            return EnsurePageTurnButton(parent, name, label, previous);
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

        private static Image EnsureBookBackground(Transform openRoot, NotebookBookArtLayout artLayout)
        {
            Transform backgroundTransform = EnsureChild(openRoot, "Book Background");
            backgroundTransform.SetAsFirstSibling();
            if (artLayout == null || !artLayout.LockLayout)
            {
                Stretch(backgroundTransform as RectTransform);
            }

            Image bookImage = backgroundTransform.GetComponent<Image>();
            if (bookImage == null)
            {
                bookImage = backgroundTransform.gameObject.AddComponent<Image>();
            }

            bookImage.color = new Color(0.96f, 0.92f, 0.84f, 0.98f);
            bookImage.raycastTarget = artLayout != null && artLayout.BookBackgroundBlocksRaycasts;
            bookImage.preserveAspect = true;
            Sprite openedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(NotebookArtOpened);
            if (openedSprite != null)
            {
                bookImage.sprite = openedSprite;
            }

            return bookImage;
        }

        private static void ReconcileBookmarkTabs(Transform openRoot, Transform leftRail, Transform rightRail)
        {
            string[] allowedNames =
            {
                "Directory Tab",
                "Present Tab",
                "Memory 1 Tab",
                "Memory 2 Tab",
                "Hidden Stats Tab",
            };

            HashSet<string> seen = new HashSet<string>();
            NotebookBookmarkTabView[] views = openRoot.GetComponentsInChildren<NotebookBookmarkTabView>(true);
            for (int i = 0; i < views.Length; i++)
            {
                NotebookBookmarkTabView view = views[i];
                if (view == null)
                {
                    continue;
                }

                string name = view.gameObject.name;
                bool allowed = false;
                for (int n = 0; n < allowedNames.Length; n++)
                {
                    if (allowedNames[n] == name)
                    {
                        allowed = true;
                        break;
                    }
                }

                if (!allowed || seen.Contains(name))
                {
                    Object.DestroyImmediate(view.gameObject);
                    continue;
                }

                seen.Add(name);
                bool defaultOnLeft = view.Section == NotebookSection.Directory;
                view.transform.SetParent(defaultOnLeft ? leftRail : rightRail, false);
            }
        }

        private static void ReconcileNamedChildren(Transform parent, params string[] allowedNames)
        {
            HashSet<string> seen = new HashSet<string>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                bool isManagedLine = false;
                for (int n = 0; n < allowedNames.Length; n++)
                {
                    if (child.name == allowedNames[n])
                    {
                        isManagedLine = true;
                        break;
                    }
                }

                if (!isManagedLine)
                {
                    continue;
                }

                if (seen.Contains(child.name))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
                else
                {
                    seen.Add(child.name);
                }
            }
        }

        private static void Stretch(RectTransform rect)
        {
            if (rect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(rect))
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchored(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (rect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(rect))
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetupVerticalLayout(GameObject target, float spacing, RectOffset padding)
        {
            VerticalLayoutGroup layout = target.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = target.AddComponent<VerticalLayoutGroup>();
            }

            layout.spacing = spacing;
            layout.padding = padding;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static TMP_Text EnsureText(
            Transform parent,
            string name,
            string text,
            float size,
            FontStyles style,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            Transform existing = parent.Find(name);
            GameObject textObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            if (existing == null)
            {
                textObject.transform.SetParent(parent, false);
            }

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            TMP_Text tmp = textObject.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = new Color(0.19f, 0.15f, 0.11f, 1f);
            return tmp;
        }
    }
}
#endif
