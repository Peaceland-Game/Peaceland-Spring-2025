using System;

namespace Peaceland
{
    /// <summary>
    /// Ten manual save slots. Files live under persistentDataPath.
    /// </summary>
    public static class PeacelandSaveSlots
    {
        // Kept for old scenes/tests. This is now the initial number shown, not a maximum.
        public const int SlotCount = 10;
        public const int InitialVisibleSlotCount = SlotCount;
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
            return slotIndex >= 0;
        }

        public static bool TryParseSlotIndex(string fileName, out int slotIndex)
        {
            slotIndex = -1;
            if (string.IsNullOrWhiteSpace(fileName)
                || !fileName.StartsWith(SlotFilePrefix, StringComparison.Ordinal)
                || !fileName.EndsWith(SlotFileSuffix, StringComparison.Ordinal))
            {
                return false;
            }

            int numberLength =
                fileName.Length - SlotFilePrefix.Length - SlotFileSuffix.Length;
            return numberLength > 0
                && int.TryParse(
                    fileName.Substring(SlotFilePrefix.Length, numberLength),
                    out slotIndex)
                && slotIndex >= 0;
        }
    }
}
