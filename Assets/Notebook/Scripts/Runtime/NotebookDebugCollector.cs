using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Peaceland.Notebook
{
    public class NotebookDebugCollector : MonoBehaviour
    {
        [SerializeField] private NotebookController notebookController;
        [SerializeField] private List<NotebookEntryDefinition> entries = new List<NotebookEntryDefinition>();

        public void Configure(NotebookController controller, IEnumerable<NotebookEntryDefinition> debugEntries)
        {
            notebookController = controller;
            entries = debugEntries != null ? debugEntries.Where(entry => entry != null).ToList() : new List<NotebookEntryDefinition>();
        }

        public void CollectAll()
        {
            if (notebookController == null)
            {
                Debug.LogWarning($"{nameof(NotebookDebugCollector)} on {name} is missing a controller reference.", this);
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                NotebookEntryDefinition entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                notebookController.CollectEntry(entry.EntryId);
            }
        }
    }
}
