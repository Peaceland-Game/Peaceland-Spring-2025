using System;
using System.Collections.Generic;
using Peaceland;
using UnityEngine;

namespace Peaceland.Notebook
{
    [CreateAssetMenu(fileName = "NotebookEntry", menuName = "Peaceland/Notebook/Entry")]
    public class NotebookEntryDefinition : ScriptableObject
    {
        [Serializable]
        public class RecordChoice
        {
            [SerializeField] private string choiceId = "choice";
            [SerializeField] private string label = "Record this as...";
            [SerializeField] private PeacelandStatId statId = PeacelandStatId.InsightNaivety;
            [SerializeField] private int statDelta;

            public string ChoiceId => choiceId;
            public string Label => label;
            public PeacelandStatId StatId => statId;
            public int StatDelta => statDelta;
        }

        [SerializeField] private string entryId = "entry-id";
        [SerializeField] private NotebookSection section = NotebookSection.Present;
        [SerializeField] private string groupId;
        [SerializeField] private string groupDisplayName;
        [SerializeField] private int groupSortOrder;
        [SerializeField] private string subgroupId;
        [SerializeField] private string subgroupDisplayName;
        [SerializeField] private int subgroupSortOrder;
        [SerializeField] private string categoryId = "scene-collected";
        [SerializeField] private string categoryDisplayName = "scene collected";
        [SerializeField] private int categorySortOrder;
        [SerializeField] private string title = "Placeholder Title";
        [TextArea(3, 8)]
        [SerializeField] private string bodyText = "Placeholder notebook text.";
        [SerializeField] private Sprite image;
        [SerializeField] private int theoreticalOrder;
        [SerializeField] private float layoutHeight = 220f;
        [SerializeField] private int sortOrder;
        [Header("Record Choice Minigame")]
        [SerializeField] private bool requiresRecordChoice;
        [SerializeField] private string recordChoicePrompt = "How do you want to record this?";
        [SerializeField] private List<RecordChoice> recordChoices = new List<RecordChoice>();

        public string EntryId => entryId;
        public NotebookSection Section => section;
        public string GroupId => groupId;
        public string GroupDisplayName => string.IsNullOrWhiteSpace(groupDisplayName) ? groupId : groupDisplayName;
        public int GroupSortOrder => groupSortOrder;
        public string SubgroupId => subgroupId;
        public string SubgroupDisplayName => string.IsNullOrWhiteSpace(subgroupDisplayName) ? subgroupId : subgroupDisplayName;
        public int SubgroupSortOrder => subgroupSortOrder;
        public string CategoryId => categoryId;
        public string CategoryDisplayName => string.IsNullOrWhiteSpace(categoryDisplayName) ? categoryId : categoryDisplayName;
        public int CategorySortOrder => categorySortOrder;
        public string Title => title;
        public string BodyText => bodyText;
        public Sprite Image => image;
        public int TheoreticalOrder => theoreticalOrder;
        public float LayoutHeight => layoutHeight;
        public int SortOrder => sortOrder;
        public bool RequiresRecordChoice => requiresRecordChoice && recordChoices != null && recordChoices.Count >= 2;
        public string RecordChoicePrompt => string.IsNullOrWhiteSpace(recordChoicePrompt)
            ? "How do you want to record this?"
            : recordChoicePrompt;
        public IReadOnlyList<RecordChoice> RecordChoices => recordChoices;
        public bool HasGroup => !string.IsNullOrWhiteSpace(groupId);
        public bool HasSubgroup => !string.IsNullOrWhiteSpace(subgroupId);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                entryId = name;
            }

            if (string.IsNullOrWhiteSpace(groupDisplayName))
            {
                groupDisplayName = groupId;
            }

            if (string.IsNullOrWhiteSpace(subgroupDisplayName))
            {
                subgroupDisplayName = subgroupId;
            }

            if (string.IsNullOrWhiteSpace(categoryDisplayName))
            {
                categoryDisplayName = categoryId;
            }

            if (layoutHeight < 1f)
            {
                layoutHeight = 1f;
            }
        }
    }
}
