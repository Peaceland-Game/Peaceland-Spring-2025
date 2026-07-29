using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookBookmarkTabView : MonoBehaviour
    {
        [SerializeField] private NotebookSection section = NotebookSection.Directory;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor = new Color(0.93f, 0.86f, 0.74f, 1f);
        [SerializeField] private Color activeColor = new Color(0.98f, 0.92f, 0.78f, 1f);
        [SerializeField] private Color hoverColor = new Color(0.96f, 0.89f, 0.76f, 1f);

        public NotebookSection Section => section;
        public Button Button => button;

        private bool isActive;

        private void Reset()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            labelText = GetComponentInChildren<TMP_Text>();
        }

        public void Configure(NotebookSection targetSection, string label, Button targetButton = null, TMP_Text targetLabel = null)
        {
            section = targetSection;
            button = targetButton != null ? targetButton : button;
            labelText = targetLabel != null ? targetLabel : labelText;
            if (labelText != null)
            {
                labelText.text = label;
                labelText.enableWordWrapping = false;
                labelText.overflowMode = TextOverflowModes.Ellipsis;
                labelText.fontSize = Mathf.Min(labelText.fontSize, 13f);
            }
        }

        public void SetActiveVisual(bool active)
        {
            isActive = active;
            ApplyColor(active ? activeColor : normalColor);
        }

        public void SetSide(NotebookBookmarkSide side)
        {
            if (labelText != null)
            {
                labelText.alignment = side == NotebookBookmarkSide.Right
                    ? TextAlignmentOptions.Right
                    : TextAlignmentOptions.Left;
                RectTransform labelRect = labelText.rectTransform;
                labelRect.offsetMin = side == NotebookBookmarkSide.Right
                    ? new Vector2(4f, 2f)
                    : new Vector2(8f, 2f);
                labelRect.offsetMax = side == NotebookBookmarkSide.Right
                    ? new Vector2(-8f, -2f)
                    : new Vector2(-4f, -2f);
            }
        }

        private void ApplyColor(Color color)
        {
            if (background != null)
            {
                background.color = color;
            }
        }

        public void OnHoverEnter()
        {
            if (!isActive)
            {
                ApplyColor(hoverColor);
            }
        }

        public void OnHoverExit()
        {
            SetActiveVisual(isActive);
        }
    }
}
