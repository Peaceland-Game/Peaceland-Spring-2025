using System;
using System.Collections.Generic;
using System.Linq;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Validates notebook database + optional collect wiring against the content catalog.
    /// Editor and runtime harness share this logic.
    /// </summary>
    public static class NotebookHarnessValidator
    {
        private static readonly List<NotebookController.SectionDefinition> DefaultSections = BuildDefaultSections();

        private static List<NotebookController.SectionDefinition> BuildDefaultSections()
        {
            List<NotebookController.SectionDefinition> result = new List<NotebookController.SectionDefinition>
            {
                new NotebookController.SectionDefinition { section = NotebookSection.Present, displayName = "Present" },
                new NotebookController.SectionDefinition { section = NotebookSection.Memory1, displayName = "Memory 1 (Florist)" },
                new NotebookController.SectionDefinition { section = NotebookSection.Memory2, displayName = "Memory 2 (R&J)" },
            };

            if (NotebookFeatureFlags.IncludeHiddenStatsSection)
            {
                result.Add(new NotebookController.SectionDefinition { section = NotebookSection.HiddenStats, displayName = "Hidden Stats" });
            }

            return result;
        }

        public static NotebookHarnessReport Validate(
            NotebookDatabase database,
            NotebookContentCatalog catalog,
            IReadOnlyCollection<string> wiredCollectEntryIds = null)
        {
            NotebookHarnessReport report = new NotebookHarnessReport
            {
                generatedUtc = DateTime.UtcNow.ToString("o"),
            };

            if (database == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "DB_MISSING", "NotebookDatabase is null.");
                return report;
            }

            if (catalog == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "CATALOG_MISSING", "NotebookContentCatalog is null.");
                return report;
            }

            report.databaseEntryCount = database.Entries.Count;
            report.catalogSpecCount = catalog.Specs.Count;
            report.wiredCollectCount = wiredCollectEntryIds != null ? wiredCollectEntryIds.Count : 0;

            HashSet<string> seenIds = new HashSet<string>();
            HashSet<string> wired = wiredCollectEntryIds != null
                ? new HashSet<string>(wiredCollectEntryIds.Where(id => !string.IsNullOrWhiteSpace(id)))
                : new HashSet<string>();

            for (int i = 0; i < database.Entries.Count; i++)
            {
                NotebookEntryDefinition entry = database.Entries[i];
                if (entry == null)
                {
                    report.Add(NotebookHarnessSeverity.Warning, "NULL_ENTRY", "Database contains a null entry reference.", null);
                    continue;
                }

                ValidateEntryFields(report, catalog, entry, seenIds);
            }

            foreach (NotebookContentSpec spec in catalog.GameplaySpecs())
            {
                if (spec == null || string.IsNullOrWhiteSpace(spec.entryId))
                {
                    report.Add(NotebookHarnessSeverity.Error, "SPEC_INVALID", "Catalog contains an empty gameplay spec entryId.");
                    continue;
                }

                NotebookEntryDefinition entry = database.GetEntry(spec.entryId);
                if (entry == null)
                {
                    report.Add(
                        NotebookHarnessSeverity.Error,
                        "GAMEPLAY_MISSING",
                        "Gameplay catalog entry is missing from NotebookDatabase.",
                        spec.entryId);
                    continue;
                }

                if (entry.Section != spec.section)
                {
                    report.Add(
                        NotebookHarnessSeverity.Error,
                        "SECTION_MISMATCH",
                        "Entry section " + entry.Section + " does not match catalog " + spec.section + ".",
                        spec.entryId);
                }

                if (!string.IsNullOrWhiteSpace(spec.categoryId) && entry.CategoryId != spec.categoryId)
                {
                    report.Add(
                        NotebookHarnessSeverity.Warning,
                        "CATEGORY_MISMATCH",
                        "Entry category '" + entry.CategoryId + "' does not match catalog '" + spec.categoryId + "'.",
                        spec.entryId);
                }

                if (spec.requireCollectTrigger && wiredCollectEntryIds != null && !wired.Contains(spec.entryId))
                {
                    report.Add(
                        NotebookHarnessSeverity.Error,
                        "COLLECT_UNWIRED",
                        "Catalog requires a collect trigger" +
                        (string.IsNullOrWhiteSpace(spec.collectSceneName) ? string.Empty : " in scene " + spec.collectSceneName) + ".",
                        spec.entryId);
                }
            }

            ValidateLayoutSimulation(report, database);

            if (report.Issues.Count == 0)
            {
                report.Add(NotebookHarnessSeverity.Info, "OK", "All harness checks passed.");
            }

            return report;
        }

        private static void ValidateEntryFields(
            NotebookHarnessReport report,
            NotebookContentCatalog catalog,
            NotebookEntryDefinition entry,
            HashSet<string> seenIds)
        {
            string entryId = entry.EntryId;
            if (string.IsNullOrWhiteSpace(entryId))
            {
                report.Add(NotebookHarnessSeverity.Error, "EMPTY_ID", "Entry has empty entryId.", entry.name);
                return;
            }

            if (!seenIds.Add(entryId))
            {
                report.Add(NotebookHarnessSeverity.Error, "DUPLICATE_ID", "Duplicate entryId in database.", entryId);
            }

            NotebookContentKind kind = catalog.ClassifyEntryId(entryId);
            NotebookContentSpec spec = catalog.FindSpec(entryId);

            if (spec == null && catalog.WarnOnUnknownEntries && kind == NotebookContentKind.TestHarness)
            {
                report.Add(
                    NotebookHarnessSeverity.Warning,
                    "ORPHAN_ENTRY",
                    "Entry exists in database but is not in NotebookContentCatalog (not gameplay contract).",
                    entryId);
            }

            if (kind == NotebookContentKind.PaginationDummy)
            {
                if (entry.TheoreticalOrder <= 0)
                {
                    report.Add(NotebookHarnessSeverity.Warning, "DUMMY_ORDER", "Pagination dummy should have theoreticalOrder > 0.", entryId);
                }

                return;
            }

            bool isGameplay = kind == NotebookContentKind.Gameplay;
            bool requireBody = spec == null || spec.requireBodyText;

            if (string.IsNullOrWhiteSpace(entry.Title))
            {
                report.Add(NotebookHarnessSeverity.Error, "EMPTY_TITLE", "Entry title is empty.", entryId);
            }

            if (requireBody && isGameplay && string.IsNullOrWhiteSpace(entry.BodyText))
            {
                report.Add(NotebookHarnessSeverity.Error, "EMPTY_BODY", "Gameplay entry bodyText is empty.", entryId);
            }

            if (isGameplay && entry.TheoreticalOrder <= 0)
            {
                report.Add(NotebookHarnessSeverity.Error, "ORDER_ZERO", "Gameplay entry theoreticalOrder must be > 0.", entryId);
            }

            if (entry.LayoutHeight < 80f)
            {
                report.Add(NotebookHarnessSeverity.Warning, "LAYOUT_HEIGHT", "layoutHeight below 80 may break pagination.", entryId);
            }

            if (isGameplay && string.IsNullOrWhiteSpace(entry.CategoryId))
            {
                report.Add(NotebookHarnessSeverity.Warning, "EMPTY_CATEGORY", "Gameplay entry categoryId is empty.", entryId);
            }
        }

        private static void ValidateLayoutSimulation(NotebookHarnessReport report, NotebookDatabase database)
        {
            ValidateHiddenStatsFallback(report, database);

            HashSet<string> allIds = new HashSet<string>();
            for (int i = 0; i < database.Entries.Count; i++)
            {
                NotebookEntryDefinition entry = database.Entries[i];
                if (entry != null && !string.IsNullOrWhiteSpace(entry.EntryId))
                {
                    allIds.Add(entry.EntryId);
                }
            }

            bool IsCollected(string id) => allIds.Contains(id);
            bool IsNew(string id) => false;

            NotebookBookLayout layout = NotebookBookLayoutBuilder.Build(
                database,
                DefaultSections,
                IsCollected,
                IsNew,
                new NotebookBookLayoutSettings(),
                entry => entry != null ? entry.LayoutHeight : 220f);

            if (layout.Spreads.Count < 2)
            {
                report.Add(NotebookHarnessSeverity.Warning, "LAYOUT_THIN", "Simulated layout has fewer than 2 spreads when all entries collected.");
            }

            foreach (KeyValuePair<string, int> pair in layout.EntryPageById)
            {
                if (pair.Value <= 0)
                {
                    report.Add(NotebookHarnessSeverity.Warning, "LAYOUT_PAGE", "Entry did not receive a page number in layout simulation.", pair.Key);
                }
            }
        }

        private static void ValidateHiddenStatsFallback(NotebookHarnessReport report, NotebookDatabase database)
        {
            if (!NotebookFeatureFlags.IncludeHiddenStatsSection)
            {
                return;
            }

            List<NotebookController.SectionDefinition> legacySections = new List<NotebookController.SectionDefinition>
            {
                new NotebookController.SectionDefinition { section = NotebookSection.Present, displayName = "Present" },
                new NotebookController.SectionDefinition { section = NotebookSection.Memory1, displayName = "Memory 1 (Florist)" },
                new NotebookController.SectionDefinition { section = NotebookSection.Memory2, displayName = "Memory 2 (R&J)" },
            };

            NotebookBookLayout layout = NotebookBookLayoutBuilder.Build(
                database,
                NotebookController.ResolveLayoutSections(legacySections),
                id => false,
                id => false);

            bool hasHiddenStatsSpread = layout.Spreads.Any(spread =>
                spread.Kind == NotebookSpreadKind.StatEditor
                && spread.Section == NotebookSection.HiddenStats);
            if (!hasHiddenStatsSpread)
            {
                report.Add(
                    NotebookHarnessSeverity.Error,
                    "HIDDEN_STATS_FALLBACK",
                    "Hidden Stats tab has no StatEditor spread when the serialized section list is stale.");
            }
        }

        public static void CollectAllGameplayEntries(NotebookContentCatalog catalog)
        {
            if (catalog == null)
            {
                return;
            }

            foreach (NotebookContentSpec spec in catalog.GameplaySpecs())
            {
                if (spec == null || string.IsNullOrWhiteSpace(spec.entryId))
                {
                    continue;
                }

                NotebookGlobalBridge.CollectEntry(spec.entryId);
            }
        }
    }
}
