using System;

namespace Peaceland
{
    [Serializable]
    public struct PeacelandCheckpointSummary
    {
        public int slotIndex;
        public string checkpointId;
        public string checkpointKey;
        public string displayName;
        public string sceneName;
        public string savedUtc;
        public bool isActive;

        public string GetDisplayName()
        {
            return string.IsNullOrWhiteSpace(displayName)
                ? sceneName
                : displayName;
        }

        public string GetDisplaySavedTimeLocal()
        {
            if (DateTime.TryParse(savedUtc, out DateTime utc))
            {
                return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            }

            return savedUtc ?? string.Empty;
        }
    }
}
