using System.Collections;
using TMPro;
using UnityEngine;
using Peaceland.Notebook.EditableScenePack;

namespace Peaceland.Notebook
{
    public class NotebookOverlayView : MonoBehaviour
    {
        [SerializeField] private GameObject collectibleHintRoot;
        [SerializeField] private TMP_Text collectibleHintText;
        [SerializeField] private RectTransform collectedToastRoot;
        [SerializeField] private TMP_Text collectedToastText;
        [SerializeField] private CanvasGroup collectedToastCanvasGroup;
        [SerializeField] private Vector2 toastHiddenPosition = NotebookCollectedToastLayout.HiddenPosition;
        [SerializeField] private Vector2 toastVisiblePosition = NotebookCollectedToastLayout.VisiblePosition;
        [SerializeField] private Vector2 toastExitPosition = NotebookCollectedToastLayout.ExitPosition;
        [SerializeField] private float toastSlideInDuration = 0.22f;
        [SerializeField] private float toastHoldDuration = 1.2f;
        [SerializeField] private float toastSlideOutDuration = 0.28f;

        [Header("Screen-safe notification layout")]
        [SerializeField] private float screenMargin = 24f;
        [SerializeField] private float notificationWidth = 420f;
        [SerializeField] private float toastHeight = 72f;
        [SerializeField] private float hintHeight = 52f;
        [SerializeField] private float notificationSpacing = 12f;

        private Coroutine toastRoutine;
        private bool applyingScreenLayout;

        private void Awake()
        {
            if (collectibleHintRoot != null)
            {
                collectibleHintRoot.SetActive(false);
            }

            SyncNotificationFromScene();
            ApplyScreenSafeLayout();

            if (collectedToastRoot != null)
            {
                collectedToastRoot.anchoredPosition = toastHiddenPosition;
            }

            if (collectedToastCanvasGroup != null)
            {
                collectedToastCanvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Reads visible/hidden/exit from <see cref="NotebookNotificationAnchor"/> when hand placement is enabled.
        /// </summary>
        public void SyncNotificationFromScene()
        {
            if (collectedToastRoot == null)
            {
                return;
            }

            if (!collectedToastRoot.TryGetComponent<NotebookNotificationAnchor>(out NotebookNotificationAnchor anchor)
                || !anchor.UseHandPlacedRestPosition)
            {
                return;
            }

            anchor.CaptureMotion(out Vector2 hidden, out Vector2 visible, out Vector2 exit);
            toastHiddenPosition = hidden;
            toastVisiblePosition = visible;
            toastExitPosition = exit;
        }

        public void Configure(
            GameObject targetCollectibleHintRoot,
            TMP_Text targetCollectibleHintText,
            RectTransform targetCollectedToastRoot,
            TMP_Text targetCollectedToastText,
            CanvasGroup targetCollectedToastCanvasGroup)
        {
            collectibleHintRoot = targetCollectibleHintRoot;
            collectibleHintText = targetCollectibleHintText;
            collectedToastRoot = targetCollectedToastRoot;
            collectedToastText = targetCollectedToastText;
            collectedToastCanvasGroup = targetCollectedToastCanvasGroup;

            SyncNotificationFromScene();
            ApplyScreenSafeLayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyScreenSafeLayout();
        }

        public void ApplyScreenSafeLayout()
        {
            if (applyingScreenLayout || collectedToastRoot == null)
            {
                return;
            }

            if (collectedToastRoot.TryGetComponent<NotebookNotificationAnchor>(out NotebookNotificationAnchor anchor)
                && anchor.UseHandPlacedRestPosition)
            {
                return;
            }

            applyingScreenLayout = true;
            try
            {
                RectTransform overlayRect = transform as RectTransform;
                float availableWidth = overlayRect != null ? overlayRect.rect.width - (screenMargin * 2f) : 0f;
                float width = availableWidth > 0f
                    ? Mathf.Min(notificationWidth, availableWidth)
                    : notificationWidth;

                ApplyScreenSafeRect(collectedToastRoot, width, toastHeight, screenMargin, -screenMargin);
                if (collectibleHintRoot != null)
                {
                    RectTransform hintRect = collectibleHintRoot.transform as RectTransform;
                    ApplyScreenSafeRect(
                        hintRect,
                        width,
                        hintHeight,
                        screenMargin,
                        -(screenMargin + toastHeight + notificationSpacing));
                }

                float hiddenX = -(width + screenMargin);
                ApplyToastLayout(
                    new Vector2(hiddenX, -screenMargin),
                    new Vector2(screenMargin, -screenMargin),
                    new Vector2(hiddenX, -screenMargin));
            }
            finally
            {
                applyingScreenLayout = false;
            }
        }

        private static void ApplyScreenSafeRect(
            RectTransform rect,
            float width,
            float height,
            float x,
            float y)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, y);
        }

        public void ApplyToastLayout(Vector2 hidden, Vector2 visible, Vector2 exit)
        {
            toastHiddenPosition = hidden;
            toastVisiblePosition = visible;
            toastExitPosition = exit;

            if (collectedToastRoot != null)
            {
                collectedToastRoot.anchoredPosition = toastHiddenPosition;
            }
        }

        public void SetCollectibleHintVisible(bool isVisible, int collectableCount)
        {
            if (collectibleHintRoot != null)
            {
                collectibleHintRoot.SetActive(isVisible);
            }

            if (collectibleHintText != null)
            {
                collectibleHintText.text = collectableCount > 1
                    ? $"Collectables available: {collectableCount}"
                    : "Collectable available";
            }
        }

        public void PlayCollectedToast(string message)
        {
            if (collectedToastRoot == null || collectedToastText == null || collectedToastCanvasGroup == null)
            {
                return;
            }

            collectedToastText.text = string.IsNullOrWhiteSpace(message)
                ? "Notebook updated"
                : message;

            if (toastRoutine != null)
            {
                StopCoroutine(toastRoutine);
            }

            toastRoutine = StartCoroutine(PlayCollectedToastRoutine());
        }

        private IEnumerator PlayCollectedToastRoutine()
        {
            yield return AnimateToast(toastHiddenPosition, toastVisiblePosition, 0f, 1f, toastSlideInDuration);
            yield return new WaitForSeconds(toastHoldDuration);
            yield return AnimateToast(toastVisiblePosition, toastExitPosition, 1f, 0f, toastSlideOutDuration);

            collectedToastRoot.anchoredPosition = toastHiddenPosition;
            collectedToastCanvasGroup.alpha = 0f;
            toastRoutine = null;
        }

        private IEnumerator AnimateToast(Vector2 from, Vector2 to, float fromAlpha, float toAlpha, float duration)
        {
            collectedToastRoot.anchoredPosition = from;
            collectedToastCanvasGroup.alpha = fromAlpha;

            if (duration <= 0f)
            {
                collectedToastRoot.anchoredPosition = to;
                collectedToastCanvasGroup.alpha = toAlpha;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                collectedToastRoot.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
                collectedToastCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);
                yield return null;
            }

            collectedToastRoot.anchoredPosition = to;
            collectedToastCanvasGroup.alpha = toAlpha;
        }
    }
}
