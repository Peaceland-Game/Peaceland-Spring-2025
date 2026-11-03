using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalMinigameController : MonoBehaviour
{
    [System.Serializable]
    public class MemoryPageData
    {
        public string memoryTitle = "Memory #1: The Misplaced Letter";
        [TextArea(2, 4)]
        public string memorySummary = "Jovan frantically searched his dining room for his love letter to Ruzica. His cousin Damir found it in the park, questioning him before promising to help keep it hidden.";
        public string slotHint = "[ Drag Selected Reflection Here ]";
        public string[] wordBankChoices = new string[4];
    }

    [Header("Data")]
    [SerializeField] private MemoryPageData[] memoryPages;

    [Header("Prefabs & Resources")]
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private Sprite cardBgSprite;
    [SerializeField] private Sprite slotBgSprite;

    [Header("Scene References")]
    [SerializeField] private DragManager dragManager;
    [SerializeField] private GameObject interactiveBookRoot;

    [Header("Left Page Elements (Interactive)")]
    [SerializeField] private TextMeshPro pageTitleText;
    [SerializeField] private TextMeshPro pageMemoryHeader;
    [SerializeField] private TextMeshPro pageMemorySummary;

    [Header("Right Page Elements (Interactive)")]
    [SerializeField] private TextMeshPro wordBankTitleText;
    [SerializeField] private TextMeshPro wordBankSubtitleText;

    [Header("Navigation UI (Overlay Canvas)")]
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TextMeshProUGUI pageIndicatorText;
    [SerializeField] private Button completeButton;
    [SerializeField] private TextMeshProUGUI completeButtonText;


    // Runtime state
    private int currentPageIndex = 0;
    private int[] chosenChoicePerMemory; // index of chosen wordBankChoice per memory (-1 if unfilled)
    private Draggable[][] spawnedPiecesPerMemory; // 4 pieces per memory page
    private DragTarget[] spawnedTargetsPerMemory; // 1 target per memory page

    private Vector3 targetSlotPos = new Vector3(-2.85f, -1.0f, 0f);
    private Vector3[] wordBankPositions = new Vector3[]
    {
        new Vector3(2.85f, 2.0f, 0f),
        new Vector3(2.85f, 0.5f, 0f),
        new Vector3(2.85f, -1.0f, 0f),
        new Vector3(2.85f, -2.5f, 0f)
    };

    private void Start()
    {
        InitDefaultDataIfEmpty();
        InitState();
        BuildAllPages();
        ShowPage(0);
        UpdateCompleteButtonState();

        if (prevPageButton != null) prevPageButton.onClick.AddListener(OnPrevPageClicked);
        if (nextPageButton != null) nextPageButton.onClick.AddListener(OnNextPageClicked);
        if (completeButton != null) completeButton.onClick.AddListener(OnCompleteJournalClicked);
    }

    private void InitDefaultDataIfEmpty()
    {
        if (memoryPages != null && memoryPages.Length > 0) return;

        memoryPages = new MemoryPageData[7];

        // Memory 1
        memoryPages[0] = new MemoryPageData
        {
            memoryTitle = "Reflection 1",
            memorySummary = "I met Jovan and Ruzica, a Thebrean and Eskani couple respectively. They told me their story of escaping their neighborhoods together and running away to a mixed city.  ",
            slotHint = "[ Drag Selected Reflection Here ]",
            wordBankChoices = new string[]
            {
                "Jovan clearly put Damir in unnecessary danger in order to see Ruzica.",
                "Jovan was trying to save Ruzica from potential danger.",
                "Jovan made a mistake, it's difficult to make good decisions during civil unrest.",
                "Jovan didn't consider Damir's safety, he put his own family in danger for a girl."
            }
        };

        // Memory 2
        memoryPages[1] = new MemoryPageData
        {
            memoryTitle = "Reflection 2",
            memorySummary = "With civil unrest growing and patrols roaming the city streets, Jovan snuck to Ruzica's home. Knowing they were no longer safe, the lovers resolved to abandon everything and escape.",
            slotHint = "[ Drag Selected Reflection Here ]",
            wordBankChoices = new string[]
            {
                "It was smart of Jovan and Ruzica to leave, they knew they weren't safe there.",
                "Jovan and Ruzica should have planned an escape before they took drastic action.",
                "Maybe Jovan's family would have been able to help if he was honest with them.",
                "Forbidden love gets people in trouble, Jovan shouldn't have been there."
            }
        };

        // Memory 3
        memoryPages[2] = new MemoryPageData
        {
            memoryTitle = "Reflection 3",
            memorySummary = "Once they escaped they learned that Damir was chased back to the Thebrean neighborhood and their house was burned down and Damir was killed.",
            slotHint = "[ Drag Selected Reflection Here ]",
            wordBankChoices = new string[]
            {
                "Jovan was the one to blame for Damir's death.",
                "Ruzica should never have asked Jovan to put himself in danger, she is responsible.",
                "Neither of them were thinking clearly and someone needed a level head.",
                "They did what they could to survive, they were both in serious danger and had to escape."
            }
        };

        memoryPages[3] = new MemoryPageData
        {
            memoryTitle = "Reflection 4",
            memorySummary = "When I talked to them about it after going through the memory they seemed remorseful about Damir. ",
            slotHint = "[ Drag Selected Reflection Here ]",
            wordBankChoices = new string[]
            {
                "Jovan and Ruzica are a good example of how life can be better in a world at peace.",
                "Jovan and Ruzica put themselves and their families in danger unnecessarily. "
            }
        };

        memoryPages[4] = new MemoryPageData
        {
            memoryTitle = "Reflection 5",
            memorySummary = "I went to Dalila’s after work for a bite to eat.",
            slotHint = "[ Drag Selected Reflection Here ]",
            wordBankChoices = new string[]
            {
                "Dalila was quick to judge Jovan just because he is Thebrean.",
                "Dalila made a good point about Jovan’s lack of care for family.",
                "Dalila is right about Jovan, the Thebreans shouldn’t have been that stupid."
            }
        };

        memoryPages[5] = new MemoryPageData
        {
            memoryTitle = "Reflection 6",
            memorySummary = "After that I met Katarina in the square.",
            slotHint = "[ Drag Selected Reflection Here ]",
            wordBankChoices = new string[]
            {
                "Katarina is justifiably skeptical of this upcoming election.",
                "Biases like Katarina’s are the reason wars start in the first place when people can’t agree.",
                "I didn't meet with them."
            }
        };

        memoryPages[6] = new MemoryPageData
        {
            memoryTitle = "Reflection 7",
            memorySummary = "And then I came home and spoke to Art.",
            slotHint = "[ Drag Selected Reflection Here ]",
            wordBankChoices = new string[]
            {
                "Art is immature and doesn’t understand the scale of these events.",
                "I’ll have to help Art better understand this.",
                "Art is dangerous, his indifference about what happened to Damir is scary.",
                "Art is understandably skeptical of people who don’t agree."
            }
        };

    }

    private void InitState()
    {
        chosenChoicePerMemory = new int[memoryPages.Length];
        for (int i = 0; i < chosenChoicePerMemory.Length; i++) chosenChoicePerMemory[i] = -1;

        spawnedPiecesPerMemory = new Draggable[memoryPages.Length][];
        spawnedTargetsPerMemory = new DragTarget[memoryPages.Length];
    }

    private void BuildAllPages()
    {
        if (dragManager == null) return;

        List<Draggable> allDraggables = new List<Draggable>();

        for (int page = 0; page < memoryPages.Length; page++)
        {
            int pageMemId = page + 1; // 1, 2, 3
            var pageData = memoryPages[page];

            // 1. Create Target Slot for this page
            GameObject targetObj = Instantiate(targetPrefab, dragManager.transform);
            targetObj.name = "TargetSlot_Page_" + page;
            targetObj.transform.localPosition = targetSlotPos;
            var dt = targetObj.GetComponent<DragTarget>();
            dt.Constructor(pageMemId, slotBgSprite);
            var targetTmp = targetObj.GetComponentInChildren<TextMeshPro>();
            if (targetTmp != null) targetTmp.text = pageData.slotHint;
            if (targetTmp != null)
            {
                targetTmp.text = pageData.slotHint;
                targetTmp.fontSize = 3f;
            }
            spawnedTargetsPerMemory[page] = dt;

            // 2. Create 4 Word Bank Pieces for this page
            spawnedPiecesPerMemory[page] = new Draggable[pageData.wordBankChoices.Length];
            for (int c = 0; c < pageData.wordBankChoices.Length; c++)
            {
                GameObject pieceObj = Instantiate(piecePrefab, dragManager.transform);
                pieceObj.name = string.Format("Piece_Page_{0}_Choice_{1}", page, c);
                pieceObj.transform.localPosition = wordBankPositions[c];

                Draggable draggable = pieceObj.GetComponent<Draggable>();
                draggable.dm = dragManager;

                draggable.EnableDrag();
                draggable.Constructor(new GameObject[] { targetObj }, pageMemId, cardBgSprite);

                var pieceTmp = pieceObj.GetComponentInChildren<TextMeshPro>();
                if (pieceTmp != null) pieceTmp.text = pageData.wordBankChoices[c];

                // Hook up snap / unsnap detection
                int capturedPage = page;
                int capturedChoice = c;
                HookPieceEvents(draggable, capturedPage, capturedChoice);
                // line 184, word bank cards

                if (pieceTmp != null)
                {
                    pieceTmp.text = pageData.wordBankChoices[c];
                    pieceTmp.fontSize = 2.7f;
                }

                spawnedPiecesPerMemory[page][c] = draggable;
                allDraggables.Add(draggable);
            }
        }

        dragManager.draggables = allDraggables.ToArray();
    }

    private void HookPieceEvents(Draggable piece, int page, int choiceIndex)
    {
        // When piece snaps to target
    }

    private void Update()
    {
        for (int page = 0; page < memoryPages.Length; page++)
        {
            var target = spawnedTargetsPerMemory[page];
            if (target == null) continue;

            // Find the card sitting in this page's slot, if any
            int foundSnappedChoice = -1;
            float closestDist = 0.5f;
            for (int c = 0; c < spawnedPiecesPerMemory[page].Length; c++)
            {
                var piece = spawnedPiecesPerMemory[page][c];
                if (piece == null) continue;

                float dist = Vector3.Distance(piece.transform.position, target.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    foundSnappedChoice = c;
                }
            }

            if (foundSnappedChoice != chosenChoicePerMemory[page])
            {
                chosenChoicePerMemory[page] = foundSnappedChoice;
                target.isSnapped = (foundSnappedChoice != -1);
                target.currentSnappedPiece = (foundSnappedChoice != -1)
                    ? spawnedPiecesPerMemory[page][foundSnappedChoice]
                    : null;
                UpdateCompleteButtonState();
                UpdateNextButtonState();
            }
        }
    }

    public void ShowPage(int pageIndex)
    {
        currentPageIndex = Mathf.Clamp(pageIndex, 0, memoryPages.Length - 1);

        // Update page text
        var pageData = memoryPages[currentPageIndex];
        if (pageTitleText != null) pageTitleText.text = "JOURNAL REFLECTIONS";
        if (pageMemoryHeader != null) pageMemoryHeader.text = pageData.memoryTitle;
        if (pageMemorySummary != null) pageMemorySummary.text = pageData.memorySummary;

        if (wordBankTitleText != null) wordBankTitleText.text = "REFLECTION CHOICES";
        if (wordBankSubtitleText != null)
            wordBankSubtitleText.text = string.Format("Choose one reflection sentence:", currentPageIndex + 1);

        // Update navigation buttons
        if (prevPageButton != null)
        {
            prevPageButton.gameObject.SetActive(currentPageIndex > 0);
            prevPageButton.interactable = (currentPageIndex > 0);
        }
        UpdateNextButtonState();
        if (pageIndicatorText != null)
            pageIndicatorText.text = string.Format("Page {0} of {1}", currentPageIndex + 1, memoryPages.Length);

        // Show/Hide page elements
        for (int p = 0; p < memoryPages.Length; p++)
        {
            bool isActivePage = (p == currentPageIndex);

            if (spawnedTargetsPerMemory[p] != null)
            {
                spawnedTargetsPerMemory[p].gameObject.SetActive(isActivePage);
            }

            if (spawnedPiecesPerMemory[p] != null)
            {
                for (int c = 0; c < spawnedPiecesPerMemory[p].Length; c++)
                {
                    var piece = spawnedPiecesPerMemory[p][c];
                    if (piece != null)
                    {
                        piece.gameObject.SetActive(isActivePage);
                    }
                }
            }
        }
    }

    public void SnapChoiceForMemory(int memoryIndex, int choiceIndex)
    {
        if (memoryIndex < 0 || memoryIndex >= memoryPages.Length) return;
        if (choiceIndex < 0 || choiceIndex >= memoryPages[memoryIndex].wordBankChoices.Length) return;

        chosenChoicePerMemory[memoryIndex] = choiceIndex;
        var target = spawnedTargetsPerMemory[memoryIndex];
        var piece = spawnedPiecesPerMemory[memoryIndex][choiceIndex];

        if (target != null && piece != null)
        {
            target.SnapPiece(piece);
            piece.transform.position = target.transform.position;
        }

        UpdateCompleteButtonState();
    }

    public void OnPrevPageClicked()
    {
        if (currentPageIndex > 0)
        {
            ShowPage(currentPageIndex - 1);
        }
    }

    public void OnNextPageClicked()
    {
        if (currentPageIndex < memoryPages.Length - 1)
        {
            ShowPage(currentPageIndex + 1);
        }
    }

    private bool AreAllMemoriesCompleted()
    {
        for (int i = 0; i < memoryPages.Length; i++)
        {
            if (chosenChoicePerMemory[i] == -1) return false;
        }
        return true;
    }

    private void UpdateCompleteButtonState()
    {
        bool allDone = AreAllMemoriesCompleted();
        if (completeButton != null)
        {
            completeButton.gameObject.SetActive(allDone);
            completeButton.interactable = allDone;
        }
    }

    private void UpdateNextButtonState()
    {
        if (nextPageButton == null) return;

        bool hasNextPage = currentPageIndex < memoryPages.Length - 1;
        bool currentPageAnswered = chosenChoicePerMemory[currentPageIndex] != -1;
        bool show = hasNextPage && currentPageAnswered;

        nextPageButton.gameObject.SetActive(show);
        nextPageButton.interactable = show;
    }

    public void OnCompleteJournalClicked()
    {
        Debug.Log(string.Format("[Journal] Complete clicked: chosen=[{0}, {1}, {2}], allDone={3}",
            chosenChoicePerMemory != null && chosenChoicePerMemory.Length > 0 ? chosenChoicePerMemory[0] : -1,
            chosenChoicePerMemory != null && chosenChoicePerMemory.Length > 1 ? chosenChoicePerMemory[1] : -1,
            chosenChoicePerMemory != null && chosenChoicePerMemory.Length > 2 ? chosenChoicePerMemory[2] : -1,
            AreAllMemoriesCompleted()));

        if (!AreAllMemoriesCompleted()) return;

        // Hide interactive book page and navigation UI
        if (interactiveBookRoot != null) interactiveBookRoot.SetActive(false);
        if (dragManager != null) dragManager.gameObject.SetActive(false);
        if (prevPageButton != null) prevPageButton.gameObject.SetActive(false);
        if (nextPageButton != null) nextPageButton.gameObject.SetActive(false);
        if (pageIndicatorText != null) pageIndicatorText.gameObject.SetActive(false);
        if (completeButton != null) completeButton.gameObject.SetActive(false);
        OnFinalContinueClicked();
    }

   

    public void OnFinalContinueClicked()
    {
        //Debug.Log("Final Journal Continue Clicked! Advancing to next story segment...");
        //if (GameManager.Instance != null && GameManager.Instance.CurrentMemoryManager != null)
        //{
        //    GameManager.Instance.CurrentMemoryManager.NextMinigame();
        //}
        LevelLoader LL = FindFirstObjectByType<LevelLoader>();
        LL.LoadLevelByBuildIndex(F26_GameManager.EndingBuildIndex, true);
    }
}
