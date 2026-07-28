using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Peaceland.Notebook.EditableScenePack;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Peaceland.Notebook
{
    [DefaultExecutionOrder(-1000)]
    public class NotebookTestSceneBootstrap : MonoBehaviour
    {
        private const string DatabasePath = "Assets/Notebook/Data/NotebookDatabase.asset";
        private const string DataFolderPath = "Assets/Notebook/Data";
        private const string NotebookArtFolderPath = "Assets/Notebook/notebook-art";
        private const string NotebookClosedArtPath = NotebookArtFolderPath + "/notebook.png";
        private const string NotebookOpenedArtPath = NotebookArtFolderPath + "/notebookOpened.png";
        private const string NotebookUiArtPath = NotebookArtFolderPath + "/notebookUi.png";

        private static readonly string[] NotebookOpenFramePaths =
        {
            NotebookClosedArtPath,
            NotebookArtFolderPath + "/notebookOpen_1.png",
            NotebookArtFolderPath + "/notebookOpen_2.png",
            NotebookArtFolderPath + "/notebookOpen_3.png",
            NotebookOpenedArtPath,
        };

        [SerializeField] private NotebookController notebookController;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private NotebookDatabase database;
        [SerializeField] private List<NotebookEntryDefinition> sampleEntries = new List<NotebookEntryDefinition>();

        private bool isBuilding;

        private void Reset()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            EnsureSetup();
        }

        private void OnEnable()
        {
            // Edit mode: do not auto-wire — it resets RectTransforms and blocks manual layout edits.
            // Use Peaceland/Notebook/Author Open UI or Prepare Playable Tonight to rebuild UI.
            if (!Application.isPlaying)
            {
                return;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Intentionally empty: deferred EnsureSetup here used to fight Scene-view edits.
        }
#endif

        /// <summary>Called from editor authoring menus only.</summary>
        public void EditorEnsureSetup()
        {
            EnsureSetup();
        }

        private void EnsureSetup()
        {
            if (isBuilding)
            {
                return;
            }

            isBuilding = true;

            try
            {
                EnsureAssets();
                EnsureEventSystem();
                WireNotebookScene();
            }
            finally
            {
                isBuilding = false;
            }
        }

        private void EnsureAssets()
        {
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder(DataFolderPath))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Notebook/Data"));
                AssetDatabase.Refresh();
            }

            database = AssetDatabase.LoadAssetAtPath<NotebookDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<NotebookDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            sampleEntries = new List<NotebookEntryDefinition>
            {
                EnsureSampleEntry("NotebookEntry_PresentNewspaper", NotebookSection.Present, "newspaper", "newspaper", "Newspaper Clipping", "The paper margin carries a short note that should enter the notebook the moment the player clicks it.", 10, 220f),
                EnsureSampleEntry("NotebookEntry_Memory1FloristNormal", NotebookSection.Memory1, "scene-collected", "scene collected", "Florist Greeting", "A notebook line about the florist's ordinary requests and the way conversations begin before flowers enter the frame.", 20, 240f),
                EnsureSampleEntry("NotebookEntry_Memory1FloristFlower", NotebookSection.Memory1, "minigame", "minigame", "Flower Request", "A notebook line unlocked after the florist flower flow, focused on the flowers themselves rather than the small talk around them.", 30, 260f),
                EnsureSampleEntry("NotebookEntry_Memory2RJBalcony", NotebookSection.Memory2, "scene-collected", "scene collected", "Balcony Fragment", "A memory fragment tied to R&J, grouped under the larger memory thread and preserved in notebook order.", 40, 220f),
                EnsureSampleEntry("NotebookEntry_Memory2RJLetter", NotebookSection.Memory2, "scene-collected", "scene collected", "Letter Draft", "A second R&J page that should sit in its theoretical notebook position once unlocked.", 50, 240f),
            };

            MergeSampleEntriesIntoDatabase(database, sampleEntries);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
#endif
        }

#if UNITY_EDITOR
        private static void MergeSampleEntriesIntoDatabase(
            NotebookDatabase database,
            List<NotebookEntryDefinition> sampleEntries)
        {
            if (database == null || sampleEntries == null)
            {
                return;
            }

            SerializedObject serializedDatabase = new SerializedObject(database);
            SerializedProperty entriesProperty = serializedDatabase.FindProperty("entries");
            HashSet<string> existingIds = new HashSet<string>();
            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                NotebookEntryDefinition existing = entriesProperty.GetArrayElementAtIndex(i).objectReferenceValue
                    as NotebookEntryDefinition;
                if (existing != null && !string.IsNullOrWhiteSpace(existing.EntryId))
                {
                    existingIds.Add(existing.EntryId);
                }
            }

            int writeIndex = entriesProperty.arraySize;
            for (int i = 0; i < sampleEntries.Count; i++)
            {
                NotebookEntryDefinition sample = sampleEntries[i];
                if (sample == null || string.IsNullOrWhiteSpace(sample.EntryId) || existingIds.Contains(sample.EntryId))
                {
                    continue;
                }

                entriesProperty.InsertArrayElementAtIndex(writeIndex);
                entriesProperty.GetArrayElementAtIndex(writeIndex).objectReferenceValue = sample;
                existingIds.Add(sample.EntryId);
                writeIndex++;
            }

            serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
        }
#endif

#if UNITY_EDITOR
        private NotebookEntryDefinition EnsureSampleEntry(
            string assetName,
            NotebookSection section,
            string categoryId,
            string categoryDisplayName,
            string title,
            string bodyText,
            int theoreticalOrder,
            float layoutHeight)
        {
            string assetPath = $"{DataFolderPath}/{assetName}.asset";
            NotebookEntryDefinition entry = AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(assetPath);
            if (entry == null)
            {
                entry = ScriptableObject.CreateInstance<NotebookEntryDefinition>();
                AssetDatabase.CreateAsset(entry, assetPath);
            }

            SerializedObject serializedEntry = new SerializedObject(entry);
            serializedEntry.FindProperty("entryId").stringValue = assetName;
            serializedEntry.FindProperty("section").enumValueIndex = (int)section;
            serializedEntry.FindProperty("groupId").stringValue = string.Empty;
            serializedEntry.FindProperty("groupDisplayName").stringValue = string.Empty;
            serializedEntry.FindProperty("groupSortOrder").intValue = 0;
            serializedEntry.FindProperty("subgroupId").stringValue = string.Empty;
            serializedEntry.FindProperty("subgroupDisplayName").stringValue = string.Empty;
            serializedEntry.FindProperty("subgroupSortOrder").intValue = 0;
            serializedEntry.FindProperty("categoryId").stringValue = categoryId;
            serializedEntry.FindProperty("categoryDisplayName").stringValue = categoryDisplayName;
            serializedEntry.FindProperty("categorySortOrder").intValue = GetCategorySortOrder(categoryId);
            serializedEntry.FindProperty("title").stringValue = title;
            serializedEntry.FindProperty("bodyText").stringValue = bodyText;
            serializedEntry.FindProperty("theoreticalOrder").intValue = theoreticalOrder;
            serializedEntry.FindProperty("layoutHeight").floatValue = layoutHeight;
            serializedEntry.FindProperty("sortOrder").intValue = theoreticalOrder;
            serializedEntry.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(entry);
            return entry;
        }

        private static int GetCategorySortOrder(string categoryId)
        {
            switch (categoryId)
            {
                case "newspaper":
                    return 10;
                case "minigame":
                    return 20;
                case "scene-collected":
                    return 30;
                default:
                    return 100;
            }
        }
