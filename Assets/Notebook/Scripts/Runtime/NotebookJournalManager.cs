using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookJournalManager : MonoBehaviour
    {
        public static NotebookJournalManager Instance { get; private set; }

        [Header("Artifact Database")]
        [Tooltip("All artifacts available in the game. Add definitions here.")]
        [SerializeField] private List<ArtifactDefinition> allArtifacts = new List<ArtifactDefinition>();

        [Header("Opening Animation")]
        [Tooltip("Image component used to play the frame-by-frame opening animation.")]
        [SerializeField] private UnityEngine.UI.Image animationImage;
        [Tooltip("The 5 animation frames: notebook, open_1, open_2, open_3, opened.")]
        [SerializeField] private List<Sprite> animationFrames = new List<Sprite>();
        [SerializeField] private float frameDuration = 0.08f;
        [SerializeField] private float startScale = 0.65f;
        [SerializeField] private float endScale = 1f;

        [Header("Open Book Root UI")]
        [SerializeField] private GameObject notebookRoot;
        [SerializeField] private CanvasGroup openBookContent;
        [SerializeField] private UnityEngine.UI.Image bookBackground;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private UnityEngine.UI.Button backgroundClickCatcher;

        [Header("Left Page - Directory")]
        [SerializeField] private TMP_Text directoryTitleText;
        [SerializeField] private Transform directoryContainer;
        [SerializeField] private GameObject chapterRowPrefab;

        [Header("Right Page - Current Page")]
        [SerializeField] private TMP_Text currentPageTitleText;
        [SerializeField] private Transform artifactCardsContainer;
        [SerializeField] private GameObject artifactCardPrefab;
        [SerializeField] private TMP_Text emptyChapterText;
        [SerializeField] private TMP_Text pageIndicatorText;

        [Header("Navigation Arrows")]
        [SerializeField] private UnityEngine.UI.Button previousPageButton;
        [SerializeField] private UnityEngine.UI.Button nextPageButton;

        [Header("HUD Integration")]
        [SerializeField] private UnityEngine.UI.Button hudNotebookButton;
        [SerializeField] private GameObject hudUnreadBadge;
        [SerializeField] private TMP_Text hudUnreadCountText;
        [SerializeField] private GameObject collectionToastRoot;
        [SerializeField] private TMP_Text collectionToastText;
        [SerializeField] private TMP_Text worldHoverText;

        [Header("Artifact Detail Popup")]
        [SerializeField] private NotebookArtifactDetailModal artifactDetailModal;

        // Runtime state
        private readonly HashSet<string> collectedIds = new HashSet<string>();
        private readonly HashSet<string> newIds = new HashSet<string>();
        private readonly List<ChapterData> chapters = new List<ChapterData>();

        private int selectedChapterIndex = 0;
        private int currentCardPageIndex = 0;
        private const int CardsPerPage = 2; 
        private bool isOpen = false;
        private bool isAnimating = false;
        private Coroutine toastCoroutine;

        public bool IsOpen => isOpen;

        public class ChapterData
        {
            public string Name;
            public string Category; // "MEMORIES" or "PRESENT"
            public List<ArtifactDefinition> Artifacts = new List<ArtifactDefinition>();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            InitializeChapters();
            InitializeStartingCollected();
            WireButtons();
        }

        private void Start()
        {
            // Start closed
            if (notebookRoot != null)
            {
                notebookRoot.SetActive(false);
            }
            if (animationImage != null)
            {
                animationImage.gameObject.SetActive(false);
            }
            if (collectionToastRoot != null)
            {
                collectionToastRoot.SetActive(false);
            }
            if (worldHoverText != null)
            {
                worldHoverText.gameObject.SetActive(false);
            }

            UpdateHudBadge();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.jKey.wasPressedThisFrame || keyboard.nKey.wasPressedThisFrame || keyboard.tabKey.wasPressedThisFrame)
                {
                    ToggleNotebook();
                }
                else if (keyboard.escapeKey.wasPressedThisFrame && isOpen)
                {
                    if (artifactDetailModal != null && artifactDetailModal.IsShowing)
                    {
                        artifactDetailModal.Hide();
                    }
                    else
                    {
                        CloseNotebook();
                    }
                }
                else if (isOpen && !isAnimating)
                {
                    if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
                    {
                        OnPreviousPageClicked();
                    }
                    else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
                    {
                        OnNextPageClicked();
                    }
                }
            }
