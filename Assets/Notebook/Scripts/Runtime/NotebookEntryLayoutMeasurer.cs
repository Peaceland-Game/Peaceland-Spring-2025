using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Estimates how tall an entry block will be inside a notebook page column.
    /// Used by pagination so overflow moves to the next page instead of stacking past the page bottom.
    /// </summary>
    public static class NotebookEntryLayoutMeasurer
    {
        private const float EmptyBodyMinHeight = 12f;
        private const float MinEntryHeight = 72f;

        public static float Measure(NotebookEntryDefinition entry, float columnWidth)
        {
            return Measure(entry, columnWidth, null);
        }

        public static float Measure(
            NotebookEntryDefinition entry,
            float columnWidth,
            NotebookEntryCellLayout layout)
        {
            if (entry == null)
            {
                return MinEntryHeight;
            }

            float safeWidth = Mathf.Max(80f, columnWidth);
            float verticalPadding = layout != null
                ? layout.VerticalPadding
                : NotebookEntryCellLayout.DefaultVerticalPadding;
            float titleHeight = layout != null
                ? layout.TitleHeight
                : NotebookEntryCellLayout.DefaultTitleHeight;
            float contentSpacing = layout != null
                ? layout.ContentSpacing
                : NotebookEntryCellLayout.DefaultContentSpacing;
            float sectionSpacing = layout != null
                ? layout.SectionSpacing
                : NotebookEntryCellLayout.DefaultSectionSpacing;
            float imageHeight = layout != null
                ? layout.ImageHeight
                : NotebookEntryCellLayout.DefaultImageHeight;
            float bodyWidth = layout != null
                ? layout.BodyWidth(safeWidth, entry.Image != null)
                : Mathf.Max(40f, safeWidth - (entry.Image != null
                    ? NotebookEntryCellLayout.DefaultImageWidth + contentSpacing
                    : 0f) - 48f);

            float bodyHeight = EmptyBodyMinHeight;

            if (!string.IsNullOrWhiteSpace(entry.BodyText))
            {
                bodyHeight = MeasureTextHeight(entry.BodyText, 20f, bodyWidth);
            }

            float contentHeight = entry.Image != null
                ? Mathf.Max(bodyHeight, imageHeight)
                : bodyHeight;
            float height = verticalPadding + titleHeight + sectionSpacing + contentHeight;

            if (entry.RequiresRecordChoice)
            {
                int choiceCount = Mathf.Min(3, entry.RecordChoices.Count);
                float choiceHeight = layout != null
                    ? layout.MeasureChoiceHeight(choiceCount, false)
                    : NotebookEntryCellLayout.DefaultChoicePromptHeight
                        + (choiceCount * NotebookEntryCellLayout.DefaultChoiceButtonHeight)
                        + (choiceCount * NotebookEntryCellLayout.DefaultChoiceSpacing);
                height += sectionSpacing + choiceHeight;
            }

            // Legacy authored heights remain useful for intentionally empty test rows,
            // while populated cells size from their actual content.
            if (string.IsNullOrWhiteSpace(entry.BodyText)
                && entry.Image == null
                && !entry.RequiresRecordChoice)
            {
                height = Mathf.Max(height, entry.LayoutHeight);
            }

            float minimumHeight = layout != null ? layout.MinimumHeight : MinEntryHeight;
            return Mathf.Max(MinEntryHeight, minimumHeight, height);
        }

        public static float MeasureIndexRowHeight(bool isCategoryHeader, NotebookBookLayoutSettings settings)
        {
            if (settings == null)
            {
                return isCategoryHeader ? 70f : 38f;
            }

            return isCategoryHeader
                ? settings.indexCategoryHeaderHeight + settings.indexCategorySpacing
                : settings.indexEntryRowHeight;
        }

        private static float MeasureTextHeight(string text, float fontSize, float width)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0f;
            }

            float lineHeight = fontSize * 1.28f;
            int charsPerLine = Mathf.Max(8, Mathf.FloorToInt(width / (fontSize * 0.52f)));
            int lineCount = 0;
            string[] paragraphs = text.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < paragraphs.Length; i++)
            {
                string paragraph = paragraphs[i];
                if (string.IsNullOrEmpty(paragraph))
                {
                    lineCount += 1;
                    continue;
                }

                lineCount += Mathf.Max(1, Mathf.CeilToInt(paragraph.Length / (float)charsPerLine));
            }

            return lineCount * lineHeight;
        }
    }
}
