using UnityEngine;
using Peaceland.Notebook.EditableScenePack;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Shared defaults for the screen-safe "Notebook updated" collect toast.
    /// </summary>
    public static class NotebookCollectedToastLayout
    {
        public static readonly Vector2 Anchor = new Vector2(0f, 1f);
        public static readonly Vector2 Pivot = new Vector2(0f, 1f);
        public static readonly Vector2 RestPosition = new Vector2(24f, -24f);
        public static readonly Vector2 Size = new Vector2(380f, 72f);

        public static readonly Vector2 HiddenPosition = new Vector2(-404f, -24f);
        public static readonly Vector2 VisiblePosition = RestPosition;
        public static readonly Vector2 ExitPosition = HiddenPosition;

        public static void ApplyTo(RectTransform toastRoot)
        {
            if (toastRoot == null)
            {
                return;
            }

            if (NotebookUILayoutGuard.ShouldSkipLayoutApply(toastRoot))
            {
                return;
            }

            if (toastRoot.TryGetComponent<NotebookNotificationAnchor>(out NotebookNotificationAnchor anchor)
                && anchor.UseHandPlacedRestPosition)
            {
                return;
            }

            toastRoot.anchorMin = Anchor;
            toastRoot.anchorMax = Anchor;
            toastRoot.pivot = Pivot;
            toastRoot.sizeDelta = Size;
            toastRoot.anchoredPosition = HiddenPosition;
        }

        public static void ApplyToOverlayView(NotebookOverlayView overlayView)
        {
            if (overlayView == null)
            {
                return;
            }

            overlayView.ApplyToastLayout(
                HiddenPosition,
                VisiblePosition,
                ExitPosition);
        }
    }
}
