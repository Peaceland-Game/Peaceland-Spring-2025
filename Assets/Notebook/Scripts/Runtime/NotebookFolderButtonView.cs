using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookFolderButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text badgeText;
        [SerializeField] private GameObject badgeRoot;
        [SerializeField] private GameObject highlightRoot;

        public void Configure(
            Button targetButton,
            TMP_Text targetTitleText,
            TMP_Text targetSubtitleText,
            TMP_Text targetBadgeText,
            GameObject targetBadgeRoot,
            GameObject targetHighlightRoot)
        {
            button = targetButton;
            titleText = targetTitleText;
            subtitleText = targetSubtitleText;
            badgeText = targetBadgeText;
            badgeRoot = targetBadgeRoot;
            highlightRoot = targetHighlightRoot;
        }

        public void Bind(string title, string subtitle, int newCount, UnityAction onClick)
        {
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (subtitleText != null)
            {
                subtitleText.text = subtitle;
                subtitleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(subtitle));
            }

            bool hasNew = newCount > 0;

            if (badgeText != null)
            {
                badgeText.text = hasNew ? newCount.ToString() : string.Empty;
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

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
