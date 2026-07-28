using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Peaceland;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookController : MonoBehaviour
    {
        [Serializable]
        public class SectionDefinition
        {
            public NotebookSection section = NotebookSection.Present;
            public string displayName = "Present";
        }

        [Header("Data")]
        [SerializeField] private NotebookDatabase database;
        [SerializeField] private string saveKey = "peaceland.notebook.state";
        [SerializeField] private List<SectionDefinition> sections = new List<SectionDefinition>
        {
            new SectionDefinition { section = NotebookSection.Present, displayName = "Present" },
            new SectionDefinition { section = NotebookSection.Memory1, displayName = "Memory 1 (Florist)" },
            new SectionDefinition { section = NotebookSection.Memory2, displayName = "Memory 2 (R&J)" },
            new SectionDefinition { section = NotebookSection.HiddenStats, displayName = "Hidden Stats" },
        };
        [SerializeField] private NotebookBookLayoutSettings layoutSettings = new NotebookBookLayoutSettings
        {
            contentPageHeight = 520f,
            indexPageHeight = 520f,
        };

        [Header("Root UI")]
        [SerializeField] private Button notebookButton;
        [SerializeField] private GameObject notebookOpenRoot;
        [SerializeField] private GameObject directoryPageRoot;
        [SerializeField] private GameObject sectionPageRoot;
        [SerializeField] private NotebookAnimationView animationView;
        [SerializeField] private NotebookOverlayView overlayView;

        private NotebookUIShellReferences activeShell;

        [Header("Directory UI")]
        [SerializeField] private List<NotebookDirectoryLineView> directoryLines = new List<NotebookDirectoryLineView>();
        [SerializeField] private List<NotebookDirectoryCellView> directoryCells = new List<NotebookDirectoryCellView>();

        [Header("Bookmark Tabs")]
        [SerializeField] private NotebookBookmarkTabBar bookmarkTabBar;

        [SerializeField] private TMP_Text leftPageNumberText;
        [SerializeField] private TMP_Text rightPageNumberText;

        [Header("Page Navigation")]
        [SerializeField] private Button previousSpreadButton;
        [SerializeField] private Button nextSpreadButton;
        [SerializeField] private float pageTurnDistance = 34f;
        [SerializeField] private float pageTurnDuration = 0.12f;

        [Header("Section Index UI")]
        [SerializeField] private GameObject indexRoot;
        [SerializeField] private Transform indexLeftContainer;
        [SerializeField] private Transform indexRightContainer;
        [SerializeField] private NotebookIndexEntryView indexEntryPrefab;

        [Header("Section Content UI")]
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private Transform contentLeftContainer;
        [SerializeField] private Transform contentRightContainer;
        [SerializeField] private NotebookEntryView entryViewPrefab;
        [SerializeField] private NotebookHiddenStatsPanelView hiddenStatsPanel;

        private readonly Dictionary<string, NotebookEntryStateData> stateById = new Dictionary<string, NotebookEntryStateData>();
        private readonly List<NotebookIndexEntryView> spawnedIndexViews = new List<NotebookIndexEntryView>();
        private readonly List<TMP_Text> spawnedCategoryLabels = new List<TMP_Text>();
        private readonly List<NotebookEntryView> spawnedEntryViews = new List<NotebookEntryView>();
        private readonly HashSet<string> pendingReviewedEntryIds = new HashSet<string>();

        private NotebookBookLayout currentLayout;
        private int currentSpreadIndex;
        private bool isOpen;
        private bool isAnimatingSpreadChange;
        private int collectedCounter;
        private int activeCollectableSources;

        public UnityEvent<string> OnEntryCollected;

        public NotebookDatabase Database => database;

        private void Awake()
        {
            BuildStateCache();
            LoadState();
            WireButtons();

            if (notebookOpenRoot != null)
            {
                notebookOpenRoot.SetActive(false);
            }

            if (animationView != null)
            {
                animationView.SetClosedFrame();
            }

            RefreshCollectableOverlay();
            ApplyHiddenStatsSectionVisibility();
        }

        private void ApplyHiddenStatsSectionVisibility()
        {
            if (NotebookFeatureFlags.IncludeHiddenStatsSection)
            {
                return;
            }

            HideSectionUi(NotebookSection.HiddenStats);
            if (bookmarkTabBar != null)
            {
                bookmarkTabBar.ApplyFeatureVisibility();
            }
        }

        private void HideSectionUi(NotebookSection section)
        {
            if (directoryLines != null)
            {
                for (int i = 0; i < directoryLines.Count; i++)
                {
                    NotebookDirectoryLineView line = directoryLines[i];
                    if (line != null && line.Section == section)
                    {
                        line.gameObject.SetActive(false);
                    }
                }
            }

            if (directoryCells != null)
            {
                for (int i = 0; i < directoryCells.Count; i++)
                {
                    NotebookDirectoryCellView cell = directoryCells[i];
                    if (cell != null && cell.Section == section)
                    {
                        cell.gameObject.SetActive(false);
                    }
                }
            }
        }

        private IEnumerable<SectionDefinition> GetLayoutSections()
        {
            return ResolveLayoutSections(sections);
        }

        internal static IEnumerable<SectionDefinition> ResolveLayoutSections(IEnumerable<SectionDefinition> configuredSections)
        {
            List<SectionDefinition> resolvedSections = (configuredSections ?? Enumerable.Empty<SectionDefinition>())
                .Where(section => section != null && NotebookFeatureFlags.IsSectionVisible(section.section))
                .ToList();

            if (NotebookFeatureFlags.IncludeHiddenStatsSection
                && resolvedSections.All(section => section.section != NotebookSection.HiddenStats))
            {
                resolvedSections.Add(new SectionDefinition
                {
                    section = NotebookSection.HiddenStats,
                    displayName = "Hidden Stats",
                });
            }

            return resolvedSections;
        }

        private void OnEnable()
        {
            NotebookGlobalBridge.RegisterController(this);
            if (PeacelandSaveService.HasInstance)
            {
                PeacelandSaveService.Instance.DataLoaded += HandleExternalSaveLoaded;
            }
        }

        private void OnDisable()
        {
            NotebookGlobalBridge.UnregisterController(this);
            if (PeacelandSaveService.HasInstance)
            {
                PeacelandSaveService.Instance.DataLoaded -= HandleExternalSaveLoaded;
            }
        }

        private void HandleExternalSaveLoaded()
        {
            stateById.Clear();
            collectedCounter = 0;
            BuildStateCache();
            ApplyLoadedNotebookState(PeacelandSaveService.Instance.GetNotebookData());
            RefreshCollectableOverlay();
            if (isOpen)
            {
                RebuildLayout();
                RenderCurrentSpread();
            }
        }

        public void ToggleNotebook()
        {
            if (isOpen)
            {
                CloseNotebook();
                return;
            }

            OpenNotebook();
        }

        public void OpenNotebook()
        {
            if (isOpen)
            {
                ShowDirectory();
                return;
            }

            isOpen = true;
            if (notebookOpenRoot != null)
            {
                notebookOpenRoot.SetActive(true);
            }

            EnsurePageClipMasks();
            StopAllCoroutines();
            isAnimatingSpreadChange = false;
            ResetSpreadRootsPresentation();
            StartCoroutine(OpenRoutine());
        }

        public void CloseNotebook()
        {
            FinalizePendingReview();
            isOpen = false;
            currentSpreadIndex = 0;

            if (notebookOpenRoot != null)
            {
                notebookOpenRoot.SetActive(false);
            }

            if (animationView != null)
            {
                animationView.ResetPresentation();
            }

            isAnimatingSpreadChange = false;
            ResetSpreadRootsPresentation();
        }

        public void ShowDirectory()
        {
            FinalizePendingReview();
            RebuildLayout();
            currentSpreadIndex = 0;
            RenderCurrentSpread();
        }

        public string GetSectionDisplayName(NotebookSection section)
        {
            if (section == NotebookSection.Directory)
            {
                return "Directory";
            }

            SectionDefinition definition = sections.FirstOrDefault(entry => entry != null && entry.section == section);
            return definition != null && !string.IsNullOrWhiteSpace(definition.displayName)
                ? definition.displayName
                : section.ToString();
        }

        public void ShowSection(NotebookSection section)
        {
            ShowSectionDirectory(section);
        }

        public void ShowSectionDirectory(NotebookSection section)
        {
            if (!NotebookFeatureFlags.IsSectionVisible(section))
            {
                return;
            }

            if (section == NotebookSection.Directory)
            {
                ShowDirectory();
                return;
            }

            FinalizePendingReview();
            RebuildLayout();

            int spreadIndex = currentLayout != null
                ? currentLayout.Spreads.FindIndex(spread =>
                    (spread.Kind == NotebookSpreadKind.SectionDirectory || spread.Kind == NotebookSpreadKind.StatEditor)
                    && spread.Section == section)
                : -1;

            if (spreadIndex < 0)
            {
                ShowSectionFirstContent(section);
                return;
            }

            currentSpreadIndex = spreadIndex;
            RenderCurrentSpread();
        }

        public void ShowSectionFirstContent(NotebookSection section)
        {
            if (!NotebookFeatureFlags.IsSectionVisible(section))
            {
                return;
            }

            if (section == NotebookSection.Directory)
            {
                ShowDirectory();
                return;
            }

            FinalizePendingReview();
            RebuildLayout();

            int spreadIndex = currentLayout != null
                ? currentLayout.Spreads.FindIndex(spread => spread.Kind == NotebookSpreadKind.Content && spread.Section == section)
                : -1;

            if (spreadIndex < 0 && currentLayout != null)
            {
                spreadIndex = currentLayout.Spreads.FindIndex(spread =>
                    (spread.Kind == NotebookSpreadKind.SectionDirectory || spread.Kind == NotebookSpreadKind.StatEditor)
                    && spread.Section == section);
            }

            if (spreadIndex < 0)
            {
                spreadIndex = 0;
            }

            currentSpreadIndex = spreadIndex;
            RenderCurrentSpread();
        }

        public void ShowNextSpread()
        {
            FinalizePendingReview();
            RebuildLayout();

            if (currentLayout == null || currentLayout.Spreads.Count == 0 || isAnimatingSpreadChange)
            {
                return;
            }

            int targetSpreadIndex = Mathf.Min(currentSpreadIndex + 1, currentLayout.Spreads.Count - 1);
            if (targetSpreadIndex == currentSpreadIndex)
            {
                UpdatePageNavigation();
                return;
            }

            StartCoroutine(AnimateSpreadChange(targetSpreadIndex, -1f));
        }

        public void ShowPreviousSpread()
        {
            FinalizePendingReview();
            RebuildLayout();

            if (currentLayout == null || currentLayout.Spreads.Count == 0 || isAnimatingSpreadChange)
            {
                return;
            }

            int targetSpreadIndex = Mathf.Max(currentSpreadIndex - 1, 0);
            if (targetSpreadIndex == currentSpreadIndex)
            {
                UpdatePageNavigation();
                return;
            }

            StartCoroutine(AnimateSpreadChange(targetSpreadIndex, 1f));
        }

        public void JumpToEntry(string entryId)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return;
            }

            FinalizePendingReview();
            RebuildLayout();

            int foundPage;
            int targetPage = currentLayout != null && currentLayout.EntryPageById.TryGetValue(entryId, out foundPage)
                ? foundPage
                : 0;
            if (targetPage <= 0)
            {
                return;
            }

            currentSpreadIndex = Mathf.Max(0, (targetPage - 1) / 2);
            RenderCurrentSpread();
        }

        public void CollectEntry(string entryId)
        {
            if (database == null || string.IsNullOrWhiteSpace(entryId))
            {
                return;
            }

            NotebookEntryDefinition definition = database.GetEntry(entryId);
            if (definition == null)
            {
                Debug.LogWarning("Notebook entry '" + entryId + "' is not in the notebook database.", this);
                return;
            }

            NotebookEntryStateData state = GetOrCreateState(entryId);
            if (state.isCollected)
            {
                return;
            }

            collectedCounter++;
            state.isCollected = true;
            state.isReviewed = false;
            state.collectedOrder = collectedCounter;

            SaveState();
            if (OnEntryCollected != null)
            {
                OnEntryCollected.Invoke(entryId);
            }

            if (overlayView != null)
            {
                overlayView.PlayCollectedToast("Notebook updated: " + definition.Title);
            }

            if (isOpen)
            {
                RebuildLayout();
                RenderCurrentSpread();
            }
        }

        public void CollectEntriesByIds(IEnumerable<string> entryIds)
        {
            if (entryIds == null)
            {
                return;
            }

            foreach (string entryId in entryIds)
            {
                CollectEntry(entryId);
            }
        }

        public void RegisterCollectableSource()
        {
            activeCollectableSources++;
            RefreshCollectableOverlay();
        }

        public void UnregisterCollectableSource()
        {
            activeCollectableSources = Mathf.Max(0, activeCollectableSources - 1);
            RefreshCollectableOverlay();
        }

        public void QueueCollectEntriesFromAnyScene(IEnumerable<string> entryIds)
        {
            NotebookGlobalBridge.CollectEntries(entryIds);
        }

        public void QueueCollectEntryFromAnyScene(string entryId)
        {
            NotebookGlobalBridge.CollectEntry(entryId);
        }

        public void ApplyShellReferences(NotebookUIShellReferences shell)
        {
            if (shell == null)
            {
                return;
            }

            activeShell = shell;

            notebookOpenRoot = shell.OpenRoot != null ? shell.OpenRoot.gameObject : notebookOpenRoot;
            directoryPageRoot = shell.DirectoryPageRoot;
            sectionPageRoot = shell.SectionPageRoot;
            animationView = shell.AnimationView;
            overlayView = shell.OverlayView;
            bookmarkTabBar = shell.BookmarkTabBar;
            directoryLines = shell.DirectoryLines != null
                ? new List<NotebookDirectoryLineView>(shell.DirectoryLines)
                : directoryLines;
            leftPageNumberText = shell.LeftPageNumberText;
            rightPageNumberText = shell.RightPageNumberText;
            previousSpreadButton = shell.PreviousSpreadButton;
            nextSpreadButton = shell.NextSpreadButton;
            indexRoot = shell.IndexRoot;
            indexLeftContainer = shell.IndexLeftContainer;
            indexRightContainer = shell.IndexRightContainer;
            indexEntryPrefab = shell.IndexEntryTemplate;
            contentRoot = shell.ContentPagesRoot;
            contentLeftContainer = shell.ContentLeftContainer;
            contentRightContainer = shell.ContentRightContainer;
            entryViewPrefab = shell.EntryTemplate;

            if (bookmarkTabBar != null)
            {
                Transform openRoot = shell.OpenRoot;
                Transform leftRail = openRoot != null ? openRoot.Find("Bookmark Tabs Left") : null;
                Transform rightRail = openRoot != null ? openRoot.Find("Bookmark Tabs Right") : null;
                bookmarkTabBar.Configure(
                    this,
                    NotebookBookmarkTabBar.CollectTabsFromOpenRoot(openRoot),
                    leftRail as RectTransform,
                    rightRail as RectTransform);
            }

            WireButtons();
            RefreshCollectableOverlay();
            ApplyHiddenStatsSectionVisibility();
        }

        public void Configure(
            NotebookDatabase notebookDatabase,
            Button toggleButton,
            GameObject openRoot,
            GameObject directoryRoot,
            GameObject targetSectionRoot,
            NotebookAnimationView notebookAnimation,
            NotebookOverlayView targetOverlayView,
            IEnumerable<NotebookDirectoryCellView> directoryCellViews,
            TMP_Text sectionTitle,
            TMP_Text breadcrumbText,
            TMP_Text leftPageText,
            TMP_Text rightPageText,
            Button backButton,
            Button previousButton,
            Button nextButton,
            GameObject targetIndexRoot,
            Transform targetIndexLeftContainer,
            Transform targetIndexRightContainer,
            NotebookIndexEntryView targetIndexEntryPrefab,
            GameObject targetContentRoot,
            Transform targetContentLeftContainer,
            Transform targetContentRightContainer,
            NotebookEntryView targetEntryPrefab)
        {
            database = notebookDatabase;
            notebookButton = toggleButton;
            notebookOpenRoot = openRoot;
            directoryPageRoot = directoryRoot;
            sectionPageRoot = targetSectionRoot;
            animationView = notebookAnimation;
            overlayView = targetOverlayView;
            directoryCells = directoryCellViews != null
                ? directoryCellViews.Where(view => view != null).ToList()
                : new List<NotebookDirectoryCellView>();
            leftPageNumberText = leftPageText;
            rightPageNumberText = rightPageText;
            previousSpreadButton = previousButton;
            nextSpreadButton = nextButton;
            indexRoot = targetIndexRoot;
            indexLeftContainer = targetIndexLeftContainer;
            indexRightContainer = targetIndexRightContainer;
            indexEntryPrefab = targetIndexEntryPrefab;
            contentRoot = targetContentRoot;
            contentLeftContainer = targetContentLeftContainer;
            contentRightContainer = targetContentRightContainer;
            entryViewPrefab = targetEntryPrefab;

            BuildStateCache();
            WireButtons();
            RefreshCollectableOverlay();
        }

        public void ClearSavedStateForDebug()
        {
            if (PeacelandSaveService.HasInstance)
            {
                PeacelandSaveService.Instance.ClearNotebookSection();
            }
            else
            {
                PlayerPrefs.DeleteKey(saveKey);
                PlayerPrefs.Save();
            }

            collectedCounter = 0;
            currentSpreadIndex = 0;
            pendingReviewedEntryIds.Clear();
            BuildStateCache();
            RebuildLayout();

            if (isOpen)
            {
                ShowDirectory();
            }
        }

        public bool HasCollected(string entryId)
        {
            return GetOrCreateState(entryId).isCollected;
        }

        public bool IsEntryNew(string entryId)
        {
            NotebookEntryStateData state = GetOrCreateState(entryId);
            return state.isCollected && !state.isReviewed;
        }

        public int GetNewCount(NotebookSection section)
        {
            if (section == NotebookSection.HiddenStats)
            {
                return 0;
            }

            return stateById.Values.Count(state =>
            {
                if (!state.isCollected || state.isReviewed)
                {
                    return false;
                }

                NotebookEntryDefinition definition = database != null ? database.GetEntry(state.entryId) : null;
                return definition != null && definition.Section == section;
            });
        }

        private IEnumerator OpenRoutine()
        {
            if (directoryPageRoot != null)
            {
                directoryPageRoot.SetActive(false);
            }

            if (sectionPageRoot != null)
            {
                sectionPageRoot.SetActive(false);
            }

            if (animationView != null && animationView.HasFrames)
            {
                SetBookChromeVisible(false);
                animationView.SetOverlayVisible(true);
                yield return animationView.PlayOpen();
                animationView.SetOpenFrame();
            }

            SetBookChromeVisible(true);
            ShowDirectory();
        }

        private void SetBookChromeVisible(bool visible)
        {
            if (activeShell != null)
            {
                activeShell.SetBookChromeVisible(visible);
                return;
            }

            if (notebookOpenRoot == null)
            {
                return;
            }

            NotebookUIShellReferences shell = notebookOpenRoot.GetComponent<NotebookUIShellReferences>();
            if (shell != null)
            {
                shell.SetBookChromeVisible(visible);
            }
        }

        private void WireButtons()
        {
            if (notebookButton != null)
            {
                notebookButton.onClick.RemoveAllListeners();
                notebookButton.onClick.AddListener(ToggleNotebook);
            }

            if (previousSpreadButton != null)
            {
                previousSpreadButton.onClick.RemoveAllListeners();
                previousSpreadButton.onClick.AddListener(ShowPreviousSpread);
            }

            if (nextSpreadButton != null)
            {
                nextSpreadButton.onClick.RemoveAllListeners();
                nextSpreadButton.onClick.AddListener(ShowNextSpread);
            }
        }

        private void RebuildLayout()
        {
            SyncLayoutHeightsFromViewport();
            float columnWidth = MeasurePageColumnWidth(contentLeftContainer);
            if (columnWidth < 10f)
            {
                columnWidth = MeasurePageColumnWidth(indexLeftContainer);
            }

            if (columnWidth < 10f)
            {
                columnWidth = 280f;
            }

            currentLayout = NotebookBookLayoutBuilder.Build(
                database,
                GetLayoutSections(),
                HasCollected,
                IsEntryNew,
                layoutSettings,
                entry => MeasureEntryHeight(entry, columnWidth));
        }

        private float MeasureEntryHeight(NotebookEntryDefinition entry, float columnWidth)
        {
            NotebookEntryCellLayout cellLayout = entryViewPrefab != null
                ? entryViewPrefab.GetComponent<NotebookEntryCellLayout>()
                : null;
            return NotebookEntryLayoutMeasurer.Measure(entry, columnWidth, cellLayout);
        }

        private void SyncLayoutHeightsFromViewport()
        {
            if (layoutSettings == null)
            {
                layoutSettings = new NotebookBookLayoutSettings();
            }

            NotebookBookArtLayout artLayout = notebookOpenRoot != null
                ? notebookOpenRoot.GetComponent<NotebookBookArtLayout>()
                : null;

            if (artLayout != null && artLayout.PageUsableHeight > 80f)
            {
                layoutSettings.contentPageHeight = artLayout.PageUsableHeight;
                layoutSettings.indexPageHeight = artLayout.PageUsableHeight;
                return;
            }

            Canvas.ForceUpdateCanvases();
            float measuredHeight = MeasureActivePageColumnHeight(true);
            if (measuredHeight <= 0f)
            {
                measuredHeight = MeasureActivePageColumnHeight(false);
            }

            if (measuredHeight > 80f)
            {
                float reserved = artLayout != null ? artLayout.PageTopReservedHeight : 0f;
                float usableHeight = Mathf.Max(120f, measuredHeight - 12f - reserved);
                layoutSettings.contentPageHeight = usableHeight;
                layoutSettings.indexPageHeight = usableHeight;
            }
        }

        private float MeasureActivePageColumnHeight(bool preferContent)
        {
            Transform container = preferContent ? contentLeftContainer : indexLeftContainer;
            if (container == null)
            {
                return 0f;
            }

            if (!container.gameObject.activeInHierarchy)
            {
                container = preferContent ? indexLeftContainer : contentLeftContainer;
            }

            if (container is RectTransform containerRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            }

            float height = MeasurePageColumnHeight(container);
            if (height > 0f)
            {
                return height;
            }

            Transform pageTransform = container.parent;
            return MeasurePageColumnHeight(pageTransform);
        }

        private void EnsurePageClipMasks()
        {
            EnsurePageClipMask(indexLeftContainer);
            EnsurePageClipMask(indexRightContainer);
            EnsurePageClipMask(contentLeftContainer);
            EnsurePageClipMask(contentRightContainer);
        }

        private static void EnsurePageClipMask(Transform container)
        {
            Transform pageTransform = container != null ? container.parent : null;
            if (pageTransform == null)
            {
                return;
            }

            if (pageTransform.GetComponent<RectMask2D>() == null)
            {
                pageTransform.gameObject.AddComponent<RectMask2D>();
            }
        }

        private static float MeasurePageColumnHeight(Transform container)
        {
            RectTransform rectTransform = container as RectTransform;
            if (rectTransform == null)
            {
                return 0f;
            }

            return rectTransform.rect.height;
        }

        private static float MeasurePageColumnWidth(Transform container)
        {
            RectTransform rectTransform = container as RectTransform;
            if (rectTransform == null)
            {
                return 0f;
            }

            return rectTransform.rect.width;
        }

        private void ResetSpreadRootsPresentation()
        {
            HideHiddenStatsPanel();
            ResetSpreadRootPresentation(directoryPageRoot);
            ResetSpreadRootPresentation(sectionPageRoot);
        }

        private static void ResetSpreadRootPresentation(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            RectTransform rectTransform = root.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }

            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private void RenderCurrentSpread()
        {
            RebuildLayout();
            RenderSpreadFromCurrentLayout();
        }

        private void RenderSpreadFromCurrentLayout()
        {
            if (currentLayout == null || currentLayout.Spreads.Count == 0)
            {
                UpdatePageNavigation();
                return;
            }

            currentSpreadIndex = Mathf.Clamp(currentSpreadIndex, 0, currentLayout.Spreads.Count - 1);
            NotebookSpreadLayout spread = currentLayout.Spreads[currentSpreadIndex];

            if (spread.Kind == NotebookSpreadKind.MainDirectory)
            {
                RenderDirectorySpread();
                UpdateBookmarkTabs(NotebookSection.Directory, true);
                UpdatePageNavigation();
                return;
            }

            RenderSectionSpread(spread);
            UpdateBookmarkTabs(spread.Section, false);
            UpdatePageNavigation();
        }

        private void UpdateBookmarkTabs(NotebookSection section, bool onMainDirectory)
        {
            if (bookmarkTabBar != null)
            {
                bookmarkTabBar.RefreshAndWire(section, onMainDirectory);
            }
        }

        private void RenderDirectorySpread()
        {
            ResetSpreadRootsPresentation();

            if (directoryPageRoot != null)
            {
                directoryPageRoot.SetActive(true);
            }

            if (sectionPageRoot != null)
            {
                sectionPageRoot.SetActive(false);
            }

            if (leftPageNumberText != null)
            {
                leftPageNumberText.text = "1";
            }

            if (rightPageNumberText != null)
            {
                rightPageNumberText.text = "2";
            }

            BindDirectoryEntries();

            UpdateBookmarkTabs(NotebookSection.Directory, true);
        }

        private void BindDirectoryEntries()
        {
            if (directoryLines != null && directoryLines.Count > 0)
            {
                for (int i = 0; i < directoryLines.Count; i++)
                {
                    NotebookDirectoryLineView line = directoryLines[i];
                    if (line == null || !NotebookFeatureFlags.IsSectionVisible(line.Section))
                    {
                        continue;
                    }

                    SectionDefinition definition = sections.FirstOrDefault(section => section.section == line.Section);
                    NotebookSection targetSection = line.Section;
                    line.Bind(
                        definition != null ? definition.displayName : targetSection.ToString(),
                        GetNewCount(targetSection),
                        () => ShowSectionDirectory(targetSection));
                }

                return;
            }

            for (int i = 0; i < directoryCells.Count; i++)
            {
                NotebookDirectoryCellView cell = directoryCells[i];
                if (cell == null || !NotebookFeatureFlags.IsSectionVisible(cell.Section))
                {
                    continue;
                }

                SectionDefinition definition = sections.FirstOrDefault(section => section.section == cell.Section);
                NotebookSection targetSection = cell.Section;
                cell.Bind(
                    definition != null ? definition.displayName : targetSection.ToString(),
                    GetNewCount(targetSection),
                    () => ShowSectionDirectory(targetSection));
            }
        }

        private void RenderSectionSpread(NotebookSpreadLayout spread)
        {
            ResetSpreadRootsPresentation();

            if (directoryPageRoot != null)
            {
                directoryPageRoot.SetActive(false);
            }

            if (sectionPageRoot != null)
            {
                sectionPageRoot.SetActive(true);
            }

            if (leftPageNumberText != null)
            {
                leftPageNumberText.gameObject.SetActive(true);
                leftPageNumberText.text = spread.LeftPageNumber.ToString();
            }

            if (rightPageNumberText != null)
            {
                rightPageNumberText.gameObject.SetActive(true);
                rightPageNumberText.text = spread.RightPageNumber.ToString();
            }

            ClearIndexViews();
            ClearEntryViews();
            HideHiddenStatsPanel();

            if (spread.Kind == NotebookSpreadKind.StatEditor)
            {
                RenderHiddenStatsSpread(spread);
                return;
            }

            bool showIndex = spread.Kind == NotebookSpreadKind.SectionDirectory;
            if (indexRoot != null)
            {
                indexRoot.SetActive(showIndex);
            }

            if (contentRoot != null)
            {
                contentRoot.SetActive(!showIndex);
            }

            if (showIndex)
            {
                RenderIndexGroups(indexLeftContainer, spread.LeftIndexGroups);
                RenderIndexGroups(indexRightContainer, spread.RightIndexGroups);
                return;
            }

            RenderEntries(contentLeftContainer, spread.LeftEntries, spread);
            RenderEntries(contentRightContainer, spread.RightEntries, spread);
        }

        private void RenderHiddenStatsSpread(NotebookSpreadLayout spread)
        {
            ClearEntryViews();

            if (directoryPageRoot != null)
            {
                directoryPageRoot.SetActive(false);
            }

            if (sectionPageRoot != null)
            {
                sectionPageRoot.SetActive(true);
            }

            if (leftPageNumberText != null)
            {
                leftPageNumberText.gameObject.SetActive(true);
                leftPageNumberText.text = spread.LeftPageNumber.ToString();
            }

            if (rightPageNumberText != null)
            {
                rightPageNumberText.gameObject.SetActive(true);
                rightPageNumberText.text = spread.RightPageNumber.ToString();
            }

            if (indexRoot != null)
            {
                indexRoot.SetActive(false);
            }

            if (contentRoot != null)
            {
                contentRoot.SetActive(true);
            }

            NotebookHiddenStatsPanelView panel = EnsureHiddenStatsPanel();
            panel.Show();
        }

        private void HideHiddenStatsPanel()
        {
            if (hiddenStatsPanel != null)
            {
                hiddenStatsPanel.Hide();
            }
        }

        private NotebookHiddenStatsPanelView EnsureHiddenStatsPanel()
        {
            if (hiddenStatsPanel != null)
            {
                return hiddenStatsPanel;
            }

            Transform host = contentLeftContainer != null ? contentLeftContainer : sectionPageRoot != null ? sectionPageRoot.transform : transform;
            GameObject panelObject = new GameObject("Hidden Stats Panel", typeof(RectTransform), typeof(NotebookHiddenStatsPanelView));
            panelObject.transform.SetParent(host, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            hiddenStatsPanel = panelObject.GetComponent<NotebookHiddenStatsPanelView>();
            return hiddenStatsPanel;
        }

        private void RenderIndexGroups(Transform container, IEnumerable<NotebookIndexGroupLayout> groups)
        {
            if (container == null || groups == null)
            {
                return;
            }

            foreach (NotebookIndexGroupLayout group in groups)
            {
                TMP_Text header = CreateCategoryLabel(container, group.DisplayName);
                spawnedCategoryLabels.Add(header);

                for (int i = 0; i < group.Entries.Count; i++)
                {
                    NotebookIndexEntryLayout entry = group.Entries[i];
                    if (indexEntryPrefab == null)
                    {
                        continue;
                    }

                    NotebookIndexEntryView view = Instantiate(indexEntryPrefab, container);
                    view.Bind(entry.Title, entry.TargetPageNumber, entry.IsNew, () => JumpToEntry(entry.EntryId));
                    spawnedIndexViews.Add(view);
                }
            }
        }

        private void RenderEntries(Transform container, IEnumerable<NotebookEntryDefinition> entries, NotebookSpreadLayout spread)
        {
            if (container == null || entries == null || entryViewPrefab == null)
            {
                return;
            }

            foreach (NotebookEntryDefinition entry in entries)
            {
                NotebookEntryStateData state = GetOrCreateState(entry.EntryId);
                NotebookEntryView view = Instantiate(entryViewPrefab, container);
                float layoutHeight = entry.LayoutHeight;
                if (spread != null && spread.TryGetEntryHeight(entry.EntryId, out float resolvedHeight))
                {
                    layoutHeight = resolvedHeight;
                }

                view.Bind(
                    entry,
                    !state.isReviewed,
                    QueuePendingReview,
                    layoutHeight,
                    state.recordChoiceCompleted,
                    state.selectedRecordChoiceIndex,
                    HandleRecordChoiceSelected);
                spawnedEntryViews.Add(view);
            }
        }

        private TMP_Text CreateCategoryLabel(Transform parent, string label)
        {
            GameObject labelObject = new GameObject("Category - " + label, typeof(RectTransform), typeof(LayoutElement), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);

            LayoutElement layout = labelObject.GetComponent<LayoutElement>();
            layout.preferredHeight = 42f;

            TMP_Text text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 22f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Left;
            text.color = new Color(0.19f, 0.15f, 0.11f, 1f);
            return text;
        }

        private void FinalizePendingReview()
        {
            if (pendingReviewedEntryIds.Count == 0)
            {
                return;
            }

            bool changed = false;
            foreach (string entryId in pendingReviewedEntryIds)
            {
                NotebookEntryStateData state = GetOrCreateState(entryId);
                if (!state.isCollected || state.isReviewed)
                {
                    continue;
                }

                NotebookEntryDefinition definition = database != null
                    ? database.GetEntry(entryId)
                    : null;
                if (definition != null && definition.RequiresRecordChoice && !state.recordChoiceCompleted)
                {
                    continue;
                }

                state.isReviewed = true;
                state.reviewCount++;
                changed = true;
            }

            pendingReviewedEntryIds.Clear();
            if (changed)
            {
                SaveState();
            }
        }

        private void QueuePendingReview(string entryId)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return;
            }

            NotebookEntryStateData state = GetOrCreateState(entryId);
            if (state.isCollected && !state.isReviewed)
            {
                pendingReviewedEntryIds.Add(entryId);
            }
        }

        private void HandleRecordChoiceSelected(string entryId, int choiceIndex)
        {
            if (database == null || string.IsNullOrWhiteSpace(entryId))
            {
                return;
            }

            NotebookEntryDefinition definition = database.GetEntry(entryId);
            if (definition == null || !definition.RequiresRecordChoice)
            {
                return;
            }

            IReadOnlyList<NotebookEntryDefinition.RecordChoice> choices = definition.RecordChoices;
            if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Count)
            {
                return;
            }

            NotebookEntryDefinition.RecordChoice choice = choices[choiceIndex];
            if (choice == null)
            {
                return;
            }

            NotebookEntryStateData state = GetOrCreateState(entryId);
            if (!state.isCollected || state.recordChoiceCompleted)
            {
                return;
            }

            if (choice.StatDelta != 0)
            {
                PeacelandStatManager.Instance.AddDelta(choice.StatId, choice.StatDelta);
            }

            state.recordChoiceCompleted = true;
            state.selectedRecordChoiceIndex = choiceIndex;
            state.selectedRecordChoiceId = choice.ChoiceId;
            if (!state.isReviewed)
            {
                state.isReviewed = true;
                state.reviewCount++;
            }

            pendingReviewedEntryIds.Remove(entryId);
            SaveState();
            RenderCurrentSpread();
        }

        private void RefreshCollectableOverlay()
        {
            if (overlayView != null)
            {
                overlayView.SetCollectibleHintVisible(activeCollectableSources > 0, activeCollectableSources);
            }
        }

        private void UpdatePageNavigation()
        {
            if (previousSpreadButton != null)
            {
                previousSpreadButton.interactable = currentLayout != null && currentSpreadIndex > 0 && !isAnimatingSpreadChange;
            }

            if (nextSpreadButton != null)
            {
                nextSpreadButton.interactable = currentLayout != null
                    && currentLayout.Spreads.Count > 0
                    && currentSpreadIndex < currentLayout.Spreads.Count - 1
                    && !isAnimatingSpreadChange;
            }
        }

        private IEnumerator AnimateSpreadChange(int targetSpreadIndex, float direction)
        {
            isAnimatingSpreadChange = true;
            UpdatePageNavigation();

            GameObject outgoingRoot = GetSpreadRootForCurrentIndex();
            yield return AnimateSpreadRoot(outgoingRoot, 0f, direction * -pageTurnDistance, 1f, 0f);

            currentSpreadIndex = targetSpreadIndex;
            RenderSpreadFromCurrentLayout();

            GameObject incomingRoot = GetSpreadRootForCurrentIndex();
            if (incomingRoot != null)
            {
                RectTransform incomingRect = incomingRoot.GetComponent<RectTransform>();
                CanvasGroup incomingGroup = incomingRoot.GetComponent<CanvasGroup>();
                if (incomingGroup == null)
                {
                    incomingGroup = incomingRoot.AddComponent<CanvasGroup>();
                }

                if (incomingRect != null)
                {
                    incomingRect.anchoredPosition = new Vector2(direction * pageTurnDistance, 0f);
                }

                incomingGroup.alpha = 0f;
            }

            yield return AnimateSpreadRoot(incomingRoot, direction * pageTurnDistance, 0f, 0f, 1f);

            isAnimatingSpreadChange = false;
            ResetSpreadRootsPresentation();
            UpdatePageNavigation();
        }

        private GameObject GetSpreadRootForCurrentIndex()
        {
            if (currentLayout == null || currentLayout.Spreads.Count == 0)
            {
                return null;
            }

            NotebookSpreadLayout spread = currentLayout.Spreads[Mathf.Clamp(currentSpreadIndex, 0, currentLayout.Spreads.Count - 1)];
            return spread.Kind == NotebookSpreadKind.MainDirectory ? directoryPageRoot : sectionPageRoot;
        }

        private IEnumerator AnimateSpreadRoot(GameObject root, float fromOffset, float toOffset, float fromAlpha, float toAlpha)
        {
            if (root == null)
            {
                yield break;
            }

            RectTransform rectTransform = root.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = root.AddComponent<CanvasGroup>();
            }

            if (rectTransform == null || pageTurnDuration <= 0f)
            {
                canvasGroup.alpha = toAlpha;
                yield break;
            }

            rectTransform.anchoredPosition = new Vector2(fromOffset, 0f);
            canvasGroup.alpha = fromAlpha;

            float elapsed = 0f;
            while (elapsed < pageTurnDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / pageTurnDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(new Vector2(fromOffset, 0f), new Vector2(toOffset, 0f), eased);
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);
                yield return null;
            }

            rectTransform.anchoredPosition = new Vector2(toOffset, 0f);
            canvasGroup.alpha = toAlpha;
        }

        private NotebookEntryStateData GetOrCreateState(string entryId)
        {
            NotebookEntryStateData existingState;
            if (stateById.TryGetValue(entryId, out existingState))
            {
                return existingState;
            }

            NotebookEntryStateData state = new NotebookEntryStateData
            {
                entryId = entryId,
                isCollected = false,
                isReviewed = false,
                reviewCount = 0,
                collectedOrder = 0,
                recordChoiceCompleted = false,
                selectedRecordChoiceIndex = -1,
            };
            stateById[entryId] = state;
            return state;
        }

        private void BuildStateCache()
        {
            stateById.Clear();
            if (database == null)
            {
                return;
            }

            for (int i = 0; i < database.Entries.Count; i++)
            {
                NotebookEntryDefinition entry = database.Entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.EntryId))
                {
                    continue;
                }

                GetOrCreateState(entry.EntryId);
            }
        }

        private void LoadState()
        {
            if (PeacelandSaveService.HasInstance)
            {
                ApplyLoadedNotebookState(PeacelandSaveService.Instance.GetNotebookData());
                return;
            }

            if (!PlayerPrefs.HasKey(saveKey))
            {
                return;
            }

            string json = PlayerPrefs.GetString(saveKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            NotebookSaveData saveData = JsonUtility.FromJson<NotebookSaveData>(json);
            ApplyLoadedNotebookState(saveData);
        }

        private void ApplyLoadedNotebookState(NotebookSaveData saveData)
        {
            if (saveData == null || saveData.states == null)
            {
                return;
            }

            for (int i = 0; i < saveData.states.Count; i++)
            {
                NotebookEntryStateData state = saveData.states[i];
                if (state == null || string.IsNullOrWhiteSpace(state.entryId))
                {
                    continue;
                }

                stateById[state.entryId] = state;
                collectedCounter = Mathf.Max(collectedCounter, state.collectedOrder);
            }
        }

        private void SaveState()
        {
            NotebookSaveData saveData = new NotebookSaveData
            {
                states = stateById.Values.OrderBy(state => state.collectedOrder).ToList(),
            };

            if (PeacelandSaveService.HasInstance)
            {
                PeacelandSaveService.Instance.ReplaceNotebookData(saveData);
                PeacelandSaveService.Instance.Save();
                return;
            }

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }

        private void ClearIndexViews()
        {
            for (int i = 0; i < spawnedIndexViews.Count; i++)
            {
                if (spawnedIndexViews[i] != null)
                {
                    Destroy(spawnedIndexViews[i].gameObject);
                }
            }

            for (int i = 0; i < spawnedCategoryLabels.Count; i++)
            {
                if (spawnedCategoryLabels[i] != null)
                {
                    Destroy(spawnedCategoryLabels[i].gameObject);
                }
            }

            spawnedIndexViews.Clear();
            spawnedCategoryLabels.Clear();
        }

        private void ClearEntryViews()
        {
            for (int i = 0; i < spawnedEntryViews.Count; i++)
            {
                if (spawnedEntryViews[i] != null)
                {
                    Destroy(spawnedEntryViews[i].gameObject);
                }
            }

            spawnedEntryViews.Clear();
        }
    }
}
