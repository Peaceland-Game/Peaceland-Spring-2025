using UnityEngine;
using UnityEngine.UI;

namespace Peaceland
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))]
    public sealed class PeacelandResponsiveSaveGrid : MonoBehaviour
    {
        [Header("Figma Save Grid")]
        [Min(1)]
        [SerializeField] private int columns = 2;
        [Min(40f)]
        [SerializeField] private float cardHeight = 128f;
        [Min(0f)]
        [SerializeField] private float horizontalSpacing = 34f;
        [Min(0f)]
        [SerializeField] private float verticalSpacing = 18f;
        [Min(0f)]
        [SerializeField] private float horizontalPadding = 4f;

        private RectTransform rectTransform;
        private GridLayoutGroup grid;
        private float lastWidth = -1f;

        private void OnEnable()
        {
            Refresh();
        }

        private void LateUpdate()
        {
            if (!Mathf.Approximately(Rect.rect.width, lastWidth))
            {
                Refresh();
            }
        }

        private void OnValidate()
        {
            columns = Mathf.Max(1, columns);
            Refresh();
        }

        public void Refresh()
        {
            rectTransform ??= GetComponent<RectTransform>();
            grid ??= GetComponent<GridLayoutGroup>();
            if (rectTransform == null || grid == null)
            {
                return;
            }

            float width = rectTransform.rect.width;
            lastWidth = width;
            float usableWidth = Mathf.Max(
                columns,
                width - horizontalPadding * 2f - horizontalSpacing * (columns - 1));

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.spacing = new Vector2(horizontalSpacing, verticalSpacing);
            grid.padding = new RectOffset(
                Mathf.RoundToInt(horizontalPadding),
                Mathf.RoundToInt(horizontalPadding),
                4,
                4);
            grid.cellSize = new Vector2(usableWidth / columns, cardHeight);
        }

        private RectTransform Rect =>
            rectTransform != null ? rectTransform : GetComponent<RectTransform>();
    }
}