#endif

        private void EnsureEventSystem()
        {
            EventSystem existingSystem = FindFirstObjectByType<EventSystem>();
            if (existingSystem != null)
            {
                EnsureInputModule(existingSystem.gameObject);
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(RectTransform), typeof(EventSystem));
            EnsureInputModule(eventSystemObject);
            eventSystemObject.transform.SetParent(transform.parent, false);
        }

        private void EnsureInputModule(GameObject eventSystemObject)
        {
            if (eventSystemObject == null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            RemoveAllComponentsIfPresent<StandaloneInputModule>(eventSystemObject);
            RemoveDuplicateComponents<InputSystemUIInputModule>(eventSystemObject);
            GetOrAddComponent<InputSystemUIInputModule>(eventSystemObject);
            RemoveDuplicateComponents<InputSystemUIInputModule>(eventSystemObject);
#else
            RemoveDuplicateComponents<StandaloneInputModule>(eventSystemObject);
            GetOrAddComponent<StandaloneInputModule>(eventSystemObject);
            RemoveDuplicateComponents<StandaloneInputModule>(eventSystemObject);
#endif
        }

        private void EnsureCanvas()
        {
            if (rootCanvas != null)
            {
                return;
            }

            rootCanvas = FindFirstObjectByType<Canvas>();
            if (rootCanvas != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("Notebook Test Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            rootCanvas = canvasObject.GetComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildNotebookScene()
        {
            notebookController = GetOrAddComponent<NotebookController>(gameObject);

            Transform existingSceneRoot = rootCanvas != null ? rootCanvas.transform.Find("Notebook Test UI") : null;
            if (existingSceneRoot != null)
            {
                BindExistingNotebookScene(existingSceneRoot);
                return;
            }

            Transform sceneRoot = EnsureChild(rootCanvas.transform, "Notebook Test UI");
            SetStretch(sceneRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform debugBar = EnsureChild(sceneRoot, "Debug Bar");
            SetupHorizontalLayout(debugBar.gameObject, 20f, new RectOffset(0, 0, 0, 0));
            SetAnchored(debugBar as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(0f, 64f));

            Button openButton = EnsureTextButton(debugBar, "Open Notebook Button", "Open Notebook", new Color(0.18f, 0.31f, 0.52f, 1f));
            Button clearButton = EnsureTextButton(debugBar, "Clear Save Button", "Clear Notebook Save", new Color(0.49f, 0.22f, 0.18f, 1f));
            RemoveChildIfPresent(debugBar, "Collect Entries Button");

            Transform notebookOpenRoot = EnsureChild(sceneRoot, "Notebook Open Root");
            Image notebookBackground = GetOrAddComponent<Image>(notebookOpenRoot.gameObject);
            notebookBackground.color = new Color(0.96f, 0.92f, 0.84f, 0.98f);
            ApplyNotebookBackground(notebookBackground);
            NotebookBookShellLayout.ApplyOpenRoot(notebookOpenRoot as RectTransform);

            Transform animationRoot = EnsureChild(notebookOpenRoot, "Animation Root");
            NotebookBookShellLayout.ApplyAnimationOverlay(animationRoot as RectTransform);
            Image animationImage = GetOrAddComponent<Image>(animationRoot.gameObject);
            animationImage.color = Color.white;
            animationImage.preserveAspect = true;
            animationImage.raycastTarget = false;
            animationRoot.gameObject.SetActive(false);
            NotebookAnimationView animationView = GetOrAddComponent<NotebookAnimationView>(animationRoot.gameObject);
            animationView.Configure(animationImage, LoadNotebookAnimationFrames());

            Transform contentRoot = EnsureChild(notebookOpenRoot, "Content Root");
            NotebookBookShellLayout.ApplyPagesViewport(contentRoot as RectTransform);

            NotebookOverlayView overlayView = EnsureOverlay(sceneRoot);
            RemoveLegacyPageButtons(notebookOpenRoot);
            EnsurePageTurnLayer(notebookOpenRoot, out Button previousSpreadButton, out Button nextSpreadButton);

            Transform directoryPageRoot = EnsureChild(contentRoot, "Directory Page Root");
            SetStretch(directoryPageRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ClearLayoutComponents(directoryPageRoot.gameObject);
            Transform directoryLeftPage = EnsureChild(directoryPageRoot, "Left Page");
            NotebookBookShellLayout.ApplySpreadPage(directoryLeftPage as RectTransform, true);
            Transform directoryRightPage = EnsureChild(directoryPageRoot, "Right Page");
            NotebookBookShellLayout.ApplySpreadPage(directoryRightPage as RectTransform, false);

            EnsureText(directoryLeftPage, "Directory Title", "Directory", 30f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetStretch((directoryLeftPage.Find("Directory Title") as RectTransform), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -52f), new Vector2(-18f, -12f));

            TMP_Text blankRightPageText = EnsureText(directoryRightPage, "Blank Page Label", string.Empty, 26f, FontStyles.Italic, TextAlignmentOptions.Center);
            blankRightPageText.color = new Color(0.42f, 0.35f, 0.27f, 0.28f);
            SetStretch(blankRightPageText.rectTransform, Vector2.zero, Vector2.one, new Vector2(18f, 18f), new Vector2(-18f, -18f));

            Transform directoryCellContainer = EnsureChild(directoryLeftPage, "Directory Cells");
            SetupVerticalLayout(directoryCellContainer.gameObject, 18f, new RectOffset(28, 28, 88, 24));
            LayoutElement directoryCellLayout = GetOrAddComponent<LayoutElement>(directoryCellContainer.gameObject);
            directoryCellLayout.flexibleHeight = 1f;
            RemoveChildIfPresent(directoryCellContainer, "Memory 1 (Florist) Cell");
            RemoveChildIfPresent(directoryCellContainer, "Memory 2 (R&J) Cell");

            List<NotebookDirectoryCellView> directoryCells = new List<NotebookDirectoryCellView>
            {
                EnsureDirectoryCell(directoryCellContainer, "Present Cell", NotebookSection.Present, "Present"),
                EnsureDirectoryCell(directoryCellContainer, "Memory 1 Cell", NotebookSection.Memory1, "Memory 1 (Florist)"),
                EnsureDirectoryCell(directoryCellContainer, "Memory 2 Cell", NotebookSection.Memory2, "Memory 2 (R&J)"),
                EnsureDirectoryCell(directoryCellContainer, "Hidden Stats Cell", NotebookSection.HiddenStats, "Hidden Stats"),
            };

            Transform sectionPageRoot = EnsureChild(contentRoot, "Section Page Root");
            SetStretch(sectionPageRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform sectionHeader = EnsureChild(sectionPageRoot, "Section Header");
            NotebookBookShellLayout.ApplySectionChrome(sectionHeader as RectTransform);
            Button backButton = EnsureTextButton(sectionHeader, "Back Button", "Back", new Color(0.42f, 0.31f, 0.21f, 0.95f));
            SetAnchored(backButton.transform as RectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(108f, 36f));
            TMP_Text sectionTitleText = EnsureText(sectionHeader, "Section Title", "Present", 22f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetStretch(sectionTitleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, -18f), new Vector2(180f, 18f));
            TMP_Text breadcrumbText = EnsureText(sectionHeader, "Breadcrumb", "Present", 14f, FontStyles.Italic, TextAlignmentOptions.Center);
            breadcrumbText.color = new Color(0.19f, 0.15f, 0.11f, 0.72f);
            SetStretch(breadcrumbText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-220f, 0f), new Vector2(220f, 16f));

            Transform entriesArea = EnsureChild(sectionPageRoot, "Entries Area");
            NotebookBookShellLayout.ApplySpreadBody(entriesArea as RectTransform);
            Image entriesBackground = GetOrAddComponent<Image>(entriesArea.gameObject);
            entriesBackground.color = new Color(1f, 1f, 1f, 0f);
            entriesBackground.raycastTarget = false;
            ApplyNotebookPanel(entriesBackground);

            Transform indexRoot = EnsureChild(entriesArea, "Index Root");
            SetStretch(indexRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Transform indexLeftPage = EnsureChild(indexRoot, "Left Page");
            NotebookBookShellLayout.ApplySpreadPage(indexLeftPage as RectTransform, true);
            Transform indexLeftContainer = EnsureChild(indexLeftPage, "Container");
            SetupVerticalLayout(indexLeftContainer.gameObject, 10f, new RectOffset(12, 12, 12, 12));
            SetStretch(indexLeftContainer as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform indexRightPage = EnsureChild(indexRoot, "Right Page");
            NotebookBookShellLayout.ApplySpreadPage(indexRightPage as RectTransform, false);
            Transform indexRightContainer = EnsureChild(indexRightPage, "Container");
            SetupVerticalLayout(indexRightContainer.gameObject, 10f, new RectOffset(12, 12, 12, 12));
            SetStretch(indexRightContainer as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            NotebookIndexEntryView indexEntryTemplate = EnsureIndexEntryTemplate(indexLeftContainer);

            Transform contentPagesRoot = EnsureChild(entriesArea, "Content Root");
            SetStretch(contentPagesRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Transform contentLeftPage = EnsureChild(contentPagesRoot, "Left Page");
            NotebookBookShellLayout.ApplySpreadPage(contentLeftPage as RectTransform, true);
            Transform contentLeftContainer = EnsureChild(contentLeftPage, "Container");
            SetupVerticalLayout(contentLeftContainer.gameObject, 18f, new RectOffset(12, 12, 12, 12));
            SetStretch(contentLeftContainer as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform contentRightPage = EnsureChild(contentPagesRoot, "Right Page");
            NotebookBookShellLayout.ApplySpreadPage(contentRightPage as RectTransform, false);
            Transform contentRightContainer = EnsureChild(contentRightPage, "Container");
            SetupVerticalLayout(contentRightContainer.gameObject, 18f, new RectOffset(12, 12, 12, 12));
            SetStretch(contentRightContainer as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            NotebookEntryView entryTemplate = EnsureEntryTemplate(contentLeftContainer);

            TMP_Text leftPageNumberText = EnsureText(contentRoot, "Left Page Number", "1", 16f, FontStyles.Bold, TextAlignmentOptions.Center);
            leftPageNumberText.color = new Color(0.19f, 0.15f, 0.11f, 0.72f);
            SetStretch(leftPageNumberText.rectTransform, new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(24f, 8f), new Vector2(-12f, 30f));
            TMP_Text rightPageNumberText = EnsureText(contentRoot, "Right Page Number", "2", 16f, FontStyles.Bold, TextAlignmentOptions.Center);
            rightPageNumberText.color = new Color(0.19f, 0.15f, 0.11f, 0.72f);
            SetStretch(rightPageNumberText.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(12f, 8f), new Vector2(-24f, 30f));

            notebookController.Configure(
                database,
                openButton,
                notebookOpenRoot.gameObject,
                directoryPageRoot.gameObject,
                sectionPageRoot.gameObject,
                animationView,
                overlayView,
                directoryCells,
                sectionTitleText,
                breadcrumbText,
                leftPageNumberText,
                rightPageNumberText,
                backButton,
                previousSpreadButton,
                nextSpreadButton,
                indexRoot.gameObject,
                indexLeftContainer,
                indexRightContainer,
                indexEntryTemplate,
                contentPagesRoot.gameObject,
                contentLeftContainer,
                contentRightContainer,
                entryTemplate);

            ApplyEditorPreviewState(notebookOpenRoot, directoryPageRoot, sectionPageRoot);

            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(notebookController.ClearSavedStateForDebug);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(notebookController);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif
        }

        private void BindExistingNotebookScene(Transform sceneRoot)
        {
            if (sceneRoot == null)
            {
                return;
            }

            Button openButton = sceneRoot.Find("Debug Bar/Open Notebook Button")?.GetComponent<Button>();
            Button clearButton = sceneRoot.Find("Debug Bar/Clear Save Button")?.GetComponent<Button>();

            Transform notebookOpenRoot = sceneRoot.Find("Notebook Open Root");
            bool preserveManualLayout = NotebookBookArtLayout.IsLocked(notebookOpenRoot);
            RepairNotebookOpenLayout(notebookOpenRoot, preserveManualLayout);

            Button previousSpreadButton = null;
            Button nextSpreadButton = null;
            if (notebookOpenRoot != null)
            {
                RemoveLegacyPageButtons(notebookOpenRoot);
                if (preserveManualLayout)
                {
                    NotebookPageTurnHitLayer hitLayer = notebookOpenRoot.Find("Page Turn Layer")?.GetComponent<NotebookPageTurnHitLayer>();
                    if (hitLayer != null)
                    {
                        previousSpreadButton = hitLayer.PreviousPageButton;
                        nextSpreadButton = hitLayer.NextPageButton;
                    }
                }
                else
                {
                    EnsurePageTurnLayer(notebookOpenRoot, out previousSpreadButton, out nextSpreadButton);
                }
            }
            Transform pagesViewport = notebookOpenRoot?.Find("Content Root");
            Transform directoryPageRoot = pagesViewport?.Find("Directory Page Root");
            if (!preserveManualLayout)
            {
                EnsureDirectorySpreadStructure(directoryPageRoot);
            }

            Transform sectionPageRoot = pagesViewport?.Find("Section Page Root");
            if (!preserveManualLayout)
            {
                EnsureSectionSpreadStructure(sectionPageRoot);
            }
            NotebookAnimationView animationView = notebookOpenRoot?.Find("Animation Root")?.GetComponent<NotebookAnimationView>();
            NotebookOverlayView overlayView = sceneRoot.Find("Notebook Overlay")?.GetComponent<NotebookOverlayView>();
            Image notebookBackground = notebookOpenRoot != null ? notebookOpenRoot.GetComponent<Image>() : null;
            Image animationImage = notebookOpenRoot?.Find("Animation Root")?.GetComponent<Image>();
            Image entriesBackground = notebookOpenRoot?.Find("Content Root/Section Page Root/Entries Area")?.GetComponent<Image>();

            List<NotebookDirectoryCellView> directoryCells = FindComponentsInDirectChildren<NotebookDirectoryCellView>(
                notebookOpenRoot?.Find("Content Root/Directory Page Root/Left Page/Directory Cells"));

            TMP_Text sectionTitleText = sectionPageRoot?.Find("Section Header/Section Title")?.GetComponent<TMP_Text>();
            TMP_Text breadcrumbText = sectionPageRoot?.Find("Section Header/Breadcrumb")?.GetComponent<TMP_Text>();
            TMP_Text leftPageNumberText = pagesViewport?.Find("Left Page Number")?.GetComponent<TMP_Text>()
                ?? sectionPageRoot?.Find("Section Header/Left Page Number")?.GetComponent<TMP_Text>();
            TMP_Text rightPageNumberText = pagesViewport?.Find("Right Page Number")?.GetComponent<TMP_Text>()
                ?? sectionPageRoot?.Find("Section Header/Right Page Number")?.GetComponent<TMP_Text>();
            if (leftPageNumberText == null && pagesViewport != null)
            {
                leftPageNumberText = EnsureText(pagesViewport, "Left Page Number", "1", 16f, FontStyles.Bold, TextAlignmentOptions.Center);
                leftPageNumberText.color = new Color(0.19f, 0.15f, 0.11f, 0.72f);
                SetStretch(leftPageNumberText.rectTransform, new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(24f, 8f), new Vector2(-12f, 30f));
            }

            if (rightPageNumberText == null && pagesViewport != null)
            {
                rightPageNumberText = EnsureText(pagesViewport, "Right Page Number", "2", 16f, FontStyles.Bold, TextAlignmentOptions.Center);
                rightPageNumberText.color = new Color(0.19f, 0.15f, 0.11f, 0.72f);
                SetStretch(rightPageNumberText.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(12f, 8f), new Vector2(-24f, 30f));
            }

            Button backButton = sectionPageRoot?.Find("Section Header/Back Button")?.GetComponent<Button>();
            Transform indexRoot = sectionPageRoot?.Find("Entries Area/Index Root");
            Transform indexLeftContainer = indexRoot?.Find("Left Page/Container");
            Transform indexRightContainer = indexRoot?.Find("Right Page/Container");
            Transform contentRoot = sectionPageRoot?.Find("Entries Area/Content Root");
            Transform contentLeftContainer = contentRoot?.Find("Left Page/Container");
            Transform contentRightContainer = contentRoot?.Find("Right Page/Container");
            NotebookIndexEntryView indexEntryTemplate = indexLeftContainer != null
                ? EnsureIndexEntryTemplate(indexLeftContainer)
                : null;
            NotebookEntryView entryTemplate = contentLeftContainer != null
                ? EnsureEntryTemplate(contentLeftContainer)
                : null;

            ApplyNotebookBackground(notebookBackground);
            ApplyNotebookPanel(entriesBackground);
            if (animationView != null && animationImage != null)
            {
                animationImage.preserveAspect = true;
                animationView.Configure(animationImage, LoadNotebookAnimationFrames());
            }

            notebookController.Configure(
                database,
                openButton,
                notebookOpenRoot != null ? notebookOpenRoot.gameObject : null,
                directoryPageRoot != null ? directoryPageRoot.gameObject : null,
                sectionPageRoot != null ? sectionPageRoot.gameObject : null,
                animationView,
                overlayView,
                directoryCells,
                sectionTitleText,
                breadcrumbText,
                leftPageNumberText,
                rightPageNumberText,
                backButton,
                previousSpreadButton,
                nextSpreadButton,
                indexRoot != null ? indexRoot.gameObject : null,
                indexLeftContainer,
                indexRightContainer,
                indexEntryTemplate,
                contentRoot != null ? contentRoot.gameObject : null,
                contentLeftContainer,
                contentRightContainer,
                entryTemplate);

            ApplyEditorPreviewState(notebookOpenRoot, directoryPageRoot, sectionPageRoot);

            if (clearButton != null)
            {
                clearButton.onClick.RemoveAllListeners();
                clearButton.onClick.AddListener(notebookController.ClearSavedStateForDebug);
            }
        }

        private static void ApplyEditorPreviewState(
            Transform notebookOpenRoot,
            Transform directoryPageRoot,
            Transform sectionPageRoot)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (notebookOpenRoot != null)
            {
                notebookOpenRoot.gameObject.SetActive(false);
            }

            if (directoryPageRoot != null)
            {
                directoryPageRoot.gameObject.SetActive(true);
            }

            if (sectionPageRoot != null)
            {
                sectionPageRoot.gameObject.SetActive(false);
            }
        }

        private NotebookDirectoryCellView EnsureDirectoryCell(Transform parent, string childName, NotebookSection section, string displayName)
        {
            Transform cellRoot = EnsureChild(parent, childName);
            Image background = GetOrAddComponent<Image>(cellRoot.gameObject);
            background.color = new Color(0.93f, 0.86f, 0.74f, 1f);

            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(cellRoot.gameObject);
            layoutElement.preferredHeight = 104f;

            Button button = GetOrAddComponent<Button>(cellRoot.gameObject);
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.97f, 0.88f, 1f);
            colors.pressedColor = new Color(0.87f, 0.77f, 0.63f, 1f);
            button.colors = colors;

            Transform highlightRoot = EnsureChild(cellRoot, "Highlight");
            Image highlightImage = GetOrAddComponent<Image>(highlightRoot.gameObject);
            highlightImage.color = new Color(0.96f, 0.74f, 0.31f, 0.6f);
            SetStretch(highlightRoot as RectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 6f), new Vector2(-6f, -6f));

            TMP_Text titleText = EnsureText(cellRoot, "Title", displayName, 28f, FontStyles.Bold, TextAlignmentOptions.Left);
            SetStretch(titleText.rectTransform, Vector2.zero, Vector2.one, new Vector2(40f, 0f), new Vector2(-200f, 0f));

            Transform badgeRoot = EnsureChild(cellRoot, "Badge");
            Image badgeBackground = GetOrAddComponent<Image>(badgeRoot.gameObject);
            badgeBackground.color = new Color(0.69f, 0.13f, 0.09f, 1f);
            SetAnchored(badgeRoot as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-48f, 0f), new Vector2(84f, 52f));

            TMP_Text countText = EnsureText(badgeRoot, "Count", string.Empty, 24f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetStretch(countText.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f));

            NotebookDirectoryCellView cellView = GetOrAddComponent<NotebookDirectoryCellView>(cellRoot.gameObject);
            cellView.Configure(section, button, titleText, countText, badgeRoot.gameObject, highlightRoot.gameObject);
            return cellView;
        }

        private void EnsureDirectorySpreadStructure(Transform directoryPageRoot)
        {
            if (directoryPageRoot == null)
            {
                return;
            }

            ClearLayoutComponents(directoryPageRoot.gameObject);
            SetStretch(directoryPageRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform leftPage = EnsureChild(directoryPageRoot, "Left Page");
            NotebookBookShellLayout.ApplySpreadPage(leftPage as RectTransform, true);
            Transform rightPage = EnsureChild(directoryPageRoot, "Right Page");
            NotebookBookShellLayout.ApplySpreadPage(rightPage as RectTransform, false);

            TMP_Text titleText = EnsureText(leftPage, "Directory Title", "Directory", 30f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetStretch(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -52f), new Vector2(-18f, -12f));

            TMP_Text blankPageText = EnsureText(rightPage, "Blank Page Label", string.Empty, 26f, FontStyles.Italic, TextAlignmentOptions.Center);
            blankPageText.color = new Color(0.42f, 0.35f, 0.27f, 0.28f);
            SetStretch(blankPageText.rectTransform, Vector2.zero, Vector2.one, new Vector2(18f, 18f), new Vector2(-18f, -18f));

            Transform directoryCells = EnsureChild(leftPage, "Directory Cells");
            SetupVerticalLayout(directoryCells.gameObject, 18f, new RectOffset(28, 28, 88, 24));
            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(directoryCells.gameObject);
            layoutElement.flexibleHeight = 1f;

            Transform legacyDirectoryCells = directoryPageRoot.Find("Directory Cells");
            if (legacyDirectoryCells != null && legacyDirectoryCells != directoryCells)
            {
                while (legacyDirectoryCells.childCount > 0)
                {
                    legacyDirectoryCells.GetChild(0).SetParent(directoryCells, false);
                }

                RemoveChildIfPresent(directoryPageRoot, "Directory Cells");
            }

            RemoveChildIfPresent(directoryPageRoot, "Directory Title");
        }

        private void EnsureSectionSpreadStructure(Transform sectionPageRoot)
        {
            if (sectionPageRoot == null)
            {
                return;
            }

            SetStretch(sectionPageRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform sectionHeader = EnsureChild(sectionPageRoot, "Section Header");
            NotebookBookShellLayout.ApplySectionChrome(sectionHeader as RectTransform);
            DeactivateLegacySectionHeader(sectionHeader);

            Transform entriesArea = EnsureChild(sectionPageRoot, "Entries Area");
            NotebookBookShellLayout.ApplySpreadBody(entriesArea as RectTransform);
            Image entriesBackground = GetOrAddComponent<Image>(entriesArea.gameObject);
            entriesBackground.color = new Color(1f, 1f, 1f, 0f);
            entriesBackground.raycastTarget = false;

            DeactivateChild(entriesArea, "Entries Root");
            DeactivateChild(entriesArea, "Folder List Root");

            Transform indexRoot = EnsureChild(entriesArea, "Index Root");
            SetStretch(indexRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Transform indexLeftPage = EnsureChild(indexRoot, "Left Page");
            NotebookBookShellLayout.ApplySpreadPage(indexLeftPage as RectTransform, true);
            Transform indexLeftContainer = EnsureChild(indexLeftPage, "Container");
            SetupVerticalLayout(indexLeftContainer.gameObject, 10f, new RectOffset(12, 12, 12, 12));
            SetStretch(indexLeftContainer as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform indexRightPage = EnsureChild(indexRoot, "Right Page");
            NotebookBookShellLayout.ApplySpreadPage(indexRightPage as RectTransform, false);
            Transform indexRightContainer = EnsureChild(indexRightPage, "Container");
            SetupVerticalLayout(indexRightContainer.gameObject, 10f, new RectOffset(12, 12, 12, 12));
            SetStretch(indexRightContainer as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform contentPagesRoot = EnsureChild(entriesArea, "Content Root");
            SetStretch(contentPagesRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Transform contentLeftPage = EnsureChild(contentPagesRoot, "Left Page");
            NotebookBookShellLayout.ApplySpreadPage(contentLeftPage as RectTransform, true);
            Transform contentLeftContainer = EnsureChild(contentLeftPage, "Container");
            SetupVerticalLayout(contentLeftContainer.gameObject, 18f, new RectOffset(12, 12, 12, 12));
            SetStretch(contentLeftContainer as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform contentRightPage = EnsureChild(contentPagesRoot, "Right Page");
            NotebookBookShellLayout.ApplySpreadPage(contentRightPage as RectTransform, false);
            Transform contentRightContainer = EnsureChild(contentRightPage, "Container");
            SetupVerticalLayout(contentRightContainer.gameObject, 18f, new RectOffset(12, 12, 12, 12));
            SetStretch(contentRightContainer as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private void RepairNotebookOpenLayout(Transform notebookOpenRoot, bool preserveManualLayout = false)
        {
            if (notebookOpenRoot == null)
            {
                return;
            }

            if (!preserveManualLayout)
            {
                NotebookBookShellLayout.ApplyOpenRoot(notebookOpenRoot as RectTransform);
            }

            ApplyNotebookBackground(notebookOpenRoot.GetComponent<Image>());

            Transform header = notebookOpenRoot.Find("Header");
            if (header != null)
            {
                header.gameObject.SetActive(false);
            }

            DeactivateChild(notebookOpenRoot, "Content Root/Section Page Root/Entries Area/Entries Root");
            DeactivateChild(notebookOpenRoot, "Content Root/Section Page Root/Entries Area/Folder List Root");
            DeactivateChild(notebookOpenRoot, "Content Root/Section Page Root/Section Header/Left Tabs");
            DeactivateChild(notebookOpenRoot, "Content Root/Section Page Root/Section Header/Right Tabs");

            Transform animationRoot = notebookOpenRoot.Find("Animation Root");
            if (animationRoot != null)
            {
                if (!preserveManualLayout)
                {
                    NotebookBookShellLayout.ApplyAnimationOverlay(animationRoot as RectTransform);
                }

                Image animationImage = animationRoot.GetComponent<Image>();
                if (animationImage != null)
                {
                    animationImage.raycastTarget = false;
                }

                animationRoot.gameObject.SetActive(false);
            }

            Transform pagesViewport = notebookOpenRoot.Find("Content Root");
            if (pagesViewport != null && !preserveManualLayout)
            {
                NotebookBookShellLayout.ApplyPagesViewport(pagesViewport as RectTransform);
            }
        }

        private static void DeactivateLegacySectionHeader(Transform sectionHeader)
        {
            if (sectionHeader == null)
            {
                return;
            }

            DeactivateChild(sectionHeader, "Left Tabs");
            DeactivateChild(sectionHeader, "Right Tabs");
            DeactivateChild(sectionHeader, "Folder List Root");
            DeactivateChild(sectionHeader, "Folder List Container");
        }

        private static void DeactivateChild(Transform parent, string childPath)
        {
            if (parent == null)
            {
                return;
            }

            Transform child = parent.Find(childPath);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        private NotebookPageTurnHitLayer EnsurePageTurnLayer(Transform openRoot, out Button previousButton, out Button nextButton)
        {
            Transform layerRoot = EnsureChild(openRoot, "Page Turn Layer");
            NotebookPageTurnHitLayer hitLayer = GetOrAddComponent<NotebookPageTurnHitLayer>(layerRoot.gameObject);
            previousButton = EnsurePageTurnButton(layerRoot, "Previous Page Hit", "<", true);
            nextButton = EnsurePageTurnButton(layerRoot, "Next Page Hit", ">", false);
            hitLayer.Configure(previousButton, nextButton, NotebookBookShellLayout.DefaultPageTurnHitWidth);
            return hitLayer;
        }

        private void RemoveLegacyPageButtons(Transform openRoot)
        {
            RemoveIfDirectChild(openRoot, "Previous Spread Button");
            RemoveIfDirectChild(openRoot, "Next Spread Button");
        }

        private static void RemoveIfDirectChild(Transform parent, string childName)
        {
            Transform child = parent != null ? parent.Find(childName) : null;
            if (child == null || child.parent != parent)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        private Button EnsurePageTurnButton(Transform parent, string name, string label, bool isPrevious)
        {
            Transform root = EnsureChild(parent, name);
            NotebookBookShellLayout.ApplyPageTurnHitArea(root as RectTransform, isPrevious);

            Image background = GetOrAddComponent<Image>(root.gameObject);
            background.color = new Color(1f, 1f, 1f, 0.04f);
            background.raycastTarget = true;

            Button button = GetOrAddComponent<Button>(root.gameObject);
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.94f);
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 0.94f);
            button.colors = colors;

            TMP_Text labelText = EnsureText(root, "Label", label, 36f, FontStyles.Bold, TextAlignmentOptions.Center);
            labelText.color = new Color(0.33f, 0.26f, 0.19f, 0.42f);
            SetStretch(labelText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private Button EnsurePageEdgeButton(
            Transform parent,
            string name,
            string label,
            bool isPrevious)
        {
            Transform root = EnsureChild(parent, name);
            NotebookBookShellLayout.ApplyPageTurnHitArea(root as RectTransform, isPrevious);

            Image background = GetOrAddComponent<Image>(root.gameObject);
            background.color = new Color(1f, 1f, 1f, 0.01f);

            Button button = GetOrAddComponent<Button>(root.gameObject);
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.94f);
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 0.94f);
            button.colors = colors;

            TMP_Text labelText = EnsureText(root, "Label", label, 36f, FontStyles.Bold, TextAlignmentOptions.Center);
            labelText.color = new Color(0.33f, 0.26f, 0.19f, 0.42f);
            SetStretch(labelText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private NotebookFolderButtonView EnsureFolderButtonTemplate(Transform parent)
        {
            Transform root = EnsureChild(parent, "Folder Button Template");
            Image background = GetOrAddComponent<Image>(root.gameObject);
            background.color = new Color(0.94f, 0.88f, 0.78f, 1f);

            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(root.gameObject);
            layoutElement.preferredHeight = 120f;

            Button button = GetOrAddComponent<Button>(root.gameObject);
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.98f, 0.9f, 1f);
            colors.pressedColor = new Color(0.88f, 0.8f, 0.68f, 1f);
            button.colors = colors;

            Transform highlightRoot = EnsureChild(root, "Highlight");
            Image highlightImage = GetOrAddComponent<Image>(highlightRoot.gameObject);
            highlightImage.color = new Color(0.96f, 0.74f, 0.31f, 0.4f);
            SetStretch(highlightRoot as RectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));

            TMP_Text titleText = EnsureText(root, "Title", "Folder", 28f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            SetStretch(titleText.rectTransform, new Vector2(0f, 0.4f), new Vector2(1f, 1f), new Vector2(28f, 0f), new Vector2(-140f, -16f));

            TMP_Text subtitleText = EnsureText(root, "Subtitle", "Select a note list", 18f, FontStyles.Normal, TextAlignmentOptions.BottomLeft);
            SetStretch(subtitleText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.48f), new Vector2(28f, 14f), new Vector2(-160f, 0f));

            Transform badgeRoot = EnsureChild(root, "Badge");
            Image badgeBackground = GetOrAddComponent<Image>(badgeRoot.gameObject);
            badgeBackground.color = new Color(0.69f, 0.13f, 0.09f, 1f);
            SetAnchored(badgeRoot as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-46f, 0f), new Vector2(84f, 52f));
            TMP_Text badgeText = EnsureText(badgeRoot, "Count", string.Empty, 24f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetStretch(badgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f));

            NotebookFolderButtonView view = GetOrAddComponent<NotebookFolderButtonView>(root.gameObject);
            view.Configure(button, titleText, subtitleText, badgeText, badgeRoot.gameObject, highlightRoot.gameObject);
            root.gameObject.SetActive(false);
            return view;
        }

        private NotebookTabButtonView EnsureTabButton(Transform parent, string name)
        {
            Transform tabRoot = EnsureChild(parent, name);
            Image background = GetOrAddComponent<Image>(tabRoot.gameObject);
            background.color = new Color(0.81f, 0.74f, 0.63f, 1f);

            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(tabRoot.gameObject);
            layoutElement.preferredHeight = 52f;
            layoutElement.preferredWidth = 148f;

            Button button = GetOrAddComponent<Button>(tabRoot.gameObject);
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.94f, 0.89f, 0.77f, 1f);
            colors.pressedColor = new Color(0.73f, 0.65f, 0.53f, 1f);
            button.colors = colors;

            TMP_Text labelText = EnsureText(tabRoot, "Label", name, 20f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetStretch(labelText.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 6f), new Vector2(-10f, -6f));

            NotebookTabButtonView tabView = GetOrAddComponent<NotebookTabButtonView>(tabRoot.gameObject);
            tabView.Configure(button, labelText);
            tabView.Hide();
            return tabView;
        }

        private NotebookEntryView EnsureEntryTemplate(Transform entriesRoot)
        {
            Transform templateRoot = EnsureChild(entriesRoot, "Entry Template");
            Image cardBackground = GetOrAddComponent<Image>(templateRoot.gameObject);
            cardBackground.color = new Color(0.99f, 0.97f, 0.9f, 0.94f);

            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(templateRoot.gameObject);
            layoutElement.preferredHeight = NotebookEntryCellLayout.DefaultPreferredHeight;

            Transform highlightRoot = EnsureChild(templateRoot, "Highlight");
            Image highlightImage = GetOrAddComponent<Image>(highlightRoot.gameObject);
            highlightImage.color = new Color(0.98f, 0.85f, 0.43f, 0.18f);
            SetStretch(highlightRoot as RectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 10f), new Vector2(-10f, -10f));

            TMP_Text titleText = EnsureText(templateRoot, "Title", "Placeholder Title", 26f, FontStyles.Bold, TextAlignmentOptions.TopLeft);

            Transform badgeRoot = EnsureChild(templateRoot, "New Badge");
            Image badgeBackground = GetOrAddComponent<Image>(badgeRoot.gameObject);
            badgeBackground.color = new Color(0.66f, 0.11f, 0.11f, 1f);
            SetAnchored(badgeRoot as RectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -20f), new Vector2(116f, 40f));
            TMP_Text badgeText = EnsureText(badgeRoot, "Badge Label", "NEW", 18f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetStretch(badgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f));

            TMP_Text bodyText = templateRoot.Find("Body")?.GetComponent<TMP_Text>()
                ?? templateRoot.Find("Content Row/Body")?.GetComponent<TMP_Text>()
                ?? EnsureText(templateRoot, "Body", "Placeholder notebook text.", 20f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            bodyText.textWrappingMode = TextWrappingModes.Normal;

            Transform imageRoot = templateRoot.Find("Image Root")
                ?? templateRoot.Find("Content Row/Image Root")
                ?? EnsureChild(templateRoot, "Image Root");
            Image imageFrame = GetOrAddComponent<Image>(imageRoot.gameObject);
            imageFrame.color = new Color(0.9f, 0.85f, 0.76f, 1f);
            Image entryImage = GetOrAddComponent<Image>(EnsureChild(imageRoot, "Image").gameObject);

            NotebookEntryCellLayout cellLayout = GetOrAddComponent<NotebookEntryCellLayout>(templateRoot.gameObject);
            cellLayout.ApplyTo(templateRoot as RectTransform);
            NotebookEntryView entryView = GetOrAddComponent<NotebookEntryView>(templateRoot.gameObject);
            entryView.ConfigureFromHierarchy();
            templateRoot.gameObject.SetActive(false);
            return entryView;
        }

        private NotebookIndexEntryView EnsureIndexEntryTemplate(Transform parent)
        {
            Transform root = EnsureChild(parent, "Index Entry Template");
            Image background = GetOrAddComponent<Image>(root.gameObject);
            background.color = new Color(1f, 1f, 1f, 0.18f);

            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(root.gameObject);
            layoutElement.preferredHeight = 40f;

            Button button = GetOrAddComponent<Button>(root.gameObject);
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            button.colors = colors;

            Transform highlightRoot = EnsureChild(root, "Highlight");
            Image highlight = GetOrAddComponent<Image>(highlightRoot.gameObject);
            highlight.color = new Color(0.98f, 0.85f, 0.43f, 0.16f);
            SetStretch(highlightRoot as RectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));

            TMP_Text titleText = EnsureText(root, "Title", "Index Entry", 18f, FontStyles.Normal, TextAlignmentOptions.Left);
            SetStretch(titleText.rectTransform, Vector2.zero, Vector2.one, new Vector2(14f, 4f), new Vector2(-84f, -4f));

            TMP_Text pageNumberText = EnsureText(root, "Page Number", "1", 18f, FontStyles.Bold, TextAlignmentOptions.Right);
            SetStretch(pageNumberText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 4f), new Vector2(-14f, -4f));

            Transform badgeRoot = EnsureChild(root, "New Badge");
            Image badgeBackground = GetOrAddComponent<Image>(badgeRoot.gameObject);
            badgeBackground.color = new Color(0.66f, 0.11f, 0.11f, 1f);
            SetAnchored(badgeRoot as RectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-74f, 0f), new Vector2(52f, 22f));
            TMP_Text badgeText = EnsureText(badgeRoot, "Badge Label", "NEW", 11f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetStretch(badgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 2f), new Vector2(-4f, -2f));

            NotebookIndexEntryView view = GetOrAddComponent<NotebookIndexEntryView>(root.gameObject);
            view.Configure(button, titleText, pageNumberText, badgeRoot.gameObject, highlightRoot.gameObject);
            root.gameObject.SetActive(false);
            return view;
        }

        private static void ApplyNotebookPanel(Image targetImage)
        {
            if (targetImage == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (targetImage.sprite == null)
            {
                targetImage.sprite = LoadSpriteAsset(NotebookUiArtPath);
            }
#endif
            targetImage.type = Image.Type.Simple;
            targetImage.preserveAspect = false;
        }

        private static void ApplyNotebookBackground(Image targetImage)
        {
            if (targetImage == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (targetImage.sprite == null)
            {
                targetImage.sprite = LoadSpriteAsset(NotebookOpenedArtPath);
            }
#endif
            targetImage.type = Image.Type.Simple;
            targetImage.preserveAspect = true;
        }

        private static List<Sprite> LoadNotebookAnimationFrames()
        {
            List<Sprite> frames = new List<Sprite>();

#if UNITY_EDITOR
            for (int i = 0; i < NotebookOpenFramePaths.Length; i++)
            {
                Sprite frame = LoadSpriteAsset(NotebookOpenFramePaths[i]);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
#endif

            return frames;
        }

#if UNITY_EDITOR
        private static Sprite LoadSpriteAsset(string assetPath)
        {
            EnsureSpriteImport(assetPath);
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void EnsureSpriteImport(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
#endif

        private NotebookOverlayView EnsureOverlay(Transform parent)
        {
            Transform overlayRoot = EnsureChild(parent, "Notebook Overlay");
            SetStretch(overlayRoot as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            overlayRoot.SetAsLastSibling();

            Transform collectibleHintRoot = EnsureChild(overlayRoot, "Collectible Hint");
            Image collectibleHintBackground = GetOrAddComponent<Image>(collectibleHintRoot.gameObject);
            collectibleHintBackground.color = new Color(0.17f, 0.13f, 0.09f, 0.9f);
            SetAnchored(collectibleHintRoot as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -108f), new Vector2(420f, 52f));
            TMP_Text collectibleHintText = EnsureText(collectibleHintRoot, "Label", "Collectable available", 20f, FontStyles.Bold, TextAlignmentOptions.Center);
            collectibleHintText.color = new Color(0.98f, 0.95f, 0.83f, 1f);
            SetStretch(collectibleHintText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 6f), new Vector2(-12f, -6f));

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
            SetStretch(collectedToastText.rectTransform, Vector2.zero, Vector2.one, new Vector2(18f, 10f), new Vector2(-18f, -10f));

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

        private Button EnsureTextButton(Transform parent, string name, string label, Color backgroundColor)
        {
            Transform buttonRoot = EnsureChild(parent, name);
            Image background = GetOrAddComponent<Image>(buttonRoot.gameObject);
            background.color = backgroundColor;

            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(buttonRoot.gameObject);
            layoutElement.preferredWidth = 260f;
            layoutElement.preferredHeight = 58f;

            Button button = GetOrAddComponent<Button>(buttonRoot.gameObject);
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            button.colors = colors;

            TMP_Text labelText = EnsureText(buttonRoot, "Label", label, 22f, FontStyles.Bold, TextAlignmentOptions.Center);
            SetStretch(labelText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f));
            return button;
        }

        private TMP_Text EnsureText(Transform parent, string name, string text, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
        {
            Transform textRoot = EnsureChild(parent, name);
            TMP_Text tmpText = GetOrAddComponent<TextMeshProUGUI>(textRoot.gameObject);
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.fontStyle = fontStyle;
            tmpText.alignment = alignment;
            tmpText.color = new Color(0.19f, 0.15f, 0.11f, 1f);
            return tmpText;
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                component = target.AddComponent<T>();
            }

            return component;
        }

        private static List<T> FindComponentsInDirectChildren<T>(Transform parent) where T : Component
        {
            List<T> components = new List<T>();
            if (parent == null)
            {
                return components;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                T component = parent.GetChild(i).GetComponent<T>();
                if (component != null)
                {
                    components.Add(component);
                }
            }

            return components;
        }

        private static void RemoveAllComponentsIfPresent<T>(GameObject target) where T : Component
        {
            T[] components = target.GetComponents<T>();
            for (int i = 0; i < components.Length; i++)
            {
                RemoveComponentInstance(components[i]);
            }
        }

        private static void RemoveDuplicateComponents<T>(GameObject target) where T : Component
        {
            T[] components = target.GetComponents<T>();
            for (int i = 1; i < components.Length; i++)
            {
                RemoveComponentInstance(components[i]);
            }
        }

        private static void RemoveComponentInstance(Component component)
        {
            if (component == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(component);
                return;
            }

#if UNITY_EDITOR
            Object.DestroyImmediate(component);
#else
            Object.Destroy(component);
#endif
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child;
            }

            GameObject childObject = new GameObject(childName, typeof(RectTransform));
            child = childObject.transform;
            child.SetParent(parent, false);
            return child;
        }

        private static void RemoveChildIfPresent(Transform parent, string childName)
        {
            if (parent == null)
            {
                return;
            }

            Transform child = parent.Find(childName);
            if (child == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(child.gameObject);
                return;
            }

#if UNITY_EDITOR
            Object.DestroyImmediate(child.gameObject);
#else
            Object.Destroy(child.gameObject);
#endif
        }

        private static void SetupVerticalLayout(GameObject target, float spacing, RectOffset padding)
        {
            VerticalLayoutGroup layout = GetOrAddComponent<VerticalLayoutGroup>(target);
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        private static void SetupHorizontalLayout(GameObject target, float spacing, RectOffset padding)
        {
            HorizontalLayoutGroup layout = GetOrAddComponent<HorizontalLayoutGroup>(target);
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            ContentSizeFitter fitter = GetOrAddComponent<ContentSizeFitter>(target);
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void ClearLayoutComponents(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            RemoveAllComponentsIfPresent<VerticalLayoutGroup>(target);
            RemoveAllComponentsIfPresent<HorizontalLayoutGroup>(target);
            RemoveAllComponentsIfPresent<ContentSizeFitter>(target);
        }

        private static void SetStretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (rectTransform == null
                || NotebookUILayoutGuard.ShouldSkipLayoutApply(rectTransform))
            {
                return;
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void SetAnchored(RectTransform rectTransform, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            if (rectTransform == null
                || NotebookUILayoutGuard.ShouldSkipLayoutApply(rectTransform))
            {
                return;
            }

            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
        }

        private void WireNotebookScene()
        {
            if (notebookController == null)
            {
                notebookController = GetComponent<NotebookController>();
            }

            if (notebookController == null)
            {
                notebookController = FindFirstObjectByType<NotebookController>();
            }

            NotebookUIShellReferences shell = NotebookSceneLookup.FindShell();
            if (shell != null && notebookController != null)
            {
                shell.ApplyToController(notebookController);

                NotebookOpenButton hudButton = NotebookSceneLookup.FindOpenButton();
                if (hudButton != null)
                {
                    hudButton.Configure(notebookController);
                }

#if UNITY_EDITOR
                if (database != null)
                {
                    SerializedObject serializedController = new SerializedObject(notebookController);
                    serializedController.FindProperty("database").objectReferenceValue = database;
                    serializedController.ApplyModifiedPropertiesWithoutUndo();
                }
#endif
            }
#if UNITY_EDITOR
            else if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "Notebook UI shell not found. Run Peaceland/Notebook/Author Open UI In Active Scene, then save the scene.",
                    this);
            }
#endif

            if (GetComponent<NotebookTestHarness>() == null)
            {
                gameObject.AddComponent<NotebookTestHarness>();
            }

            if (Application.isPlaying)
            {
                EnsurePaginationDummyEntriesCollectedForTest();
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && shell != null)
            {
                ApplyEditorPreviewState(
                    shell.OpenRoot,
                    shell.DirectoryPageRoot != null ? shell.DirectoryPageRoot.transform : null,
                    shell.SectionPageRoot != null ? shell.SectionPageRoot.transform : null);
                EditorUtility.SetDirty(notebookController);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            NotebookSceneAuthoringProfile authoringProfile = NotebookSceneAuthoringProfile.FindInScene();
            if (authoringProfile != null && authoringProfile.DisableRuntimeBootstrapRebuild)
            {
                if (authoringProfile.CollectDummyEntriesOnPlay)
                {
                    EnsurePaginationDummyEntriesCollectedForTest();
                }

                return;
            }

            EnsureSetup();

            if (Application.isPlaying)
            {
                EnsurePaginationDummyEntriesCollectedForTest();
            }
        }

        private void EnsurePaginationDummyEntriesCollectedForTest()
        {
            if (database == null || notebookController == null)
            {
                return;
            }

            const string dummyPrefix = "NotebookEntry_DummyPage_";
            for (int i = 0; i < database.Entries.Count; i++)
            {
                NotebookEntryDefinition entry = database.Entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.EntryId))
                {
                    continue;
                }

                if (!entry.EntryId.StartsWith(dummyPrefix))
                {
                    continue;
                }

                notebookController.CollectEntry(entry.EntryId);
            }
        }
    }
}
