#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Peaceland.Notebook;
using UnityEditor;
using UnityEngine;

namespace Peaceland.Editor
{
    public static class PeacelandExpandedSaveLoadValidator
    {
        [MenuItem("Peaceland/Save/Run Expanded Closed Loop (Play Mode)")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Enter Play Mode in SaveLoad, then run this validation.");
            }

            PeacelandSaveService service = PeacelandSaveService.Instance;
            int previousSlot = service.ActiveSlotIndex;
            int testSlot = FindUnusedSlot(12);
            int legacySlot = -1;

            try
            {
                service.ActivateSlotAndStartNewGame(testSlot);
                string firstId = Capture(
                    service,
                    1,
                    "expanded-checkpoint-a",
                    "Arrival",
                    "DemoDisclaimer");
                Capture(
                    service,
                    2,
                    "expanded-checkpoint-b",
                    "First Decision",
                    "DemoStart");
                Capture(
                    service,
                    3,
                    "expanded-checkpoint-c",
                    "Notebook Review",
                    "NoteBookTesting");

                Require(File.Exists(PeacelandSaveService.GetSlotPath(testSlot)),
                    "slot file above index 9 was not written");
                Require(
                    service.TryGetCheckpointSummaries(
                        testSlot,
                        out List<PeacelandCheckpointSummary> checkpoints)
                    && checkpoints.Count == 3,
                    "expected three checkpoints in one save");
                Require(
                    PeacelandSaveService.GetVisibleSlotCount(10, 2) >= testSlot + 3,
                    "dynamic slot scan did not include empty-slot buffer");

                service.ClearActiveSlotSelection();
                Require(service.ActivateSlotAtCheckpoint(testSlot, firstId),
                    "checkpoint could not be restored from disk");
                Require(
                    service.GetStat(PeacelandStatId.KindnessCruelty) == 1,
                    "stat snapshot did not roll back");
                Require(service.GetLastSceneName() == "DemoDisclaimer",
                    "scene checkpoint did not roll back");
                Require(
                    service.GetNotebookData().states.Any(
                        state => state.entryId == "expanded-checkpoint-a"),
                    "notebook snapshot did not roll back");
                Require(
                    service.TryGetProgressInt("expanded/cursor", out int cursor)
                    && cursor == 1,
                    "integer scene progress did not roll back");

                PeacelandSaveSlotPanel slotPanel =
                    UnityEngine.Object.FindFirstObjectByType<PeacelandSaveSlotPanel>(
                        FindObjectsInactive.Include);
                Require(slotPanel != null, "SaveLoad scene has no slot panel");
                slotPanel.Refresh();
                int visibleCards =
                    slotPanel.GetComponentsInChildren<PeacelandSaveSlotEntryView>(true).Length;
                Require(visibleCards >= testSlot + 3,
                    "runtime UI did not create enough save cards");

                PeacelandCheckpointListPanel checkpointPanel =
                    UnityEngine.Object.FindFirstObjectByType<PeacelandCheckpointListPanel>(
                        FindObjectsInactive.Include);
                Require(checkpointPanel != null, "checkpoint panel is missing");
                Require(checkpointPanel.Show(testSlot, checkpoints, null),
                    "checkpoint panel could not display checkpoint history");
                int checkpointCards =
                    checkpointPanel.GetComponentsInChildren<PeacelandCheckpointEntryView>(true).Length;
                Require(checkpointCards >= 3,
                    "checkpoint panel did not create enough entries");
                checkpointPanel.Hide();

                legacySlot = FindUnusedSlot(testSlot + 1);
                ValidateLegacySlotUpgrade(service, legacySlot);

                Debug.Log(
                    "[Expanded SaveLoad] PASS | slot=" + testSlot
                    + " | checkpoints=3 | legacy v2 upgraded"
                    + " | disk/stat/notebook/scene/UI restored.");
            }
            finally
            {
                service.DeleteSlot(testSlot);
                if (legacySlot >= 0)
                {
                    service.DeleteSlot(legacySlot);
                }

                if (previousSlot >= 0)
                {
                    service.ActivateSlotAndContinue(previousSlot);
                }
                else
                {
                    service.ClearActiveSlotSelection();
                }
            }
        }

