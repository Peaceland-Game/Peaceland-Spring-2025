using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Editor-wired UI references for the open notebook shell. Assign on Notebook Open Root prefab.
    /// </summary>
    public class NotebookUIShellReferences : MonoBehaviour
    {
        [Header("Shell")]
        [SerializeField] private RectTransform openRoot;
        [SerializeField] private Image bookBackground;
        [SerializeField] private RectTransform pagesViewport;
        [SerializeField] private NotebookAnimationView animationView;
        [SerializeField] private NotebookBookmarkTabBar bookmarkTabBar;

        [Header("Spread Roots")]
        [SerializeField] private GameObject directoryPageRoot;
        [SerializeField] private GameObject sectionPageRoot;

        [Header("Directory Spread")]
        [SerializeField] private List<NotebookDirectoryLineView> directoryLines = new List<NotebookDirectoryLineView>();
        [SerializeField] private Transform testToolsAnchor;

        [Header("Section Spread")]
        [SerializeField] private GameObject indexRoot;
        [SerializeField] private Transform indexLeftContainer;
        [SerializeField] private Transform indexRightContainer;
        [SerializeField] private NotebookIndexEntryView indexEntryTemplate;
        [SerializeField] private GameObject contentPagesRoot;
        [SerializeField] private Transform contentLeftContainer;
        [SerializeField] private Transform contentRightContainer;
        [SerializeField] private NotebookEntryView entryTemplate;

        [Header("Chrome")]
        [SerializeField] private TMP_Text leftPageNumberText;
        [SerializeField] private TMP_Text rightPageNumberText;
        [SerializeField] private Button previousSpreadButton;
        [SerializeField] private Button nextSpreadButton;

        [Header("Overlay")]
        [SerializeField] private NotebookOverlayView overlayView;

        public RectTransform OpenRoot => openRoot != null ? openRoot : transform as RectTransform;
        public Image BookBackground => bookBackground;
        public RectTransform PagesViewport => pagesViewport;
        public NotebookAnimationView AnimationView => animationView;
        public NotebookBookmarkTabBar BookmarkTabBar => bookmarkTabBar;
        public GameObject DirectoryPageRoot => directoryPageRoot;
        public GameObject SectionPageRoot => sectionPageRoot;
        public IReadOnlyList<NotebookDirectoryLineView> DirectoryLines => directoryLines;
        public Transform TestToolsAnchor => testToolsAnchor;
        public GameObject IndexRoot => indexRoot;
        public Transform IndexLeftContainer => indexLeftContainer;
        public Transform IndexRightContainer => indexRightContainer;
        public NotebookIndexEntryView IndexEntryTemplate => indexEntryTemplate;
        public GameObject ContentPagesRoot => contentPagesRoot;
        public Transform ContentLeftContainer => contentLeftContainer;
        public Transform ContentRightContainer => contentRightContainer;
        public NotebookEntryView EntryTemplate => entryTemplate;
        public TMP_Text LeftPageNumberText => leftPageNumberText;
        public TMP_Text RightPageNumberText => rightPageNumberText;
        public Button PreviousSpreadButton => previousSpreadButton;
        public Button NextSpreadButton => nextSpreadButton;
        public NotebookOverlayView OverlayView => overlayView;

        private void Reset()
        {
            openRoot = transform as RectTransform;
        }

        public void ApplyToController(NotebookController controller)
        {
            if (controller == null)
            {
                return;
            }

            controller.ApplyShellReferences(this);
        }

        /// <summary>
        /// Hides everything on the open-book shell except the opening animation overlay.
        /// </summary>
        public void SetBookChromeVisible(bool visible)
        {
            SetObjectActive(bookBackground != null ? bookBackground.gameObject : null, visible);
            SetObjectActive(pagesViewport != null ? pagesViewport.gameObject : null, visible);
            SetObjectActive(directoryPageRoot, visible);
            SetObjectActive(sectionPageRoot, visible);
            SetObjectActive(previousSpreadButton != null ? previousSpreadButton.gameObject : null, visible);
            SetObjectActive(nextSpreadButton != null ? nextSpreadButton.gameObject : null, visible);
            SetObjectActive(bookmarkTabBar != null ? bookmarkTabBar.gameObject : null, visible);

            Transform root = OpenRoot;
            if (root == null)
            {
                return;
            }

            Transform leftRail = root.Find("Bookmark Tabs Left");
            Transform rightRail = root.Find("Bookmark Tabs Right");
            SetObjectActive(leftRail != null ? leftRail.gameObject : null, visible);
            SetObjectActive(rightRail != null ? rightRail.gameObject : null, visible);
        }

        private static void SetObjectActive(GameObject target, bool visible)
        {
            if (target != null)
            {
                target.SetActive(visible);
            }
        }
    }
}
