using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookDirectoryCellView : MonoBehaviour
    {
        [SerializeField] private NotebookSection section = NotebookSection.Present;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text newCountText;
        [SerializeField] private GameObject badgeRoot;
        [SerializeField] private GameObject highlightRoot;

        public NotebookSection Section => section;

        public void Configure(
            NotebookSection targetSection,
            Button targetButton,
            TMP_Text targetTitleText,
            TMP_Text targetNewCountText,
            GameObject targetBadgeRoot,
            GameObject targetHighlightRoot)
        {
            section = targetSection;
            button = targetButton;
            titleText = targetTitleText;
            newCountText = targetNewCountText;
            badgeRoot = targetBadgeRoot;
            highlightRoot = targetHighlightRoot;
        }

        public void Bind(string displayName, int newCount, UnityAction onClick)
        {
            if (titleText != null)
            {
                titleText.text = displayName;
            }

            bool hasNew = newCount > 0;

            if (newCountText != null)
            {
                newCountText.text = hasNew ? newCount.ToString() : string.Empty;
            }

            if (badgeRoot != null)
            {
                badgeRoot.SetActive(hasNew);
            }

            if (highlightRoot != null)
            {
                highlightRoot.SetActive(hasNew);
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
    }
}
