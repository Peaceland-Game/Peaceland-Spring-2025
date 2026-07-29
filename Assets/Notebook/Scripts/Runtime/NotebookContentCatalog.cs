using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Production contract for notebook content. Harness validates database + scenes against this catalog.
    /// </summary>
    [CreateAssetMenu(fileName = "NotebookContentCatalog", menuName = "Peaceland/Notebook/Content Catalog")]
    public sealed class NotebookContentCatalog : ScriptableObject
    {
        [SerializeField] private List<NotebookContentSpec> specs = new List<NotebookContentSpec>();

        [Tooltip("When true, database entries not listed here (and not DummyPage_*) emit warnings")]
        [SerializeField] private bool warnOnUnknownEntries = true;

        public IReadOnlyList<NotebookContentSpec> Specs => specs;
        public bool WarnOnUnknownEntries => warnOnUnknownEntries;

        public NotebookContentSpec FindSpec(string entryId)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return null;
            }

            return specs.FirstOrDefault(spec => spec != null && spec.entryId == entryId);
        }

        public NotebookContentKind ClassifyEntryId(string entryId)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return NotebookContentKind.TestHarness;
            }

            NotebookContentSpec spec = FindSpec(entryId);
            if (spec != null)
            {
                return spec.kind;
            }

            if (entryId.StartsWith("NotebookEntry_DummyPage_"))
            {
                return NotebookContentKind.PaginationDummy;
            }

            return NotebookContentKind.TestHarness;
        }

        public IEnumerable<NotebookContentSpec> GameplaySpecs()
        {
            return specs.Where(spec => spec != null && spec.kind == NotebookContentKind.Gameplay);
        }
    }
}
