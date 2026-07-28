using System;

namespace Peaceland
{
    /// <summary>
    /// Read-only metadata for one save slot (title screen UI).
    /// </summary>
    [Serializable]
    public struct PeacelandSaveSlotSummary
    {
        public int slotIndex;
        public bool hasData;
        public string savedUtc;
        public string lastSceneName;
        public string displayLocationName;
        public int currentDay;
        public int kindnessCruelty;
        public int collectedNotebookEntryCount;

        public string GetDisplayLocationName()
        {
            if (!string.IsNullOrWhiteSpace(displayLocationName))
            {
                return displayLocationName;
            }

            return string.IsNullOrWhiteSpace(lastSceneName) ? "New game" : lastSceneName;
        }

        public string GetDisplaySavedTimeLocal()
        {
            if (string.IsNullOrWhiteSpace(savedUtc))
            {
                return string.Empty;
            }

            if (DateTime.TryParse(savedUtc, out DateTime utc))
            {
                return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            }

            return savedUtc;
        }
    }
}
