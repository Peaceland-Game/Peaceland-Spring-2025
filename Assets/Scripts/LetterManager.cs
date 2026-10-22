using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LetterManager : MinigameBehavior
{
    [Header("Letter Content")]
    [SerializeField] private LetterData currentLetter;
    [SerializeField] private TextMeshProUGUI letterBodyText;
    [SerializeField] private RectTransform letterPaperRect;
    [SerializeField] private RectTransform paperSealRect;

    [Header("Word Bank")]
    [SerializeField] private RectTransform wordBankPanel;
    [SerializeField] private Transform wordButtonsContainer;
    [SerializeField] private GameObject wordButtonPrefab;
    [SerializeField] private TextMeshProUGUI blankProgressText;

    [Header("Seal & Results")]
    [SerializeField] private Button sealButton;
    [SerializeField] private RectTransform sealButtonRect;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultTitle;
    [SerializeField] private TextMeshProUGUI resultDesc;
    [SerializeField] private Button rewriteButton;
    [SerializeField] private Button closeButton;

    // Runtime state
    private int currentBlankIndex = 0;
    private string[] filledWords;
    private bool isStamping = false;
    private Coroutine sealAnimationCoroutine;

    private void Awake()
    {
        if (sealButton != null)
        {
            sealButton.onClick.AddListener(OnSealButtonClicked);
        }

        if (rewriteButton != null)
        {
            rewriteButton.onClick.AddListener(ResetMinigame);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseResults);
        }
    }

    private void Start()
    {
        StartMinigame();
    }

    public override void StartMinigame()
    {
        if (currentLetter == null)
        {
            Debug.LogWarning("LetterManager: No currentLetter assigned!");
            return;
        }

        ResetMinigame();
    }

    public override void StopMinigame()
    {
        // Cleanup if minigame stops
    }

    public void ResetMinigame()
    {
        isStamping = false;
        if (sealAnimationCoroutine != null)
        {
            StopCoroutine(sealAnimationCoroutine);
            sealAnimationCoroutine = null;
        }

        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (sealButton != null) sealButton.gameObject.SetActive(false);
        if (paperSealRect != null) paperSealRect.gameObject.SetActive(false);

        int blankCount = currentLetter != null ? currentLetter.BlankCount : 0;
        filledWords = new string[blankCount];
        currentBlankIndex = 0;

        UpdateLetterDisplay();
        LoadCurrentWordBank();
    }

    private void LoadCurrentWordBank()
    {
        // Clear previous buttons
        if (wordButtonsContainer != null)
        {
            for (int i = wordButtonsContainer.childCount - 1; i >= 0; i--)
            {
                GameObject child = wordButtonsContainer.GetChild(i).gameObject;
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

        if (currentLetter == null) return;

        int totalBlanks = currentLetter.BlankCount;

        if (currentBlankIndex >= totalBlanks)
        {
            // All blanks filled!
            if (blankProgressText != null)
            {
                blankProgressText.text = "Letter Complete!\nTap the wax seal to stamp your letter.";
            }

            if (sealButton != null)
            {
                sealButton.gameObject.SetActive(true);
            }
            return;
        }

        if (blankProgressText != null)
        {
            blankProgressText.text = $"Word Bank — Blank {currentBlankIndex + 1} of {totalBlanks}";
        }

        BlankData blank = currentLetter.GetBlank(currentBlankIndex);
        if (blank == null || blank.choices == null || blank.choices.Count == 0)
        {
            // Fallback for simple letters
            CreateFallbackChoices();
            return;
        }

        foreach (var choice in blank.choices)
        {
            CreateWordButton(choice);
        }
    }

    private void CreateFallbackChoices()
    {
        var choices = new List<WordChoice>
        {
            new WordChoice("smiled at me", LetterMoodType.Affectionate),
            new WordChoice("looked away", LetterMoodType.Melancholic),
            new WordChoice("went pale", LetterMoodType.Somber),
            new WordChoice("laughed aloud", LetterMoodType.Passionate)
        };

        foreach (var choice in choices)
        {
            CreateWordButton(choice);
        }
    }

    private void CreateWordButton(WordChoice choice)
    {
        if (wordButtonPrefab == null || wordButtonsContainer == null) return;

        GameObject btnObj = Instantiate(wordButtonPrefab, wordButtonsContainer);
        LetterWordButton btnScript = btnObj.GetComponent<LetterWordButton>();

        if (btnScript != null)
        {
            btnScript.Setup(choice, SelectWord);
        }
        else
        {
            // Fallback: configure directly
            Button btn = btnObj.GetComponent<Button>();
            TMP_Text tmp = btnObj.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = choice.word;
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectWord(choice));
            }
        }
    }

    public void SelectWord(WordChoice choice)
    {
        if (currentBlankIndex >= filledWords.Length) return;

        // Fill blank
        filledWords[currentBlankIndex] = choice.word;

        // Advance to next blank
        currentBlankIndex++;

        UpdateLetterDisplay();
        LoadCurrentWordBank();
    }

   

    private void UpdateLetterDisplay()
    {
        if (letterBodyText == null || currentLetter == null) return;

        string[] parts = currentLetter.textParts;
        if (parts == null || parts.Length == 0) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int blankCount = currentLetter.BlankCount;

        for (int i = 0; i < parts.Length; i++)
        {
            sb.Append(parts[i]);

            if (i < blankCount)
            {
                // Blank i
                if (!string.IsNullOrEmpty(filledWords[i]))
                {
                    
                    sb.Append($"<b><u><color=#153B6B>{filledWords[i]}</color></u></b>");
                }
                else if (i == currentBlankIndex)
                {
                    // Active blank 
                    sb.Append("<color=#A03E15><b><u>\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0</u></b></color>");
                }
                else
                {
                    // Future blank 
                    sb.Append("<color=#7A6C5E><u>\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0</u></color>");
                }
            }
        }

        letterBodyText.text = sb.ToString();
    }

    private void OnSealButtonClicked()
    {
        if (isStamping) return;
        if (sealAnimationCoroutine != null) StopCoroutine(sealAnimationCoroutine);
        ResetMinigame();
    }

    public void CloseResults()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }
    }
}


