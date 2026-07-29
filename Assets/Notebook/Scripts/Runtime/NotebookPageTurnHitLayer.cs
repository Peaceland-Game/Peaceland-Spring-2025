using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Dedicated full-height left/right hit targets for page turns.
    /// Tune <see cref="hitWidth"/> in the Inspector without touching bookmark tabs or page content.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotebookPageTurnHitLayer : MonoBehaviour
    {
        [SerializeField] private float hitWidth = NotebookBookShellLayout.DefaultPageTurnHitWidth;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;

        public float HitWidth => hitWidth;
        public Button PreviousPageButton => previousPageButton;
        public Button NextPageButton => nextPageButton;

        public void Configure(Button previousButton, Button nextButton, float targetHitWidth = NotebookBookShellLayout.DefaultPageTurnHitWidth)
        {
            previousPageButton = previousButton;
            nextPageButton = nextButton;
            hitWidth = Mathf.Max(48f, targetHitWidth);
            if (!IsParentLayoutLocked())
            {
                ApplyLayout();
            }
        }

        public void ApplyLayout()
        {
            NotebookBookShellLayout.ApplyPageTurnHitLayer(transform as RectTransform);

            if (previousPageButton != null)
            {
                NotebookBookShellLayout.ApplyPageTurnHitArea(previousPageButton.transform as RectTransform, true, hitWidth);
            }

            if (nextPageButton != null)
            {
                NotebookBookShellLayout.ApplyPageTurnHitArea(nextPageButton.transform as RectTransform, false, hitWidth);
            }
        }

        private void OnValidate()
        {
            hitWidth = Mathf.Max(48f, hitWidth);
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (Application.isPlaying)
            {
                ApplyLayout();
                return;
            }

            // Edit mode: respect hand-authored layout on Notebook Open Root.
            if (IsParentLayoutLocked())
            {
                return;
            }

            ApplyLayout();
        }

        private bool IsParentLayoutLocked()
        {
            NotebookBookArtLayout artLayout = GetComponentInParent<NotebookBookArtLayout>();
            return artLayout != null && artLayout.LockLayout;
        }
    }
}
