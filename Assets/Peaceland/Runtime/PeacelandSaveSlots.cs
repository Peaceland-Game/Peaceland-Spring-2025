namespace Peaceland
{
    /// <summary>
    /// Ten manual save slots. Files live under persistentDataPath.
    /// </summary>
    public static class PeacelandSaveSlots
    {
        public const int SlotCount = 10;
        public const string ActiveSlotPlayerPrefsKey = "peaceland.active_slot_index";
        public const string LegacyDefaultFileName = PeacelandSaveService.LegacyDefaultFileName;
        public const string SlotFilePrefix = "peaceland_save_slot_";
        public const string SlotFileSuffix = ".json";

        public static string GetSlotFileName(int slotIndex)
        {
            return SlotFilePrefix + slotIndex + SlotFileSuffix;
        }

        public static bool IsValidSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < SlotCount;
        }
    }
}
