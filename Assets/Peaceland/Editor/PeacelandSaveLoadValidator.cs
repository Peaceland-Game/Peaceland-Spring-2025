#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System;
using Peaceland.Notebook;
using UnityEditor;
using UnityEngine;

namespace Peaceland.Editor
{
    public static class PeacelandSaveLoadValidator
    {
        [MenuItem("Peaceland/Save/Seed Loading Test Slots")]
        public static void SeedLoadingTestSlots()
        {
            int created = 0;
            created += SeedIfEmpty(1, "NoteBookTesting", "Notebook", 2, "loading-test-notebook") ? 1 : 0;
            created += SeedIfEmpty(
                2,
                "NotebookTest_FloristMinigame",
                "Florist Minigame",
                3,
                "loading-test-florist") ? 1 : 0;
            created += SeedIfEmpty(
                3,
                "NotebookTest_RandJItemCollect",
                "R&J Item Collect",
                4,
                "loading-test-randj") ? 1 : 0;

            Debug.Log(
                "[SaveLoad Seed] Created " + created
                + " empty test slot(s). Existing slot files were preserved.");
        }

        [MenuItem("Peaceland/Save/Validate Save Load Scene")]
        public static void Validate()
        {
            List<string> failures = new List<string>();
            ValidateSlotContract(failures);
            ValidateNotebookIsolation(failures);
            ValidateSceneRegistration(failures);

            if (failures.Count > 0)
            {
                throw new System.InvalidOperationException(
                    "Save/load validation failed:\n- " + string.Join("\n- ", failures));
            }

            Debug.Log(
                "[SaveLoad Validation] PASS - 10 unique slots, isolated notebook JSON, "
                + "and enabled SaveLoad scene registration verified.");
        }

        private static void ValidateSlotContract(List<string> failures)
        {
            if (PeacelandSaveSlots.SlotCount != 10)
            {
                failures.Add("Expected 10 save slots, found " + PeacelandSaveSlots.SlotCount + ".");
            }

            HashSet<string> fileNames = new HashSet<string>();
            HashSet<string> paths = new HashSet<string>();
            for (int i = 0; i < PeacelandSaveSlots.SlotCount; i++)
            {
                fileNames.Add(PeacelandSaveSlots.GetSlotFileName(i));
                paths.Add(PeacelandSaveService.GetSlotPath(i));
            }

            if (fileNames.Count != PeacelandSaveSlots.SlotCount
                || paths.Count != PeacelandSaveSlots.SlotCount)
            {
                failures.Add("Slot file names or persistent paths are not unique.");
            }
        }

        private static void ValidateNotebookIsolation(List<string> failures)
        {
            PeacelandGameSaveData first = PeacelandGameSaveData.CreateNewGame();
            first.notebook.states.Add(CreateNotebookState("slot-a-entry"));

            PeacelandGameSaveData second = PeacelandGameSaveData.CreateNewGame();
            second.notebook.states.Add(CreateNotebookState("slot-b-entry"));

            PeacelandGameSaveData firstRoundTrip = JsonUtility.FromJson<PeacelandGameSaveData>(
                JsonUtility.ToJson(first));
            PeacelandGameSaveData secondRoundTrip = JsonUtility.FromJson<PeacelandGameSaveData>(
                JsonUtility.ToJson(second));

            firstRoundTrip.notebook.states[0].entryId = "slot-a-mutated";
            bool isolated = secondRoundTrip.notebook.states.Count == 1
                && secondRoundTrip.notebook.states[0].entryId == "slot-b-entry";
            if (!isolated)
            {
                failures.Add("Notebook state leaked between two serialized save documents.");
            }
        }

        private static NotebookEntryStateData CreateNotebookState(string entryId)
        {
            return new NotebookEntryStateData
            {
                entryId = entryId,
                isCollected = true,
            };
        }

        private static bool SeedIfEmpty(
            int slotIndex,
            string sceneName,
            string locationName,
            int day,
            string notebookEntryId)
        {
            string path = PeacelandSaveService.GetSlotPath(slotIndex);
            if (File.Exists(path))
            {
                Debug.LogWarning(
                    "[SaveLoad Seed] Save " + (slotIndex + 1) + " already exists; skipped.");
                return false;
            }

            PeacelandGameSaveData data = PeacelandGameSaveData.CreateNewGame();
            data.savedUtc = DateTime.UtcNow.ToString("o");
            data.progress.lastSceneName = sceneName;
            data.progress.displayLocationName = locationName;
            data.progress.currentDay = day;
            data.notebook.states.Add(CreateNotebookState(notebookEntryId));

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonUtility.ToJson(data, true));
            return true;
        }

        private static void ValidateSceneRegistration(List<string> failures)
        {
            if (!File.Exists(PeacelandSaveLoadSceneEditor.ScenePath))
            {
                failures.Add("SaveLoad.unity is missing.");
                return;
            }

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == PeacelandSaveLoadSceneEditor.ScenePath && scene.enabled)
                {
                    return;
                }
            }

            failures.Add("SaveLoad.unity is not enabled in Build Settings.");
        }
    }
}
#endif
