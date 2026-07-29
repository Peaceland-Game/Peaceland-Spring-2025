using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookDirectoryLineView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private NotebookSection section = NotebookSection.Present;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text newCountText;
        [SerializeField] private Graphic hoverTarget;
        [SerializeField] private Color normalColor = new Color(0.19f, 0.15f, 0.11f, 1f);
        [SerializeField] private Color hoverColor = new Color(0.35f, 0.22f, 0.12f, 1f);
        [SerializeField] private Color newAccentColor = new Color(0.55f, 0.12f, 0.1f, 1f);

        public NotebookSection Section => section;

        private void Reset()
        {
            button = GetComponent<Button>();
            labelText = GetComponentInChildren<TMP_Text>();
            hoverTarget = labelText;
        }

        public void Configure(NotebookSection targetSection, TMP_Text targetLabel, TMP_Text targetNewCount = null, Button targetButton = null)
        {
            section = targetSection;
            labelText = targetLabel;
            newCountText = targetNewCount;
            button = targetButton != null ? targetButton : button;
            if (hoverTarget == null)
            {
                hoverTarget = labelText;
            }
        }

        public void Bind(string displayName, int newCount, UnityAction onClick)
        {
            if (labelText != null)
            {
                labelText.text = displayName;
                labelText.color = newCount > 0 ? newAccentColor : normalColor;
            }

            if (newCountText != null)
            {
                bool hasNew = newCount > 0;
                newCountText.gameObject.SetActive(hasNew);
                newCountText.text = hasNew ? "· " + newCount : string.Empty;
            }

            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (labelText != null)
            {
                labelText.color = hoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (labelText == null)
            {
                return;
            }

            bool hasNew = newCountText != null && newCountText.gameObject.activeSelf;
            labelText.color = hasNew ? newAccentColor : normalColor;
        }
    }
}
