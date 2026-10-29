using System;
using System.Collections.Generic;

namespace Peaceland.Notebook
{
    /// <summary>Per-entry flags stored in the save JSON (collected, reviewed, record-choice).</summary>
    [Serializable]
    public class NotebookEntryStateData
    {
        public string entryId;
        public bool isCollected;
        public bool isReviewed;
        public int reviewCount;
        public int collectedOrder;
        public bool recordChoiceCompleted;
        public int selectedRecordChoiceIndex = -1;
        public string selectedRecordChoiceId;
    }

    /// <summary>Notebook slice of peaceland_save_slot_N.json.</summary>
    [Serializable]
    public class NotebookSaveData
    {
        public List<NotebookEntryStateData> states = new List<NotebookEntryStateData>();
    }
}
