using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Keeps the open notebook sized relative to the parent canvas (resolution-aware).
    /// Disabled by default so Book Background / Open Root rects stay as you set them in the Editor.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class NotebookUIScaleDriver : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private bool fitOpenRootToCanvas;
        [SerializeField] [Range(0.5f, 1f)] private float maxCanvasWidthPercent = 0.9f;
        [SerializeField] private float referenceWidth = NotebookBookShellLayout.ReferenceBookWidth;
        [SerializeField] private float referenceHeight = NotebookBookShellLayout.ReferenceBookHeight;

        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private NotebookBookArtLayout artLayout;

        private void Awake()
        {
            rectTransform = target != null ? target : transform as RectTransform;
            rootCanvas = GetComponentInParent<Canvas>();
            artLayout = GetComponent<NotebookBookArtLayout>();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            ApplyScale();
        }

#if UNITY_EDITOR
        private void OnRectTransformDimensionsChange()
        {
            if (!fitOpenRootToCanvas)
            {
                return;
            }

            ApplyScale();
        }
#endif

        private void Update()
        {
            if (!fitOpenRootToCanvas)
            {
                return;
            }

            ApplyScale();
        }

        public void ApplyScale()
        {
            if (!fitOpenRootToCanvas || rectTransform == null)
            {
                return;
            }

            if (artLayout != null && artLayout.LockLayout)
            {
                return;
            }

            float canvasWidth = referenceWidth;
            if (rootCanvas != null)
            {
                RectTransform canvasRect = rootCanvas.transform as RectTransform;
                if (canvasRect != null)
                {
                    canvasWidth = canvasRect.rect.width;
                }
            }

            float targetWidth = Mathf.Min(referenceWidth, canvasWidth * maxCanvasWidthPercent);
            float aspect = referenceHeight / referenceWidth;
            float targetHeight = targetWidth * aspect;

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
        }
    }
}