#endif
        }

        private void WireButtons()
        {
            if (hudNotebookButton != null)
            {
                hudNotebookButton.onClick.RemoveAllListeners();
                hudNotebookButton.onClick.AddListener(ToggleNotebook);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseNotebook);
            }

            if (backgroundClickCatcher != null)
            {
                backgroundClickCatcher.onClick.RemoveAllListeners();
                backgroundClickCatcher.onClick.AddListener(CloseNotebook);
            }

            if (previousPageButton != null)
            {
                previousPageButton.onClick.RemoveAllListeners();
                previousPageButton.onClick.AddListener(OnPreviousPageClicked);
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick.RemoveAllListeners();
                nextPageButton.onClick.AddListener(OnNextPageClicked);
            }
        }

        public void InitializeChapters()
        {
            chapters.Clear();
            InitializeStartingCollected();

            // Default chapters matching mockup:
            // MEMORIES: Florist Memory, Romeo & Juliet, Child Memory
            // PRESENT: Museum, Town Square
            string[] defaultMemories = { "Florist Memory", "Romeo & Juliet", "Child Memory" };
            string[] defaultPresent = { "Museum", "Town Square" };

            foreach (var ch in defaultMemories)
            {
                chapters.Add(new ChapterData { Name = ch, Category = "MEMORIES" });
            }
            foreach (var ch in defaultPresent)
            {
                chapters.Add(new ChapterData { Name = ch, Category = "PRESENT" });
            }

            // Distribute artifacts into chapters
            foreach (var art in allArtifacts)
            {
                if (art == null) continue;

                var targetChapter = chapters.FirstOrDefault(c => c.Name.Equals(art.Chapter, StringComparison.OrdinalIgnoreCase));
                if (targetChapter == null)
                {
                    targetChapter = new ChapterData
                    {
                        Name = art.Chapter,
                        Category = art.Category
                    };
                    chapters.Add(targetChapter);
                }

                if (!targetChapter.Artifacts.Contains(art))
                {
                    targetChapter.Artifacts.Add(art);
                }
            }

            // Sort artifacts within each chapter by sortOrder
            foreach (var ch in chapters)
            {
                ch.Artifacts = ch.Artifacts.OrderBy(a => a.SortOrder).ThenBy(a => a.Title).ToList();
            }
        }

        private void InitializeStartingCollected()
        {
            foreach (var art in allArtifacts)
            {
                if (art != null && art.CollectedAtStart)
                {
                    collectedIds.Add(art.Id);
                }
            }
        }

        public bool IsArtifactCollected(string artifactId)
        {
            return collectedIds.Contains(artifactId);
        }

        public bool IsArtifactNew(string artifactId)
        {
            return newIds.Contains(artifactId);
        }

        public void CollectArtifact(ArtifactDefinition artifact)
        {
            if (artifact == null) return;

            bool wasAlreadyCollected = collectedIds.Contains(artifact.Id);
            collectedIds.Add(artifact.Id);
            newIds.Add(artifact.Id);

            // Play notification toast
            ShowCollectionToast($"Artifact Collected: {artifact.Title}!\nAdded to Journal.");

            UpdateHudBadge();

            // If open, refresh view
            if (isOpen)
            {
                RefreshUI();
            }
        }

        public void MarkArtifactReviewed(string artifactId)
        {
            if (newIds.Remove(artifactId))
            {
                UpdateHudBadge();
                RefreshDirectory();
            }
        }

        public void SetWorldHoverHint(string text)
        {
            if (worldHoverText == null) return;

            if (string.IsNullOrEmpty(text))
            {
                worldHoverText.gameObject.SetActive(false);
            }
            else
            {
                worldHoverText.text = text;
                worldHoverText.gameObject.SetActive(true);
            }
        }

        public void ShowCollectionToast(string message)
        {
            if (collectionToastRoot == null) return;

            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }
            toastCoroutine = StartCoroutine(ToastRoutine(message));
        }

        private IEnumerator ToastRoutine(string message)
        {
            collectionToastRoot.SetActive(true);
            if (collectionToastText != null)
            {
                collectionToastText.text = message;
            }

            var cg = collectionToastRoot.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                float fadeIn = 0.2f;
                float elapsed = 0f;
                while (elapsed < fadeIn)
                {
                    elapsed += Time.deltaTime;
                    cg.alpha = Mathf.Clamp01(elapsed / fadeIn);
                    yield return null;
                }
                cg.alpha = 1f;

                yield return new WaitForSeconds(2.5f);

                float fadeOut = 0.35f;
                elapsed = 0f;
                while (elapsed < fadeOut)
                {
                    elapsed += Time.deltaTime;
                    cg.alpha = Mathf.Clamp01(1f - (elapsed / fadeOut));
                    yield return null;
                }
                cg.alpha = 0f;
            }
            else
            {
                yield return new WaitForSeconds(2.8f);
            }

            collectionToastRoot.SetActive(false);
        }

        private void UpdateHudBadge()
        {
            if (hudUnreadBadge == null) return;

            int count = newIds.Count;
            hudUnreadBadge.SetActive(count > 0);
            if (hudUnreadCountText != null)
            {
                hudUnreadCountText.text = count.ToString();
            }
        }

        // ==================== OPEN / CLOSE ANIMATION ====================

        public void ToggleNotebook()
        {
            if (isAnimating) return;

            if (isOpen)
            {
                CloseNotebook();
            }
            else
            {
                OpenNotebook();
            }
        }

        public void OpenNotebook()
        {
            if (isOpen || isAnimating) return;
            StartCoroutine(OpenNotebookRoutine());
        }

        public void CloseNotebook()
        {
            if (!isOpen || isAnimating) return;
            StartCoroutine(CloseNotebookRoutine());
        }

        private IEnumerator OpenNotebookRoutine()
        {
            isAnimating = true;
            isOpen = true;

            if (notebookRoot != null)
            {
                notebookRoot.SetActive(true);
            }

            // Hide open book contents during animation
            if (openBookContent != null)
            {
                openBookContent.alpha = 0f;
                openBookContent.blocksRaycasts = false;
            }

            // Setup animation image
            if (animationImage != null && animationFrames.Count > 0)
            {
                animationImage.gameObject.SetActive(true);
                animationImage.preserveAspect = true;
                animationImage.rectTransform.localScale = Vector3.one * startScale;

                float totalDuration = animationFrames.Count * frameDuration;
                float elapsed = 0f;

                for (int i = 0; i < animationFrames.Count; i++)
                {
                    animationImage.sprite = animationFrames[i];
                    float t = (float)i / Mathf.Max(1, animationFrames.Count - 1);
                    float scale = Mathf.Lerp(startScale, endScale, t);
                    animationImage.rectTransform.localScale = Vector3.one * scale;

                    yield return new WaitForSecondsRealtime(frameDuration);
                }

                animationImage.sprite = animationFrames[animationFrames.Count - 1];
                animationImage.rectTransform.localScale = Vector3.one * endScale;
            }

            // Reveal the open notebook content
            RefreshUI();

            if (openBookContent != null)
            {
                float fadeDuration = 0.15f;
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    openBookContent.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                    yield return null;
                }
                openBookContent.alpha = 1f;
                openBookContent.blocksRaycasts = true;
            }

            if (animationImage != null)
            {
                animationImage.gameObject.SetActive(false);
            }

            isAnimating = false;
        }

        private IEnumerator CloseNotebookRoutine()
        {
            isAnimating = true;

            if (artifactDetailModal != null && artifactDetailModal.IsShowing)
            {
                artifactDetailModal.Hide(immediate: true);
            }

            // Fade out open book contents
            if (openBookContent != null)
            {
                float fadeDuration = 0.12f;
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    openBookContent.alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
                    yield return null;
                }
                openBookContent.alpha = 0f;
                openBookContent.blocksRaycasts = false;
            }

            // Play reverse animation
            if (animationImage != null && animationFrames.Count > 0)
            {
                animationImage.gameObject.SetActive(true);
                animationImage.preserveAspect = true;

                for (int i = animationFrames.Count - 1; i >= 0; i--)
                {
                    animationImage.sprite = animationFrames[i];
                    float t = (float)i / Mathf.Max(1, animationFrames.Count - 1);
                    float scale = Mathf.Lerp(startScale, endScale, t);
                    animationImage.rectTransform.localScale = Vector3.one * scale;

                    yield return new WaitForSecondsRealtime(frameDuration * 0.8f);
                }

                animationImage.gameObject.SetActive(false);
            }

            if (notebookRoot != null)
            {
                notebookRoot.SetActive(false);
            }

            isOpen = false;
            isAnimating = false;
        }

        // ==================== UI RENDERING ====================

        [Header("Page Turn Animation")]
        [SerializeField] private float pageTurnDuration = 0.16f;
        [SerializeField] private float pageTurnDistance = 38f;

        private Coroutine pageTurnCoroutine;

        public void SelectChapter(int chapterIndex)
        {
            if (chapterIndex < 0 || chapterIndex >= chapters.Count) return;

            int oldIndex = selectedChapterIndex;
            selectedChapterIndex = chapterIndex;
            currentCardPageIndex = 0;

            if (isOpen && gameObject.activeInHierarchy)
            {
                float dir = chapterIndex > oldIndex ? 1f : -1f;
                TransitionPage(dir, updateDirectory: true);
            }
            else
            {
                RefreshUI();
            }
        }

        public void OnPreviousPageClicked()
        {
            if (isAnimating) return;

            if (currentCardPageIndex > 0)
            {
                currentCardPageIndex--;
                TransitionPage(-1f, updateDirectory: false);
            }
            else if (selectedChapterIndex > 0)
            {
                selectedChapterIndex--;
                // Go to last page of previous chapter
                var prevChapter = chapters[selectedChapterIndex];
                var collected = prevChapter.Artifacts.Where(a => collectedIds.Contains(a.Id)).ToList();
                int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)collected.Count / CardsPerPage));
                currentCardPageIndex = totalPages - 1;
                TransitionPage(-1f, updateDirectory: true);
            }
        }

        public void OnNextPageClicked()
        {
            if (isAnimating) return;

            var curChapter = chapters[selectedChapterIndex];
            var collected = curChapter.Artifacts.Where(a => collectedIds.Contains(a.Id)).ToList();
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)collected.Count / CardsPerPage));

            if (currentCardPageIndex < totalPages - 1)
            {
                currentCardPageIndex++;
                TransitionPage(1f, updateDirectory: false);
            }
            else if (selectedChapterIndex < chapters.Count - 1)
            {
                selectedChapterIndex++;
                currentCardPageIndex = 0;
                TransitionPage(1f, updateDirectory: true);
            }
        }

        private void TransitionPage(float direction, bool updateDirectory)
        {
            if (updateDirectory)
            {
                RefreshDirectory();
            }

            if (!isOpen || !gameObject.activeInHierarchy || artifactCardsContainer == null)
            {
                RefreshCards();
                return;
            }

            if (pageTurnCoroutine != null)
            {
                StopCoroutine(pageTurnCoroutine);
            }
            pageTurnCoroutine = StartCoroutine(PageFlipRoutine(direction));
        }

        private IEnumerator PageFlipRoutine(float direction)
        {
            isAnimating = true;

            RectTransform cardsRect = artifactCardsContainer as RectTransform;
            CanvasGroup cardsCg = artifactCardsContainer.GetComponent<CanvasGroup>();
            if (cardsCg == null)
            {
                cardsCg = artifactCardsContainer.gameObject.AddComponent<CanvasGroup>();
            }

            Vector2 originalPos = cardsRect != null ? cardsRect.anchoredPosition : Vector2.zero;

            // Phase 1: Slide out
            float elapsed = 0f;
            float halfDuration = pageTurnDuration * 0.5f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float ease = t * t;
                cardsCg.alpha = 1f - ease;
                if (cardsRect != null)
                {
                    cardsRect.anchoredPosition = originalPos + new Vector2(-direction * pageTurnDistance * ease, 0f);
                }
                yield return null;
            }

            // Repopulate cards
            RefreshCards();

            // Phase 2: Slide in from opposite side
            elapsed = 0f;
            if (cardsRect != null)
            {
                cardsRect.anchoredPosition = originalPos + new Vector2(direction * pageTurnDistance, 0f);
            }
            cardsCg.alpha = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float ease = 1f - Mathf.Pow(1f - t, 2f);
                cardsCg.alpha = ease;
                if (cardsRect != null)
                {
                    cardsRect.anchoredPosition = Vector2.Lerp(originalPos + new Vector2(direction * pageTurnDistance, 0f), originalPos, ease);
                }
                yield return null;
            }

            if (cardsRect != null)
            {
                cardsRect.anchoredPosition = originalPos;
            }
            cardsCg.alpha = 1f;

            isAnimating = false;
        }

        public void RefreshUI()
        {
            RefreshDirectory();
            RefreshCards();
        }

        private void ClearChildren(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private void RefreshDirectory()
        {
            if (directoryContainer == null || chapterRowPrefab == null) return;

            // Clear previous rows
            ClearChildren(directoryContainer);

            // Group chapters by Category
            var categories = chapters.GroupBy(c => c.Category);

            foreach (var catGroup in categories)
            {
                // Category header label
                GameObject headerObj = new GameObject("Category_" + catGroup.Key, typeof(RectTransform), typeof(TextMeshProUGUI));
                headerObj.transform.SetParent(directoryContainer, false);
                var headerText = headerObj.GetComponent<TextMeshProUGUI>();
                headerText.text = catGroup.Key;
                headerText.fontSize = 18f;
                headerText.fontStyle = FontStyles.Bold;
                headerText.color = new Color(0.46f, 0.43f, 0.40f, 1f); // Dark muted grey/brown
                headerText.alignment = TextAlignmentOptions.Left;
                var headerLe = headerObj.AddComponent<LayoutElement>();
                headerLe.preferredHeight = 28f;

                foreach (var chapter in catGroup)
                {
                    int chIndex = chapters.IndexOf(chapter);
                    GameObject rowObj = Instantiate(chapterRowPrefab, directoryContainer);
                    rowObj.name = "ChapterRow_" + chapter.Name;

                    var rowView = rowObj.GetComponent<NotebookJournalChapterRow>();
                    if (rowView != null)
                    {
                        int collected = chapter.Artifacts.Count(a => collectedIds.Contains(a.Id));
                        int total = chapter.Artifacts.Count;
                        bool hasNew = chapter.Artifacts.Any(a => newIds.Contains(a.Id));
                        bool isSelected = (chIndex == selectedChapterIndex);

                        rowView.Bind(chapter.Name, collected, total, hasNew, isSelected, () => SelectChapter(chIndex));
                    }
                }
            }
        }

        private void RefreshCards()
        {
            if (artifactCardsContainer == null || artifactCardPrefab == null) return;

            ClearChildren(artifactCardsContainer);

            if (selectedChapterIndex < 0 || selectedChapterIndex >= chapters.Count) return;

            var chapter = chapters[selectedChapterIndex];
            var collectedInChapter = chapter.Artifacts.Where(a => collectedIds.Contains(a.Id)).ToList();

            if (currentPageTitleText != null)
            {
                currentPageTitleText.text = "Current Page";
            }

            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)collectedInChapter.Count / CardsPerPage));
            currentCardPageIndex = Mathf.Clamp(currentCardPageIndex, 0, totalPages - 1);

            if (pageIndicatorText != null)
            {
                pageIndicatorText.text = $"Page {currentCardPageIndex + 1} of {totalPages}";
            }

            // Update arrow button interactability
            bool hasPrev = (currentCardPageIndex > 0) || (selectedChapterIndex > 0);
            bool hasNext = (currentCardPageIndex < totalPages - 1) || (selectedChapterIndex < chapters.Count - 1);

            if (previousPageButton != null) previousPageButton.interactable = hasPrev;
            if (nextPageButton != null) nextPageButton.interactable = hasNext;

            if (collectedInChapter.Count == 0)
            {
                if (emptyChapterText != null)
                {
                    emptyChapterText.gameObject.SetActive(true);
                    emptyChapterText.text = $"No artifacts collected in {chapter.Name} yet.\nExplore the world to discover items!";
                }
                return;
            }

            if (emptyChapterText != null)
            {
                emptyChapterText.gameObject.SetActive(false);
            }

            // Show artifacts for this page
            int startIndex = currentCardPageIndex * CardsPerPage;
            int count = Mathf.Min(CardsPerPage, collectedInChapter.Count - startIndex);

            for (int i = 0; i < count; i++)
            {
                var artifact = collectedInChapter[startIndex + i];
                GameObject cardObj = Instantiate(artifactCardPrefab, artifactCardsContainer);
                cardObj.name = "Card_" + artifact.Title;

                var cardView = cardObj.GetComponent<NotebookJournalCardView>();
                if (cardView != null)
                {
                    bool isNew = newIds.Contains(artifact.Id);
                    cardView.Bind(artifact, isNew, 
                        () => MarkArtifactReviewed(artifact.Id),
                        (art) => ShowArtifactDetail(art));
                }
            }
        }

        public NotebookArtifactDetailModal ArtifactDetailModal => artifactDetailModal;

        public void ShowArtifactDetail(ArtifactDefinition artifact)
        {
            if (artifactDetailModal != null && artifact != null)
            {
                artifactDetailModal.Show(artifact);
            }
        }

        public void SetArtifactDetailModal(NotebookArtifactDetailModal modal)
        {
            artifactDetailModal = modal;
        }

        // ==================== EDITOR HELPERS ====================
        public void SetArtifactsList(IEnumerable<ArtifactDefinition> artifacts)
        {
            allArtifacts = artifacts.Where(a => a != null).ToList();
            InitializeChapters();
        }

        public void SetAnimationFrames(IEnumerable<Sprite> frames)
        {
            animationFrames = frames.Where(f => f != null).ToList();
        }
    }
}
