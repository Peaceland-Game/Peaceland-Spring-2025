using System.Collections.Generic;
using System.Linq;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Cross-scene collect entry point. Use this from minigames / Yarn / world objects
    /// when the notebook UI may not be loaded yet.
    /// </summary>
    public static class NotebookGlobalBridge
    {
        private static readonly HashSet<string> PendingEntryIds = new HashSet<string>();
        private static NotebookController activeController;

        public static void RegisterController(NotebookController controller)
        {
            activeController = controller;
            FlushQueuedEntries();
        }

        public static void UnregisterController(NotebookController controller)
        {
            if (activeController == controller)
            {
                activeController = null;
            }
        }

        /// <summary>
        /// Unlocks one NotebookDatabase entry by id.
        /// If no controller is in the current scene, the id is queued and applied when the book loads.
        /// Safe to call twice; already-collected ids are ignored.
        /// </summary>
        public static void CollectEntry(string entryId)
        {
            if (string.IsNullOrWhiteSpace(entryId))
            {
                return;
            }

            if (activeController != null)
            {
                activeController.CollectEntry(entryId);
                return;
            }

            if (NotebookSaveUtility.IsCollected(entryId))
            {
                return;
            }

            if (!NotebookSaveUtility.TryMarkCollected(entryId))
            {
                return;
            }

            PendingEntryIds.Add(entryId);
        }

        public static void CollectEntries(IEnumerable<string> entryIds)
        {
            if (entryIds == null)
            {
                return;
            }

            foreach (string entryId in entryIds.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                CollectEntry(entryId);
            }
        }

        private static void FlushQueuedEntries()
        {
            if (activeController == null || PendingEntryIds.Count == 0)
            {
                return;
            }

            string[] queuedEntries = PendingEntryIds.ToArray();
            PendingEntryIds.Clear();

            for (int i = 0; i < queuedEntries.Length; i++)
            {
                activeController.CollectEntry(queuedEntries[i]);
            }
        }
    }
}
