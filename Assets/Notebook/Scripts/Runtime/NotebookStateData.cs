using System;
using System.Collections.Generic;

namespace Peaceland.Notebook
{
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

    [Serializable]
    public class NotebookSaveData
    {
        public List<NotebookEntryStateData> states = new List<NotebookEntryStateData>();
    }
}
