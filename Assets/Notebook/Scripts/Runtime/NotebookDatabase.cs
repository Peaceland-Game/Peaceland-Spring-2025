using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Peaceland.Notebook
{
    public sealed class NotebookCategoryDefinition
    {
        public string CategoryId;
        public string DisplayName;
        public int SortOrder;
    }

    [CreateAssetMenu(fileName = "NotebookDatabase", menuName = "Peaceland/Notebook/Database")]
    public class NotebookDatabase : ScriptableObject
    {
        [SerializeField] private List<NotebookEntryDefinition> entries = new List<NotebookEntryDefinition>();

        public IReadOnlyList<NotebookEntryDefinition> Entries => entries;

        public NotebookEntryDefinition GetEntry(string entryId)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return null;
            }

            return entries.FirstOrDefault(entry => entry != null && entry.EntryId == entryId);
        }

        public List<NotebookEntryDefinition> GetEntriesForSection(NotebookSection section)
        {
            return entries
                .Where(entry => entry != null && entry.Section == section)
                .OrderBy(entry => entry.TheoreticalOrder)
                .ThenBy(entry => entry.SortOrder)
                .ThenBy(entry => entry.Title)
                .ToList();
        }

        public List<NotebookEntryDefinition> GetEntriesForGroup(NotebookSection section, string groupId)
        {
            return entries
                .Where(entry =>
                    entry != null &&
                    entry.Section == section &&
                    entry.GroupId == groupId)
                .OrderBy(entry => entry.TheoreticalOrder)
                .ThenBy(entry => entry.SortOrder)
                .ThenBy(entry => entry.Title)
                .ToList();
        }

        public List<NotebookEntryDefinition> GetEntriesForSubgroup(NotebookSection section, string groupId, string subgroupId)
        {
            return entries
                .Where(entry =>
                    entry != null &&
                    entry.Section == section &&
                    entry.GroupId == groupId &&
                    entry.SubgroupId == subgroupId)
                .OrderBy(entry => entry.TheoreticalOrder)
                .ThenBy(entry => entry.SortOrder)
                .ThenBy(entry => entry.Title)
                .ToList();
        }

        public List<NotebookCategoryDefinition> GetCategoriesForSection(NotebookSection section)
        {
            return entries
                .Where(entry => entry != null && entry.Section == section)
                .GroupBy(entry => entry.CategoryId)
                .Select(group =>
                {
                    NotebookEntryDefinition first = group
                        .OrderBy(entry => entry.CategorySortOrder)
                        .ThenBy(entry => entry.CategoryDisplayName)
                        .First();
                    return new NotebookCategoryDefinition
                    {
                        CategoryId = group.Key,
                        DisplayName = first.CategoryDisplayName,
                        SortOrder = first.CategorySortOrder,
                    };
                })
                .OrderBy(category => category.SortOrder)
                .ThenBy(category => category.DisplayName)
                .ToList();
        }

        public List<NotebookEntryDefinition> GetEntriesForSectionCategory(NotebookSection section, string categoryId)
        {
            return entries
                .Where(entry =>
                    entry != null &&
                    entry.Section == section &&
                    entry.CategoryId == categoryId)
                .OrderBy(entry => entry.TheoreticalOrder)
                .ThenBy(entry => entry.SortOrder)
                .ThenBy(entry => entry.Title)
                .ToList();
        }
    }
}
