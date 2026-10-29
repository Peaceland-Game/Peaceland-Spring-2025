using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookJournalChapterRow : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Button rowButton;
        [SerializeField] private UnityEngine.UI.Image backgroundImage;
        [SerializeField] private TMP_Text chapterNameText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private GameObject countContainer;
        [SerializeField] private GameObject newDot;

        [Header("Visual Styles")]
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Color normalTextColor = new Color(0.2f, 0.18f, 0.16f, 1f);
        [SerializeField] private Color selectedTextColor = new Color(0.15f, 0.12f, 0.08f, 1f);

        public void Bind(string chapterName, int collected, int total, bool hasNew, bool isSelected, Action onClick)
        {
            if (chapterNameText != null)
            {
                chapterNameText.text = chapterName;
                chapterNameText.color = isSelected ? selectedTextColor : normalTextColor;
            }

            if (countText != null)
            {
                countText.text = $"{collected:D2}/{total:D2}";
            }

            if (newDot != null)
            {
                newDot.SetActive(hasNew);
            }

            if (backgroundImage != null)
            {
                if (isSelected && selectedSprite != null)
                {
                    backgroundImage.sprite = selectedSprite;
                }
                else if (normalSprite != null)
                {
                    backgroundImage.sprite = normalSprite;
                }
            }

            if (rowButton != null)
            {
                rowButton.onClick.RemoveAllListeners();
                if (onClick != null)
                {
                    rowButton.onClick.AddListener(() => onClick());
                }
            }
        }
    }
}
