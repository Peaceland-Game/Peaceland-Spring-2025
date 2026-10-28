using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class LetterWordButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI wordLabel;
    [SerializeField] private RectTransform cardRoot;

    private WordChoice currentChoice;
    private Action<WordChoice> onSelectedCallback;
    private Vector3 originalScale = Vector3.one;
    private float targetScale = 1f;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (button == null) button = GetComponent<Button>();
        if (cardRoot == null) cardRoot = GetComponent<RectTransform>();
        if (cardRoot != null && originalScale == Vector3.one)
        {
            originalScale = cardRoot.localScale;
        }
    }

    public void Setup(WordChoice choice, Action<WordChoice> onSelected)
    {
        EnsureInitialized();
        currentChoice = choice;
        onSelectedCallback = onSelected;

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
            button.onClick.AddListener(HandleClicked);
        }

        if (wordLabel != null && choice != null)
        {
            wordLabel.text = choice.word;
        }

        targetScale = 1f;
        if (cardRoot != null)
        {
            cardRoot.localScale = originalScale;
        }
    }

    // Overload for backwards compatibility
    public void Setup(WordChoice choice, Action<WordChoice> onSelected, Sprite moodSprite, Sprite pushpinSprite)
    {
        Setup(choice, onSelected);
    }

    private void Update()
    {
        if (cardRoot != null)
        {
            cardRoot.localScale = Vector3.Lerp(cardRoot.localScale, originalScale * targetScale, Time.unscaledDeltaTime * 15f);
        }
    }

    private void HandleClicked()
    {
        targetScale = 1.0f;
        if (currentChoice != null && onSelectedCallback != null)
        {
            onSelectedCallback.Invoke(currentChoice);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = 1.04f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = 1.0f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Tactile touch & click feedback
        targetScale = 0.94f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // On touch release, always return to normal scale (prevents sticky hover on mobile)
        targetScale = 1.0f;
    }
}
