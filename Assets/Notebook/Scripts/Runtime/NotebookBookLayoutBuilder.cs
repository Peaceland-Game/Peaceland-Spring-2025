using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Peaceland.Notebook
{
    [Serializable]
    public sealed class NotebookBookLayoutSettings
    {
        public float contentPageHeight = 720f;
        public float indexPageHeight = 720f;
        public float indexCategoryHeaderHeight = 52f;
        public float indexEntryRowHeight = 38f;
        public float indexCategorySpacing = 18f;
        public float indexEntryRowSpacing = 10f;
        public float contentEntrySpacing = 18f;
    }

    public sealed class NotebookBookLayout
    {
        public readonly List<NotebookSpreadLayout> Spreads = new List<NotebookSpreadLayout>();
        public readonly Dictionary<string, int> EntryPageById = new Dictionary<string, int>();

        public int TotalPages => Spreads.Count * 2;
    }

    public sealed class NotebookSpreadLayout
    {
        public NotebookSpreadKind Kind;
        public NotebookSection Section;
        public int LeftPageNumber;
        public int RightPageNumber;
        public readonly List<NotebookIndexGroupLayout> LeftIndexGroups = new List<NotebookIndexGroupLayout>();
        public readonly List<NotebookIndexGroupLayout> RightIndexGroups = new List<NotebookIndexGroupLayout>();
        public readonly List<NotebookEntryDefinition> LeftEntries = new List<NotebookEntryDefinition>();
        public readonly List<NotebookEntryDefinition> RightEntries = new List<NotebookEntryDefinition>();
        public readonly Dictionary<string, float> EntryHeights = new Dictionary<string, float>();

        public void SetEntryHeight(string entryId, float height)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                return;
            }

            EntryHeights[entryId] = Mathf.Max(1f, height);
        }

        public bool TryGetEntryHeight(string entryId, out float height)
        {
            return EntryHeights.TryGetValue(entryId, out height);
        }
    }

    public sealed class NotebookIndexGroupLayout
    {
        public string CategoryId;
        public string DisplayName;
        public readonly List<NotebookIndexEntryLayout> Entries = new List<NotebookIndexEntryLayout>();
    }

    public sealed class NotebookIndexEntryLayout
    {
        public string EntryId;
        public string Title;
        public bool IsNew;
        public int TargetPageNumber;
    }

    public enum NotebookSpreadKind
    {
        MainDirectory = 0,
        SectionDirectory = 1,
        Content = 2,
        StatEditor = 3,
    }

    public static class NotebookBookLayoutBuilder
    {
        private sealed class PendingIndexRow
        {
            public string CategoryId;
            public string CategoryDisplayName;
            public bool IsHeader;
            public NotebookEntryDefinition Entry;
            public bool IsNew;
        }

        public static NotebookBookLayout Build(
            NotebookDatabase database,
            IEnumerable<NotebookController.SectionDefinition> sectionDefinitions,
            Func<string, bool> isCollected,
            Func<string, bool> isNew,
            NotebookBookLayoutSettings settings = null,
            Func<NotebookEntryDefinition, float> resolveEntryHeight = null)
        {
            if (settings == null)
            {
                settings = new NotebookBookLayoutSettings();
            }

            NormalizeSettings(settings);

            NotebookBookLayout layout = new NotebookBookLayout();
            if (database == null || sectionDefinitions == null)
            {
                return layout;
            }

            List<NotebookController.SectionDefinition> orderedSections = sectionDefinitions
                .Where(section => section != null && section.section != NotebookSection.Directory)
                .ToList();

            Dictionary<NotebookSection, List<PendingIndexRow>> indexRowsBySection = BuildPendingIndexRows(
                database,
                orderedSections,
                isCollected,
                isNew);

            // Spread 1: left page is the top-level directory, right page intentionally blank.
            NotebookSpreadLayout mainDirectorySpread = new NotebookSpreadLayout
            {
                Kind = NotebookSpreadKind.MainDirectory,
                Section = NotebookSection.Directory,
                LeftPageNumber = 1,
                RightPageNumber = 2,
            };

            NotebookIndexGroupLayout majorSectionGroup = new NotebookIndexGroupLayout
            {
                CategoryId = "major-sections",
                DisplayName = "Directory",
            };

            foreach (NotebookController.SectionDefinition sectionDefinition in orderedSections)
            {
                majorSectionGroup.Entries.Add(new NotebookIndexEntryLayout
                {
                    EntryId = sectionDefinition.section.ToString(),
                    Title = sectionDefinition.displayName,
                    TargetPageNumber = 0,
                });
            }

            mainDirectorySpread.LeftIndexGroups.Add(majorSectionGroup);
            layout.Spreads.Add(mainDirectorySpread);

            Dictionary<NotebookSection, int> firstDirectoryPageBySection = new Dictionary<NotebookSection, int>();

            // Per section: directory spreads, then content spreads (so page turns stay inside the section).
            foreach (NotebookController.SectionDefinition sectionDefinition in orderedSections)
            {
                if (sectionDefinition.section == NotebookSection.HiddenStats)
                {
                    NotebookSpreadLayout statSpread = new NotebookSpreadLayout
                    {
                        Kind = NotebookSpreadKind.StatEditor,
                        Section = NotebookSection.HiddenStats,
                    };

                    firstDirectoryPageBySection[sectionDefinition.section] = (layout.Spreads.Count * 2) + 1;
                    AssignSpreadPageNumbers(layout, statSpread);
                    layout.Spreads.Add(statSpread);
                    continue;
                }

                List<PendingIndexRow> foundRows;
                List<PendingIndexRow> rows = indexRowsBySection.TryGetValue(sectionDefinition.section, out foundRows)
                    ? foundRows
                    : new List<PendingIndexRow>();

                List<NotebookSpreadLayout> sectionSpreads = BuildSectionDirectorySpreads(sectionDefinition.section, rows, settings);
                if (sectionSpreads.Count == 0)
                {
                    sectionSpreads.Add(new NotebookSpreadLayout
                    {
                        Kind = NotebookSpreadKind.SectionDirectory,
                        Section = sectionDefinition.section,
                    });
                }

                firstDirectoryPageBySection[sectionDefinition.section] = (layout.Spreads.Count * 2) + 1;
                foreach (NotebookSpreadLayout spread in sectionSpreads)
                {
                    AssignSpreadPageNumbers(layout, spread);
                    layout.Spreads.Add(spread);
                }

                List<NotebookEntryDefinition> sectionEntries = database
                    .GetEntriesForSection(sectionDefinition.section)
                    .Where(entry => entry != null && isCollected != null && isCollected(entry.EntryId))
                    .OrderBy(entry => entry.TheoreticalOrder)
                    .ThenBy(entry => entry.SortOrder)
                    .ThenBy(entry => entry.Title)
                    .ToList();

                List<NotebookSpreadLayout> contentSpreads = BuildContentSpreads(
                    sectionDefinition.section,
                    sectionEntries,
                    settings,
                    resolveEntryHeight);
                foreach (NotebookSpreadLayout spread in contentSpreads)
                {
                    AssignSpreadPageNumbers(layout, spread);

                    foreach (NotebookEntryDefinition entry in spread.LeftEntries)
                    {
                        layout.EntryPageById[entry.EntryId] = spread.LeftPageNumber;
                    }

                    foreach (NotebookEntryDefinition entry in spread.RightEntries)
                    {
                        layout.EntryPageById[entry.EntryId] = spread.RightPageNumber;
                    }

                    layout.Spreads.Add(spread);
                }
            }

            // Fill section directory row targets now that content pages are known.
            foreach (NotebookSpreadLayout spread in layout.Spreads.Where(spread => spread.Kind == NotebookSpreadKind.SectionDirectory))
            {
                ApplyIndexTargets(spread.LeftIndexGroups, layout.EntryPageById);
                ApplyIndexTargets(spread.RightIndexGroups, layout.EntryPageById);
            }

            // Point major-section directory rows to the first directory page for each section.
            foreach (NotebookController.SectionDefinition sectionDefinition in orderedSections)
            {
                NotebookIndexEntryLayout majorEntry = majorSectionGroup.Entries.FirstOrDefault(entry => entry.EntryId == sectionDefinition.section.ToString());
                int targetPage;
                if (majorEntry != null && firstDirectoryPageBySection.TryGetValue(sectionDefinition.section, out targetPage))
                {
                    majorEntry.TargetPageNumber = targetPage;
                }
            }

            return layout;
        }

        private static void AssignSpreadPageNumbers(NotebookBookLayout layout, NotebookSpreadLayout spread)
        {
            int spreadIndex = layout.Spreads.Count;
            spread.LeftPageNumber = (spreadIndex * 2) + 1;
            spread.RightPageNumber = (spreadIndex * 2) + 2;
        }

        private static void NormalizeSettings(NotebookBookLayoutSettings settings)
        {
            if (settings.contentPageHeight < 80f)
            {
                settings.contentPageHeight = 520f;
            }

            if (settings.indexPageHeight < 80f)
            {
                settings.indexPageHeight = settings.contentPageHeight;
            }
        }

        private static Dictionary<NotebookSection, List<PendingIndexRow>> BuildPendingIndexRows(
            NotebookDatabase database,
            IEnumerable<NotebookController.SectionDefinition> orderedSections,
            Func<string, bool> isCollected,
            Func<string, bool> isNew)
        {
            Dictionary<NotebookSection, List<PendingIndexRow>> result = new Dictionary<NotebookSection, List<PendingIndexRow>>();

            foreach (NotebookController.SectionDefinition sectionDefinition in orderedSections)
            {
                if (sectionDefinition.section == NotebookSection.HiddenStats)
                {
                    continue;
                }

                List<PendingIndexRow> rows = new List<PendingIndexRow>();
                List<NotebookCategoryDefinition> categories = database.GetCategoriesForSection(sectionDefinition.section);

                for (int i = 0; i < categories.Count; i++)
                {
                    NotebookCategoryDefinition category = categories[i];
                    List<NotebookEntryDefinition> categoryEntries = database
                        .GetEntriesForSectionCategory(sectionDefinition.section, category.CategoryId)
                        .Where(entry => isCollected != null && isCollected(entry.EntryId))
                        .ToList();

                    if (categoryEntries.Count == 0)
                    {
                        continue;
                    }

                    rows.Add(new PendingIndexRow
                    {
                        CategoryId = category.CategoryId,
                        CategoryDisplayName = category.DisplayName,
                        IsHeader = true,
                    });

                    for (int entryIndex = 0; entryIndex < categoryEntries.Count; entryIndex++)
                    {
                        NotebookEntryDefinition entry = categoryEntries[entryIndex];
                        rows.Add(new PendingIndexRow
                        {
                            CategoryId = category.CategoryId,
                            CategoryDisplayName = category.DisplayName,
                            IsHeader = false,
                            Entry = entry,
                            IsNew = isNew != null && isNew(entry.EntryId),
                        });
                    }
                }

                result[sectionDefinition.section] = rows;
            }

            return result;
        }

        private static List<NotebookSpreadLayout> BuildSectionDirectorySpreads(
            NotebookSection section,
            List<PendingIndexRow> rows,
            NotebookBookLayoutSettings settings)
        {
            List<NotebookSpreadLayout> spreads = new List<NotebookSpreadLayout>();
            if (rows == null || rows.Count == 0)
            {
                return spreads;
            }

            NotebookSpreadLayout currentSpread = NewSectionDirectorySpread(section);
            bool onLeftPage = true;
            float usedHeight = 0f;

            for (int i = 0; i < rows.Count; i++)
            {
                PendingIndexRow row = rows[i];
                float rowHeight = NotebookEntryLayoutMeasurer.MeasureIndexRowHeight(row.IsHeader, settings);
                if (!row.IsHeader && usedHeight > 0f)
                {
                    rowHeight += settings.indexEntryRowSpacing;
                }

                if (usedHeight + rowHeight > settings.indexPageHeight)
                {
                    if (onLeftPage)
                    {
                        onLeftPage = false;
                        usedHeight = 0f;
                    }
                    else
                    {
                        spreads.Add(currentSpread);
                        currentSpread = NewSectionDirectorySpread(section);
                        onLeftPage = true;
                        usedHeight = 0f;
                    }
                }

                List<NotebookIndexGroupLayout> groups = onLeftPage ? currentSpread.LeftIndexGroups : currentSpread.RightIndexGroups;
                NotebookIndexGroupLayout currentGroup = groups.LastOrDefault();

                if (row.IsHeader || currentGroup == null || currentGroup.CategoryId != row.CategoryId)
                {
                    currentGroup = new NotebookIndexGroupLayout
                    {
                        CategoryId = row.CategoryId,
                        DisplayName = row.CategoryDisplayName,
                    };
                    groups.Add(currentGroup);
                }

                if (!row.IsHeader && row.Entry != null)
                {
                    currentGroup.Entries.Add(new NotebookIndexEntryLayout
                    {
                        EntryId = row.Entry.EntryId,
                        Title = row.Entry.Title,
                        IsNew = row.IsNew,
                    });
                }

                usedHeight += rowHeight;
            }

            spreads.Add(currentSpread);
            return spreads;
        }

        private static List<NotebookSpreadLayout> BuildContentSpreads(
            NotebookSection section,
            List<NotebookEntryDefinition> entries,
            NotebookBookLayoutSettings settings,
            Func<NotebookEntryDefinition, float> resolveEntryHeight = null)
        {
            List<NotebookSpreadLayout> spreads = new List<NotebookSpreadLayout>();
            if (entries == null || entries.Count == 0)
            {
                return spreads;
            }

            NotebookSpreadLayout currentSpread = new NotebookSpreadLayout
            {
                Kind = NotebookSpreadKind.Content,
                Section = section,
            };

            bool onLeftPage = true;
            float usedHeight = 0f;

            for (int i = 0; i < entries.Count; i++)
            {
                NotebookEntryDefinition entry = entries[i];
                float entryHeight = ResolveEntryHeight(entry, resolveEntryHeight);
                float requiredHeight = usedHeight <= 0f
                    ? entryHeight
                    : entryHeight + settings.contentEntrySpacing;

                if (usedHeight > 0f && usedHeight + requiredHeight > settings.contentPageHeight)
                {
                    if (onLeftPage)
                    {
                        onLeftPage = false;
                        usedHeight = 0f;
                        requiredHeight = entryHeight;
                    }
                    else
                    {
                        spreads.Add(currentSpread);
                        currentSpread = new NotebookSpreadLayout
                        {
                            Kind = NotebookSpreadKind.Content,
                            Section = section,
                        };
                        onLeftPage = true;
                        usedHeight = 0f;
                        requiredHeight = entryHeight;
                    }
                }
                else if (usedHeight <= 0f && entryHeight > settings.contentPageHeight)
                {
                    // Single entry taller than one page still gets its own page slot.
                    requiredHeight = entryHeight;
                }

                if (onLeftPage)
                {
                    currentSpread.LeftEntries.Add(entry);
                }
                else
                {
                    currentSpread.RightEntries.Add(entry);
                }

                currentSpread.SetEntryHeight(entry.EntryId, entryHeight);
                usedHeight += usedHeight <= 0f
                    ? entryHeight
                    : entryHeight + settings.contentEntrySpacing;
            }

            spreads.Add(currentSpread);
            return spreads;
        }

        private static float ResolveEntryHeight(
            NotebookEntryDefinition entry,
            Func<NotebookEntryDefinition, float> resolveEntryHeight)
        {
            if (entry == null)
            {
                return 1f;
            }

            if (resolveEntryHeight != null)
            {
                float resolved = resolveEntryHeight(entry);
                if (resolved > 1f)
                {
                    return resolved;
                }
            }

            return Mathf.Max(1f, entry.LayoutHeight);
        }

        private static NotebookSpreadLayout NewSectionDirectorySpread(NotebookSection section)
        {
            return new NotebookSpreadLayout
            {
                Kind = NotebookSpreadKind.SectionDirectory,
                Section = section,
            };
        }

        private static void ApplyIndexTargets(IEnumerable<NotebookIndexGroupLayout> groups, IReadOnlyDictionary<string, int> entryPageById)
        {
            foreach (NotebookIndexGroupLayout group in groups)
            {
                foreach (NotebookIndexEntryLayout entry in group.Entries)
                {
                    int targetPage;
                    if (entryPageById.TryGetValue(entry.EntryId, out targetPage))
                    {
                        entry.TargetPageNumber = targetPage;
                    }
                }
            }
        }
    }
}
