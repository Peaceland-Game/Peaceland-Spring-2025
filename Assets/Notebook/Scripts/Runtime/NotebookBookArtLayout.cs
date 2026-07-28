using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Marks book shell rects as editor-authored. When lockLayout is true, runtime helpers will not
    /// overwrite Book Background / Open Root / bookmark rail positions you set in the Scene view.
    /// </summary>
    public class NotebookBookArtLayout : MonoBehaviour
    {
        [Header("Layout lock")]
        [Tooltip("When enabled, runtime and OnValidate helpers will not overwrite rects you set by hand.")]
        [SerializeField] private bool lockLayout = true;
        [SerializeField] private bool bookBackgroundBlocksRaycasts;

        [Header("Pagination (set Page Usable Height to match ruled lines on art)")]
        [Tooltip("When > 0, one page column uses this height for pagination instead of measuring the UI container.")]
        [SerializeField] private float pageUsableHeight;
        [Tooltip("Reserved space at top of each column (category headers).")]
        [SerializeField] private float pageTopReservedHeight = 52f;

        [Header("Bookmark tabs")]
        [Tooltip("Distance from top of open book to first bookmark tab.")]
        [SerializeField] private float bookmarkTopInset = 96f;
        [Tooltip("How far tabs stick out past the left/right book edge.")]
        [SerializeField] private float bookmarkHorizontalOffset = 8f;

        public bool LockLayout => lockLayout;
        public bool BookBackgroundBlocksRaycasts => bookBackgroundBlocksRaycasts;
        public float PageUsableHeight => pageUsableHeight;
        public float PageTopReservedHeight => pageTopReservedHeight;
        public float BookmarkTopInset => bookmarkTopInset;
        public float BookmarkHorizontalOffset => bookmarkHorizontalOffset;

        public void SetLockLayout(bool locked)
        {
            lockLayout = locked;
        }

        public static bool IsLocked(Transform notebookOpenRoot)
        {
            if (notebookOpenRoot == null)
            {
                return false;
            }

            NotebookBookArtLayout artLayout = notebookOpenRoot.GetComponent<NotebookBookArtLayout>();
            return artLayout != null && artLayout.LockLayout;
        }
    }
}
