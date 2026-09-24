#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Peaceland.Editor
{
    [InitializeOnLoad]
    public static class PeacelandCheckpointClosedLoopTest
    {
        private const string Key = "Peaceland.CheckpointClosedLoop.";
        private const string SaveLoadScenePath = "Assets/Peaceland/Scenes/SaveLoad.unity";
        private const string SaveLoadSceneName = "SaveLoad";
        private const double SceneWaitSeconds = 0.5d;

        static PeacelandCheckpointClosedLoopTest()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        [MenuItem("Peaceland/Save/Run Every Checkpoint Closed Loop")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[SaveLoad Closed Loop] Start this test from Edit Mode.");
                return;
            }

            PeacelandCheckpointSceneAuditor.ValidateSceneMatrix();
            List<string> scenes =
                PeacelandCheckpointSceneAuditor.GetEnabledCheckpointSceneNames();
            int testSlot = FindEmptySlot();
            if (testSlot < 0)
            {
                throw new InvalidOperationException(
                    "All 10 save slots are occupied. No save file was changed.");
            }

            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.isDirty)
            {
                throw new InvalidOperationException(
                    "Save or discard changes in '" + activeScene.name + "' before testing.");
            }

            SessionState.SetBool(Key + "Running", true);
            SessionState.SetString(Key + "Scenes", string.Join("|", scenes));
            SessionState.SetString(Key + "OriginalScene", activeScene.path);
            SessionState.SetInt(Key + "OriginalSlot",
                PlayerPrefs.GetInt(PeacelandSaveSlots.ActiveSlotPlayerPrefsKey, -1));
            SessionState.SetInt(Key + "TestSlot", testSlot);
            SessionState.SetInt(Key + "Index", 0);
            SessionState.SetInt(Key + "Phase", 0);
            SessionState.SetString(Key + "Failure", string.Empty);
            SessionState.SetFloat(Key + "ReadyAt", 0f);

            EditorSceneManager.OpenScene(SaveLoadScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key + "Running", false))
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                string originalScene = SessionState.GetString(Key + "OriginalScene", string.Empty);
                if (!string.IsNullOrWhiteSpace(originalScene) && File.Exists(originalScene))
                {
                    EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
                }

                string failure = SessionState.GetString(Key + "Failure", string.Empty);
                int count = GetScenes().Length;
                ClearSession();

                if (string.IsNullOrWhiteSpace(failure))
                {
                    Debug.Log("[SaveLoad Closed Loop] PASS - " + count
                        + " checkpoint scene(s) saved to disk and reloaded through SaveLoad.");
                }
                else
                {
                    Debug.LogError("[SaveLoad Closed Loop] FAIL - " + failure);
                }
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Key + "Running", false)
                || !EditorApplication.isPlaying
                || EditorApplication.isPaused)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup
                < SessionState.GetFloat(Key + "ReadyAt", 0f))
            {
                return;
            }

            try
            {
                Advance();
            }
            catch (Exception exception)
            {
                FinishWithFailure(exception.Message);
            }
        }

        private static void Advance()
        {
            string[] scenes = GetScenes();
            int index = SessionState.GetInt(Key + "Index", 0);
            int phase = SessionState.GetInt(Key + "Phase", 0);
            int testSlot = SessionState.GetInt(Key + "TestSlot", -1);

            if (index >= scenes.Length)
            {
                Finish();
                return;
            }

            PeacelandGameBootstrap.EnsureExists();
            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            string expectedScene = scenes[index];

            switch (phase)
            {
                case 0:
                    saveService.ActivateSlotAndStartNewGame(testSlot);
                    Load(expectedScene, 1);
                    break;
                case 1:
                    RequireActiveScene(expectedScene);
                    RequireCheckpointOnDisk(saveService, testSlot, expectedScene);
                    Load(SaveLoadSceneName, 2);
                    break;
                case 2:
                    RequireActiveScene(SaveLoadSceneName);
                    if (!saveService.ActivateSlotAndContinue(testSlot))
                    {
                        throw new InvalidOperationException(
                            "Save " + (testSlot + 1) + " could not be activated.");
                    }

                    string restoredScene = saveService.GetLastSceneName();
                    if (restoredScene != expectedScene)
                    {
                        throw new InvalidOperationException(
                            expectedScene + " restored checkpoint '" + restoredScene + "'.");
                    }

                    Load(restoredScene, 3);
                    break;
                case 3:
                    RequireActiveScene(expectedScene);
                    Debug.Log("[SaveLoad Closed Loop] PASS " + (index + 1)
                        + "/" + scenes.Length + " - " + expectedScene);
                    SessionState.SetInt(Key + "Index", index + 1);
                    SessionState.SetInt(Key + "Phase", 0);
                    break;
            }
        }

        private static void RequireCheckpointOnDisk(
            PeacelandSaveService saveService,
            int slotIndex,
            string expectedScene)
        {
            string actualCheckpoint = saveService.GetLastSceneName();
            if (actualCheckpoint != expectedScene)
            {
                throw new InvalidOperationException(
                    expectedScene + " did not update the in-memory checkpoint; found '"
                    + actualCheckpoint + "'.");
            }

            if (!saveService.TryGetSlotSummary(slotIndex, out PeacelandSaveSlotSummary summary)
                || !summary.hasData
                || summary.lastSceneName != expectedScene)
            {
                throw new InvalidOperationException(
                    expectedScene + " did not update the checkpoint save file; found '"
                    + summary.lastSceneName + "'.");
            }
        }

        private static void RequireActiveScene(string expectedScene)
        {
            string actualScene = SceneManager.GetActiveScene().name;
            if (actualScene != expectedScene)
            {
                throw new InvalidOperationException(
                    "Expected scene '" + expectedScene + "', found '" + actualScene + "'.");
            }
        }

        private static void Load(string sceneName, int nextPhase)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                throw new InvalidOperationException(
                    "Scene '" + sceneName + "' is not loadable from Build Settings.");
            }

            SessionState.SetInt(Key + "Phase", nextPhase);
            SessionState.SetFloat(
                Key + "ReadyAt",
                (float)(EditorApplication.timeSinceStartup + SceneWaitSeconds));
            SceneManager.LoadScene(sceneName);
        }

        private static void Finish()
        {
            CleanupTestSlot();
            EditorApplication.ExitPlaymode();
        }

        private static void FinishWithFailure(string message)
        {
            SessionState.SetString(Key + "Failure", message);
            CleanupTestSlot();
            EditorApplication.ExitPlaymode();
        }

        private static void CleanupTestSlot()
        {
            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            int testSlot = SessionState.GetInt(Key + "TestSlot", -1);
            if (testSlot >= 0)
            {
                saveService.DeleteSlot(testSlot);
            }

            int originalSlot = SessionState.GetInt(Key + "OriginalSlot", -1);
            if (originalSlot >= 0)
            {
                saveService.ActivateSlotAndContinue(originalSlot);
            }
            else
            {
                saveService.ClearActiveSlotSelection();
            }
        }

        private static int FindEmptySlot()
        {
            for (int i = PeacelandSaveSlots.SlotCount - 1; i >= 0; i--)
            {
                if (!File.Exists(PeacelandSaveService.GetSlotPath(i)))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string[] GetScenes()
        {
            string value = SessionState.GetString(Key + "Scenes", string.Empty);
            return value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static void ClearSession()
        {
            SessionState.EraseBool(Key + "Running");
            SessionState.EraseString(Key + "Scenes");
            SessionState.EraseString(Key + "OriginalScene");
            SessionState.EraseInt(Key + "OriginalSlot");
            SessionState.EraseInt(Key + "TestSlot");
            SessionState.EraseInt(Key + "Index");
            SessionState.EraseInt(Key + "Phase");
            SessionState.EraseString(Key + "Failure");
            SessionState.EraseFloat(Key + "ReadyAt");
        }
    }
}
#endif
