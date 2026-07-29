using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookIndexEntryView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text pageNumberText;
        [SerializeField] private GameObject newBadgeRoot;
        [SerializeField] private GameObject highlightRoot;

        public void Configure(
            Button targetButton,
            TMP_Text targetTitleText,
            TMP_Text targetPageNumberText,
            GameObject targetNewBadgeRoot,
            GameObject targetHighlightRoot)
        {
            button = targetButton;
            titleText = targetTitleText;
            pageNumberText = targetPageNumberText;
            newBadgeRoot = targetNewBadgeRoot;
            highlightRoot = targetHighlightRoot;
        }

        public void Bind(string title, int pageNumber, bool isNew, UnityAction onClick)
        {
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (pageNumberText != null)
            {
                pageNumberText.text = pageNumber > 0 ? pageNumber.ToString() : string.Empty;
            }

            if (newBadgeRoot != null)
            {
                newBadgeRoot.SetActive(isNew);
            }

            if (highlightRoot != null)
            {
                highlightRoot.SetActive(isNew);
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

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
