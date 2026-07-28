using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Inspector-facing layout contract for an entry cell. The same values drive
    /// the UGUI hierarchy and pagination estimates.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotebookEntryCellLayout : MonoBehaviour
    {
        public const float DefaultPreferredHeight = 180f;
        public const float DefaultVerticalPadding = 36f;
        public const float DefaultTitleHeight = 38f;
        public const float DefaultSectionSpacing = 10f;
        public const float DefaultContentSpacing = 16f;
        public const float DefaultImageWidth = 184f;
        public const float DefaultImageHeight = 124f;
        public const float DefaultChoiceSpacing = 6f;
        public const float DefaultChoicePromptHeight = 28f;
        public const float DefaultChoiceButtonHeight = 38f;
        public const float DefaultChoiceRecordedHeight = 28f;

        [Header("Cell")]
        // RectOffset mutates Unity-backed properties in its constructor. Keep the
        // field null until Unity has created the component, then supply defaults
        // through SafePadding. This keeps scene authoring deterministic.
        [SerializeField] private RectOffset padding;
        [SerializeField] private float sectionSpacing = 10f;
        [SerializeField] private float titleHeight = 38f;

        [Header("Content row")]
        [SerializeField] private float contentSpacing = 16f;
        [SerializeField] private float imageWidth = 184f;
        [SerializeField] private float imageHeight = 124f;
        [SerializeField] private float imagePadding = 10f;

        [Header("Record choice")]
        [SerializeField] private float choiceSpacing = 6f;
        [SerializeField] private float choicePromptHeight = 28f;
        [SerializeField] private float choiceButtonHeight = 38f;
        [SerializeField] private float choiceRecordedHeight = 28f;

        public float InnerWidth(float columnWidth)
        {
            return Mathf.Max(40f, columnWidth - HorizontalPadding);
        }

        public float BodyWidth(float columnWidth, bool hasImage)
        {
            float width = InnerWidth(columnWidth);
            return hasImage ? Mathf.Max(40f, width - imageWidth - contentSpacing) : width;
        }

        public float MeasureChoiceHeight(int choiceCount, bool completed)
        {
            float contentHeight = completed
                ? choiceRecordedHeight
                : choiceButtonHeight * Mathf.Clamp(choiceCount, 0, 3);
            int spacingCount = completed ? 1 : Mathf.Clamp(choiceCount, 0, 3);
            return choicePromptHeight + contentHeight + (spacingCount * choiceSpacing);
        }

        public float VerticalPadding => SafePadding.top + SafePadding.bottom;
        public float SectionSpacing => Mathf.Max(0f, sectionSpacing);
        public float TitleHeight => Mathf.Max(1f, titleHeight);
        public float ContentSpacing => Mathf.Max(0f, contentSpacing);
        public float ImageWidth => Mathf.Max(1f, imageWidth);
        public float ImageHeight => Mathf.Max(1f, imageHeight);
        public float ImagePadding => Mathf.Max(0f, imagePadding);
        public float ChoiceSpacing => Mathf.Max(0f, choiceSpacing);
        public float ChoicePromptHeight => Mathf.Max(1f, choicePromptHeight);
        public float ChoiceButtonHeight => Mathf.Max(1f, choiceButtonHeight);
        public float ChoiceRecordedHeight => Mathf.Max(1f, choiceRecordedHeight);
        public float MinimumHeight => VerticalPadding + TitleHeight + ContentSpacing;
        public float HorizontalPadding => SafePadding.left + SafePadding.right;

        public void ApplyTo(RectTransform root)
        {
            if (root == null)
            {
                return;
            }

            Transform title = root.Find("Title");
            Transform body = root.Find("Body") ?? root.Find("Content Row/Body");
            Transform imageRoot = root.Find("Image Root") ?? root.Find("Content Row/Image Root");
            Transform contentRow = EnsureChild(root, "Content Row");
            Transform choiceRoot = EnsureChild(root, "Record Choice Root");

            Reparent(body, contentRow);
            Reparent(imageRoot, contentRow);

            VerticalLayoutGroup rootLayout = GetOrAdd<VerticalLayoutGroup>(root.gameObject);
            rootLayout.padding = SafePadding;
            rootLayout.spacing = SectionSpacing;
            rootLayout.childAlignment = TextAnchor.UpperLeft;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            SetOverlay(root.Find("Highlight"));
            SetOverlay(root.Find("New Badge"));
            ConfigureTitle(title);
            ConfigureContent(contentRow, body, imageRoot);
            ConfigureChoice(choiceRoot);

            LayoutElement rootLayoutElement = GetOrAdd<LayoutElement>(root.gameObject);
            rootLayoutElement.minHeight = MinimumHeight;
            rootLayoutElement.flexibleWidth = 1f;
        }

        private void ConfigureTitle(Transform title)
        {
            if (title == null)
            {
                return;
            }

            LayoutElement layout = GetOrAdd<LayoutElement>(title.gameObject);
            layout.preferredHeight = TitleHeight;
            layout.flexibleWidth = 1f;
        }

        private void ConfigureContent(Transform contentRow, Transform body, Transform imageRoot)
        {
            if (contentRow == null)
            {
                return;
            }

            HorizontalLayoutGroup rowLayout = GetOrAdd<HorizontalLayoutGroup>(contentRow.gameObject);
            rowLayout.spacing = ContentSpacing;
            rowLayout.childAlignment = TextAnchor.UpperLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            LayoutElement rowElement = GetOrAdd<LayoutElement>(contentRow.gameObject);
            rowElement.flexibleWidth = 1f;

            if (body != null)
            {
                LayoutElement bodyElement = GetOrAdd<LayoutElement>(body.gameObject);
                bodyElement.minWidth = 0f;
                bodyElement.preferredWidth = 0f;
                bodyElement.flexibleWidth = 1f;

                ContentSizeFitter fitter = GetOrAdd<ContentSizeFitter>(body.gameObject);
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                TMP_Text text = body.GetComponent<TMP_Text>();
                if (text != null)
                {
                    text.textWrappingMode = TextWrappingModes.Normal;
                    text.overflowMode = TextOverflowModes.Overflow;
                }
            }

            if (imageRoot != null)
            {
                LayoutElement imageElement = GetOrAdd<LayoutElement>(imageRoot.gameObject);
                imageElement.minWidth = ImageWidth;
                imageElement.preferredWidth = ImageWidth;
                imageElement.minHeight = ImageHeight;
                imageElement.preferredHeight = ImageHeight;
                imageElement.flexibleWidth = 0f;
                imageElement.flexibleHeight = 0f;

                RectTransform imageRect = imageRoot as RectTransform;
                if (imageRect != null)
                {
                    imageRect.sizeDelta = new Vector2(ImageWidth, ImageHeight);
                }

                Transform image = imageRoot.Find("Image");
                if (image != null)
                {
                    RectTransform imageRectTransform = image as RectTransform;
                    if (imageRectTransform != null)
                    {
                        imageRectTransform.anchorMin = Vector2.zero;
                        imageRectTransform.anchorMax = Vector2.one;
                        imageRectTransform.offsetMin = new Vector2(ImagePadding, ImagePadding);
                        imageRectTransform.offsetMax = new Vector2(-ImagePadding, -ImagePadding);
                    }
                }
            }
        }

        private void ConfigureChoice(Transform choiceRoot)
        {
            if (choiceRoot == null)
            {
                return;
            }

            VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(choiceRoot.gameObject);
            layout.spacing = ChoiceSpacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            LayoutElement element = GetOrAdd<LayoutElement>(choiceRoot.gameObject);
            element.flexibleWidth = 1f;
        }

        private static void SetOverlay(Transform child)
        {
            if (child != null)
            {
                GetOrAdd<LayoutElement>(child.gameObject).ignoreLayout = true;
            }
        }

        private static void Reparent(Transform child, Transform parent)
        {
            if (child != null && parent != null && child.parent != parent)
            {
                child.SetParent(parent, false);
            }
        }

        private static Transform EnsureChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            GameObject childObject = new GameObject(name, typeof(RectTransform));
            childObject.transform.SetParent(parent, false);
            return childObject.transform;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private RectOffset SafePadding
        {
            get
            {
                if (padding == null)
                {
                    padding = new RectOffset(24, 24, 18, 18);
                }

                return padding;
            }
        }
    }
}
