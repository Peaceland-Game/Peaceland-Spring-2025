using UnityEngine;
using UnityEngine.UI;
using Peaceland.Notebook.EditableScenePack;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Shared rect/layout constants for the open-notebook shell and page viewport.
    /// </summary>
    public static class NotebookBookShellLayout
    {
        public const float ReferenceBookWidth = 1200f;
        public const float ReferenceBookHeight = 660f;

        public static readonly Vector2 BookmarkRailOffsetLeft = new Vector2(-56f, 0f);
        public static readonly Vector2 BookmarkRailOffsetRight = new Vector2(56f, 0f);
        public const float BookmarkWidth = 56f;
        public const float BookmarkHeight = 72f;
        public const float BookmarkSpacing = 6f;
        public const float BookmarkTopInset = 96f;
        public const float BookmarkHorizontalOffset = 8f;

        public static readonly Vector2 PageInsetMin = new Vector2(72f, 40f);
        public static readonly Vector2 PageInsetMax = new Vector2(-52f, -36f);

        public static readonly Vector2 PageGutterLeft = new Vector2(20f, 20f);
        public static readonly Vector2 PageGutterRight = new Vector2(-20f, -20f);
        public static readonly Vector2 PageGutterCenterLeft = new Vector2(20f, 20f);
        public static readonly Vector2 PageGutterCenterRight = new Vector2(-10f, -20f);
        public static readonly Vector2 PageGutterCenterRightPage = new Vector2(10f, 20f);
        public static readonly Vector2 PageGutterCenterRightPageMax = new Vector2(-20f, -20f);

        public static void ApplyOpenRoot(RectTransform openRoot)
        {
            if (openRoot == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(openRoot))
            {
                return;
            }

            openRoot.anchorMin = new Vector2(0.5f, 0.5f);
            openRoot.anchorMax = new Vector2(0.5f, 0.5f);
            openRoot.pivot = new Vector2(0.5f, 0.5f);
            openRoot.anchoredPosition = Vector2.zero;
            openRoot.sizeDelta = new Vector2(ReferenceBookWidth, ReferenceBookHeight);

            Image background = openRoot.GetComponent<Image>();
            if (background != null)
            {
                background.raycastTarget = true;
                background.preserveAspect = true;
            }
        }

        public static void ApplyPagesViewport(RectTransform pagesViewport)
        {
            if (pagesViewport == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(pagesViewport))
            {
                return;
            }

            pagesViewport.anchorMin = Vector2.zero;
            pagesViewport.anchorMax = Vector2.one;
            pagesViewport.pivot = new Vector2(0.5f, 0.5f);
            pagesViewport.anchoredPosition = Vector2.zero;
            pagesViewport.offsetMin = PageInsetMin;
            pagesViewport.offsetMax = PageInsetMax;
        }

        public static void ApplySpreadPage(RectTransform pageRect, bool isLeftPage)
        {
            if (pageRect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(pageRect))
            {
                return;
            }

            if (isLeftPage)
            {
                pageRect.anchorMin = new Vector2(0f, 0f);
                pageRect.anchorMax = new Vector2(0.5f, 1f);
                pageRect.offsetMin = PageGutterCenterLeft;
                pageRect.offsetMax = PageGutterCenterRight;
            }
            else
            {
                pageRect.anchorMin = new Vector2(0.5f, 0f);
                pageRect.anchorMax = new Vector2(1f, 1f);
                pageRect.offsetMin = PageGutterCenterRightPage;
                pageRect.offsetMax = PageGutterCenterRightPageMax;
            }

            pageRect.pivot = new Vector2(0.5f, 0.5f);
            pageRect.anchoredPosition = Vector2.zero;
        }

        public static void ApplySectionChrome(RectTransform chromeRect)
        {
            if (chromeRect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(chromeRect))
            {
                return;
            }

            chromeRect.anchorMin = new Vector2(0f, 1f);
            chromeRect.anchorMax = new Vector2(1f, 1f);
            chromeRect.pivot = new Vector2(0.5f, 1f);
            chromeRect.anchoredPosition = Vector2.zero;
            chromeRect.offsetMin = new Vector2(16f, -44f);
            chromeRect.offsetMax = new Vector2(-16f, 0f);
        }

        public static void ApplySpreadBody(RectTransform bodyRect)
        {
            if (bodyRect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(bodyRect))
            {
                return;
            }

            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.pivot = new Vector2(0.5f, 0.5f);
            bodyRect.anchoredPosition = Vector2.zero;
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;
        }

        public const float DefaultPageTurnHitWidth = 160f;

        public static void ApplyPageTurnHitArea(RectTransform hitRect, bool isPrevious, float hitWidth = DefaultPageTurnHitWidth)
        {
            if (hitRect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(hitRect))
            {
                return;
            }

            hitWidth = Mathf.Max(48f, hitWidth);

            if (isPrevious)
            {
                hitRect.anchorMin = new Vector2(0f, 0f);
                hitRect.anchorMax = new Vector2(0f, 1f);
                hitRect.pivot = new Vector2(0f, 0.5f);
                hitRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                hitRect.anchorMin = new Vector2(1f, 0f);
                hitRect.anchorMax = new Vector2(1f, 1f);
                hitRect.pivot = new Vector2(1f, 0.5f);
                hitRect.anchoredPosition = Vector2.zero;
            }

            hitRect.sizeDelta = new Vector2(hitWidth, 0f);
        }

        public static void ApplyPageTurnHitLayer(RectTransform layerRect)
        {
            if (layerRect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(layerRect))
            {
                return;
            }

            layerRect.anchorMin = Vector2.zero;
            layerRect.anchorMax = Vector2.one;
            layerRect.pivot = new Vector2(0.5f, 0.5f);
            layerRect.anchoredPosition = Vector2.zero;
            layerRect.offsetMin = Vector2.zero;
            layerRect.offsetMax = Vector2.zero;
        }

        public static void ApplyPageEdgeButton(RectTransform buttonRect, bool isPrevious)
        {
            ApplyPageTurnHitArea(buttonRect, isPrevious, DefaultPageTurnHitWidth);
        }

        public static void ApplyBookmarkRailLeft(
            RectTransform railRect,
            float topInset = BookmarkTopInset,
            float horizontalOffset = BookmarkHorizontalOffset)
        {
            if (railRect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(railRect))
            {
                return;
            }

            railRect.anchorMin = new Vector2(0f, 1f);
            railRect.anchorMax = new Vector2(0f, 1f);
            railRect.pivot = new Vector2(1f, 1f);
            railRect.anchoredPosition = new Vector2(-Mathf.Abs(horizontalOffset), -Mathf.Abs(topInset));
            railRect.sizeDelta = new Vector2(BookmarkWidth, BookmarkHeight);
        }

        public static void ApplyBookmarkRailRight(
            RectTransform railRect,
            float topInset = BookmarkTopInset,
            float horizontalOffset = BookmarkHorizontalOffset)
        {
            if (railRect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(railRect))
            {
                return;
            }

            railRect.anchorMin = new Vector2(1f, 1f);
            railRect.anchorMax = new Vector2(1f, 1f);
            railRect.pivot = new Vector2(0f, 1f);
            railRect.anchoredPosition = new Vector2(Mathf.Abs(horizontalOffset), -Mathf.Abs(topInset));
            railRect.sizeDelta = new Vector2(BookmarkWidth, BookmarkHeight);
        }

        [System.Obsolete("Use ApplyBookmarkRailLeft")]
        public static void ApplyBookmarkRail(RectTransform railRect)
        {
            ApplyBookmarkRailLeft(railRect);
        }

        public static void ApplyBookmarkTabLeft(RectTransform tabRect, int indexFromTop)
        {
            ApplyBookmarkTab(tabRect, indexFromTop, false);
        }

        public static void ApplyBookmarkTabRight(RectTransform tabRect, int indexFromTop)
        {
            ApplyBookmarkTab(tabRect, indexFromTop, true);
        }

        private static void ApplyBookmarkTab(RectTransform tabRect, int sectionSlotIndex, bool isRightSide)
        {
            if (tabRect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(tabRect))
            {
                return;
            }

            // sectionSlotIndex is the global section order (Dir=0, Present=1, ...), not a per-rail stack index.
            tabRect.anchorMin = new Vector2(0f, 1f);
            tabRect.anchorMax = new Vector2(1f, 1f);
            tabRect.pivot = new Vector2(isRightSide ? 0f : 1f, 1f);
            float y = -(sectionSlotIndex * (BookmarkHeight + BookmarkSpacing));
            tabRect.anchoredPosition = new Vector2(0f, y);
            tabRect.sizeDelta = new Vector2(0f, BookmarkHeight);
        }

        [System.Obsolete("Use ApplyBookmarkTabLeft")]
        public static void ApplyBookmarkTab(RectTransform tabRect, int indexFromTop)
        {
            ApplyBookmarkTabLeft(tabRect, indexFromTop);
        }

        public static void ApplyAnimationOverlay(RectTransform animationRect)
        {
            if (animationRect == null || NotebookUILayoutGuard.ShouldSkipLayoutApply(animationRect))
            {
                return;
            }

            animationRect.anchorMin = Vector2.zero;
            animationRect.anchorMax = Vector2.one;
            animationRect.pivot = new Vector2(0.5f, 0.5f);
            animationRect.anchoredPosition = Vector2.zero;
            animationRect.offsetMin = Vector2.zero;
            animationRect.offsetMax = Vector2.zero;
        }
    }
}
