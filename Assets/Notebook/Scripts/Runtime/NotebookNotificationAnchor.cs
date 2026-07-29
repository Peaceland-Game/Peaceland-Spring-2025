using UnityEngine;

namespace Peaceland.Notebook
{
    public enum NotebookNotificationSlideFrom
    {
        Right,
        Left,
        Up,
        Down
    }

    /// <summary>
    /// Place on <c>Collected Toast</c>. Screen-safe placement is the default;
    /// hand placement is an explicit authoring/debug option.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class NotebookNotificationAnchor : MonoBehaviour
    {
        [Tooltip("Keep the notification in the full-screen left-top safe area. Disable only when debugging a hand-placed scene layout.")]
        [SerializeField] private bool useScreenSafePosition = true;

        [Tooltip("When enabled and screen-safe placement is disabled, runtime reads anchoredPosition from this object.")]
        [SerializeField] private bool useHandPlacedRestPosition;

        [Tooltip("Direction the toast slides in from (relative to the hand-placed rest position).")]
        [SerializeField] private NotebookNotificationSlideFrom slideFrom = NotebookNotificationSlideFrom.Right;

        [Tooltip("How far off-screen the toast starts and exits, in anchored pixels along the slide axis.")]
        [SerializeField] private float slideDistance = 144f;

        public bool UseScreenSafePosition => useScreenSafePosition;
        public bool UseHandPlacedRestPosition => useHandPlacedRestPosition && !useScreenSafePosition;

        public void CaptureMotion(out Vector2 hidden, out Vector2 visible, out Vector2 exit)
        {
            RectTransform rect = (RectTransform)transform;
            visible = rect.anchoredPosition;
            Vector2 delta = GetSlideDelta();
            hidden = visible + delta;
            exit = hidden;
        }

        public void ApplyPresetTopRightDefaults()
        {
            ApplyPresetScreenSafeDefaults();
        }

        public void ApplyPresetScreenSafeDefaults()
        {
            RectTransform rect = (RectTransform)transform;
            rect.anchorMin = NotebookCollectedToastLayout.Anchor;
            rect.anchorMax = NotebookCollectedToastLayout.Anchor;
            rect.pivot = NotebookCollectedToastLayout.Pivot;
            rect.sizeDelta = NotebookCollectedToastLayout.Size;
            rect.anchoredPosition = NotebookCollectedToastLayout.HiddenPosition;
            useScreenSafePosition = true;
            useHandPlacedRestPosition = false;
        }

        public void EnableHandPlacedRestPosition()
        {
            useScreenSafePosition = false;
            useHandPlacedRestPosition = true;
        }

        private Vector2 GetSlideDelta()
        {
            switch (slideFrom)
            {
                case NotebookNotificationSlideFrom.Left:
                    return new Vector2(-slideDistance, 0f);
                case NotebookNotificationSlideFrom.Up:
                    return new Vector2(0f, slideDistance);
                case NotebookNotificationSlideFrom.Down:
                    return new Vector2(0f, -slideDistance);
                default:
                    return new Vector2(slideDistance, 0f);
            }
        }
    }
}