        [MenuItem("Peaceland/Save/Run R&J Internal Save Point Smoke (Play Mode)")]
        public static void RunRjInternalSavePointSmoke()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Open R&JMemoryScene, enter Play Mode, then run this validation.");
            }

            PeacelandMinigameProgressBridge bridge =
                UnityEngine.Object.FindFirstObjectByType<PeacelandMinigameProgressBridge>(
                    FindObjectsInactive.Include);
            Require(bridge != null && bridge.ProgressKey == "rj-memory",
                "R&J progress bridge was not found");
            Require(bridge.IsReady, "R&J progress bridge is not ready");

            PeacelandSaveService service = PeacelandSaveService.Instance;
            int previousSlot = service.ActiveSlotIndex;
            int testSlot = FindUnusedSlot(20);
            try
            {
                service.ActivateSlotAndStartNewGame(testSlot);
                service.SetProgressInt("rj-memory/minigame", 2);
                service.SetProgressInt("rj-memory/order", 0);
                Require(bridge.RestoreProgress(), "R&J cursor could not restore");
                Require(bridge.Manager.CurrentMinigame == 2,
                    "R&J did not resume at AfterPuzzle");

                bridge.CaptureProgress();
                string earlyCheckpoint = service.CaptureCheckpoint(
                    "test/rj-after-puzzle",
                    "Test After Puzzle",
                    bridge.gameObject.scene.name);
                Require(!string.IsNullOrWhiteSpace(earlyCheckpoint),
                    "R&J test checkpoint could not be captured");

                bridge.Manager.RestoreProgress(4, 0);
                bridge.PollProgress();
                Require(
                    service.TryGetCheckpointSummaries(
                        testSlot,
                        out List<PeacelandCheckpointSummary> checkpoints)
                    && checkpoints.Any(item => item.checkpointKey == "rj/ruzica-house"),
                    "Ruzica milestone did not create a checkpoint");

                Require(
                    service.ActivateSlotAtCheckpoint(testSlot, earlyCheckpoint),
                    "earlier R&J checkpoint could not be selected");
                Require(
                    service.TryGetProgressInt(
                        "rj-memory/minigame",
                        out int restoredCursor)
                    && restoredCursor == 2,
                    "earlier R&J cursor was not restored from checkpoint");
                Require(bridge.RestoreProgress(), "R&J manager could not apply old cursor");
                Require(bridge.Manager.CurrentMinigame == 2,
                    "R&J manager did not return to AfterPuzzle");

                Debug.Log(
                    "[Internal Save Point] PASS | R&J 2 -> Ruzica milestone -> 2 restored.");
            }
            finally
            {
                service.DeleteSlot(testSlot);
                if (previousSlot >= 0)
                {
                    service.ActivateSlotAndContinue(previousSlot);
                }
                else
                {
                    service.ClearActiveSlotSelection();
                }
            }
        }

        [MenuItem("Peaceland/Save/Run Flower Internal Save Point Smoke (Play Mode)")]
        public static void RunFlowerInternalSavePointSmoke()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Open FlowerMemoryScene, enter Play Mode, then run this validation.");
            }

            PeacelandMinigameProgressBridge bridge =
                UnityEngine.Object.FindFirstObjectByType<PeacelandMinigameProgressBridge>(
                    FindObjectsInactive.Include);
            Require(bridge != null && bridge.ProgressKey == "flower-memory",
                "Flower progress bridge was not found");
            Require(bridge.IsReady, "Flower progress bridge is not ready");

            PeacelandMinigameCheckpointMilestone borisMilestone =
                UnityEngine.Object.FindObjectsByType<PeacelandMinigameCheckpointMilestone>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .FirstOrDefault(
                        item => item.gameObject.name == "Save Point - Boris Visit");
            Require(borisMilestone != null, "Boris save point was not found");
            int targetIndex =
                bridge.Manager.IndexOfMinigame(borisMilestone.TargetMinigame);
            Require(targetIndex >= 0, "Boris minigame is not in the manager sequence");

            PeacelandSaveService service = PeacelandSaveService.Instance;
            int previousSlot = service.ActiveSlotIndex;
            int testSlot = FindUnusedSlot(20);
            try
            {
                service.ActivateSlotAndStartNewGame(testSlot);
                bridge.Manager.RestoreProgress(targetIndex, 0);
                bridge.PollProgress();

                Require(
                    service.TryGetCheckpointSummaries(
                        testSlot,
                        out List<PeacelandCheckpointSummary> checkpoints)
                    && checkpoints.Any(item => item.checkpointKey == "flower/boris-visit"),
                    "Boris milestone did not create a checkpoint");
                PeacelandCheckpointSummary checkpoint =
                    checkpoints.First(item => item.checkpointKey == "flower/boris-visit");
                Require(
                    service.ActivateSlotAtCheckpoint(
                        testSlot,
                        checkpoint.checkpointId),
                    "Boris checkpoint could not be selected");
                Require(
                    service.TryGetProgressInt(
                        "flower-memory/minigame",
                        out int restoredCursor)
                    && restoredCursor == targetIndex,
                    "Flower cursor was not stored in Boris checkpoint");
                Require(bridge.RestoreProgress(),
                    "Flower manager could not apply Boris cursor");
                Require(bridge.Manager.CurrentMinigame == targetIndex,
                    "Flower manager did not resume at Boris");

                Debug.Log(
                    "[Internal Save Point] PASS | Flower Boris milestone restored.");
            }
            finally
            {
                service.DeleteSlot(testSlot);
                if (previousSlot >= 0)
                {
                    service.ActivateSlotAndContinue(previousSlot);
                }
                else
                {
                    service.ClearActiveSlotSelection();
                }
            }
        }

        private static string Capture(
            PeacelandSaveService service,
            int statValue,
            string notebookEntryId,
            string checkpointName,
            string sceneName)
        {
            service.SetStat(PeacelandStatId.KindnessCruelty, statValue);
            service.SetProgressInt("expanded/cursor", statValue);
            NotebookSaveData notebook = new NotebookSaveData();
            notebook.states.Add(new NotebookEntryStateData
            {
                entryId = notebookEntryId,
                isCollected = true,
                isReviewed = true,
            });
            Require(service.SaveNotebookToActiveSlot(notebook),
                "notebook snapshot could not be saved");
            string checkpointId = service.CaptureCheckpoint(
                "expanded/" + notebookEntryId,
                checkpointName,
                sceneName);
            Require(!string.IsNullOrWhiteSpace(checkpointId),
                "checkpoint could not be captured");
            return checkpointId;
        }

        /// <summary>
        /// Gameplay that runs before anyone picked a slot - a scene opened straight from
        /// the Editor, a tester who cleared the title screen - falls back to slot 0. The
        /// fallback used to load slot 0's file over the document that had just been
        /// changed, so the first stat delta, collected entry and scene name were lost.
        /// </summary>
        [MenuItem("Peaceland/Save/Run Unbound Slot Fallback Check (Play Mode)")]
        public static void RunUnboundSlotFallback()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "Enter Play Mode in SaveLoad, then run this validation.");
            }

            PeacelandSaveService service = PeacelandSaveService.Instance;
            int previousSlot = service.ActiveSlotIndex;
            string slotZeroPath = PeacelandSaveService.GetSlotPath(0);
            // The fallback slot is always 0, so a real slot 0 is parked and put back.
            string parked = File.Exists(slotZeroPath) ? File.ReadAllText(slotZeroPath) : null;

            try
            {
                PeacelandGameSaveData existing = PeacelandGameSaveData.CreateNewGame();
                existing.savedUtc = DateTime.UtcNow.ToString("o");
                existing.stats.Set(PeacelandStatId.KindnessCruelty, 2);
                existing.notebook.states.Add(new NotebookEntryStateData
                {
                    entryId = "unbound-existing-entry",
                    isCollected = true,
                    collectedOrder = 1,
                });
                File.WriteAllText(slotZeroPath, JsonUtility.ToJson(existing, true));

                service.ClearActiveSlotSelection();
                Require(!service.HasActiveSlot, "the slot selection did not clear");

                service.AddStat(PeacelandStatId.KindnessCruelty, 1);
                Require(service.HasActiveSlot && service.ActiveSlotIndex == 0,
                    "the first stat change did not bind slot 0");
                Require(service.GetStat(PeacelandStatId.KindnessCruelty) == 3,
                    "the stat delta made before binding was lost; K/C is "
                    + service.GetStat(PeacelandStatId.KindnessCruelty) + ", expected 3");

                service.ClearActiveSlotSelection();
                Require(NotebookSaveUtility.TryMarkCollected("unbound-new-entry"),
                    "the unbound collect was refused");
                NotebookSaveData notebook = service.GetNotebookData();
                Require(notebook.states.Any(state => state.entryId == "unbound-new-entry" && state.isCollected),
                    "the entry collected before binding was lost");
                Require(notebook.states.Any(state => state.entryId == "unbound-existing-entry" && state.isCollected),
                    "binding after a collect wiped the entries slot 0 already had");

                // The path a scene load takes: bootstrap -> progress -> checkpoint.
                service.ClearActiveSlotSelection();
                PeacelandProgress.Instance.SetCurrentSceneCheckpoint("UnboundScene");
                Require(service.GetLastSceneName() == "UnboundScene",
                    "the scene name recorded before binding was lost");
                Require(
                    service.TryGetCheckpointSummaries(0, out List<PeacelandCheckpointSummary> unboundCheckpoints)
                    && unboundCheckpoints.Any(checkpoint => checkpoint.sceneName == "UnboundScene"),
                    "the scene load before binding recorded no checkpoint");

                PeacelandGameSaveData disk =
                    JsonUtility.FromJson<PeacelandGameSaveData>(File.ReadAllText(slotZeroPath));
                Require(disk.stats.Get(PeacelandStatId.KindnessCruelty) == 3
                        && disk.notebook.states.Count == 2
                        && disk.progress.lastSceneName == "UnboundScene",
                    "slot 0 on disk does not hold the changes made before binding");

                Debug.Log("[Unbound Slot Fallback] PASS | stat, notebook entry and scene name survive binding to slot 0.");
            }
            finally
            {
                if (parked != null)
                {
                    File.WriteAllText(slotZeroPath, parked);
                }
                else if (File.Exists(slotZeroPath))
                {
                    File.Delete(slotZeroPath);
                }

                if (previousSlot >= 0)
                {
                    service.ActivateSlotAndContinue(previousSlot);
                }
                else
                {
                    service.ClearActiveSlotSelection();
                }
            }
        }

        private static int FindUnusedSlot(int minimum)
        {
            int slotIndex = Mathf.Max(10, minimum);
            while (File.Exists(PeacelandSaveService.GetSlotPath(slotIndex)))
            {
                slotIndex++;
            }

            return slotIndex;
        }

        private static void ValidateLegacySlotUpgrade(
            PeacelandSaveService service,
            int slotIndex)
        {
            PeacelandGameSaveData legacy = PeacelandGameSaveData.CreateNewGame();
            legacy.version = 2;
            legacy.savedUtc = DateTime.UtcNow.ToString("o");
            legacy.progress.lastSceneName = "DemoStart";
            legacy.stats.Set(PeacelandStatId.KindnessCruelty, -2);
            legacy.notebook.states.Add(new NotebookEntryStateData
            {
                entryId = "legacy-v2-entry",
                isCollected = true,
            });
            legacy.checkpoints = null;
            legacy.MarkOccupied();
            File.WriteAllText(
                PeacelandSaveService.GetSlotPath(slotIndex),
                JsonUtility.ToJson(legacy, true));

            Require(
                service.TryGetCheckpointSummaries(
                    slotIndex,
                    out List<PeacelandCheckpointSummary> checkpoints)
                && checkpoints.Count == 1
                && checkpoints[0].checkpointId == "legacy-latest",
                "version 2 save did not expose a legacy checkpoint");
            Require(
                service.ActivateSlotAtCheckpoint(slotIndex, "legacy-latest"),
                "version 2 legacy checkpoint could not be activated");
            Require(service.GetStat(PeacelandStatId.KindnessCruelty) == -2,
                "legacy stat snapshot was not restored");
            Require(
                service.GetNotebookData().states.Any(
                    state => state.entryId == "legacy-v2-entry"),
                "legacy notebook snapshot was not restored");
        }

        private static void Require(bool condition, string failure)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    "[Expanded SaveLoad] FAIL: " + failure);
            }
        }
    }
}
#endif
