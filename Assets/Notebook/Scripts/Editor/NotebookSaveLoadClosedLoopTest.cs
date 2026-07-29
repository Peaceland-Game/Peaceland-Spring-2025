#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Peaceland.Notebook.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    // Reusable Editor-only runtime test; it never ships in a player build.
    [InitializeOnLoad]
    public static class NotebookSaveLoadClosedLoopTest
    {
        private const int TestSlot = 9;
        private const int ChoiceIndex = 1;
        private const string EntryId = "NotebookEntry_Memory1FloristFlower";
        private const string ChoiceId = "practical";
        private const string SourceScene = "Assets/Notebook/Scenes/NotebookTest_FloristMinigame.unity";
        private const string NotebookSceneName = "NoteBookTesting";
        private const string TransitionSceneName = "NotebookTest_RandJItemCollect";
        private const string EntryAssetPath = "Assets/Notebook/Data/NotebookEntry_Memory1FloristFlower.asset";
        private const string ReportPath = "Library/NotebookReports/save_load_closed_loop_report.md";
        private const string RequestFileName = ".notebook-closed-loop-request";
        private const string KeyPrefix = "Peaceland.Notebook.ClosedLoop.";

        private static Action scheduledAction;
        private static double scheduledAt;

        static NotebookSaveLoadClosedLoopTest()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Application.logMessageReceived -= CaptureLog;
            Application.logMessageReceived += CaptureLog;
            EditorApplication.delayCall += Resume;
        }

        [MenuItem("Peaceland/Notebook/Harness/Run Save-Load Closed Loop")]
        public static void RunFromMenu()
        {
            StartRun();
        }

        private static void Resume()
        {
            if (File.Exists(RequestPath) && !SessionState.GetBool(KeyPrefix + "Running", false))
            {
                File.Delete(RequestPath);
                StartRun();
                return;
            }

            if (!SessionState.GetBool(KeyPrefix + "Running", false))
            {
                return;
            }

            int stage = SessionState.GetInt(KeyPrefix + "Stage", 0);
            if (EditorApplication.isPlaying)
            {
                ScheduleStage(stage);
            }
            else if (!EditorApplication.isPlayingOrWillChangePlaymode && stage == 2)
            {
                Schedule(EditorApplication.EnterPlaymode, 0.5d);
            }
            else if (!EditorApplication.isPlayingOrWillChangePlaymode && stage == 3)
            {
                FinishRun();
            }
        }

        private static void StartRun()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Notebook closed-loop test requires Edit Mode to start.");
                return;
            }

            string testPath = PeacelandSaveService.GetSlotPath(TestSlot);
            if (File.Exists(testPath))
            {
                WriteStandaloneFailure("Save 10 is occupied; no file was changed.");
                return;
            }

            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.isDirty)
            {
                WriteStandaloneFailure("The active scene has unsaved changes; test stopped without changing scenes.");
                return;
            }

            ClearSession();
            SessionState.SetBool(KeyPrefix + "Running", true);
            SessionState.SetInt(KeyPrefix + "Stage", 1);
            SessionState.SetInt(KeyPrefix + "StartedStage", 0);
            SessionState.SetString(KeyPrefix + "StartedUtc", DateTime.UtcNow.ToString("o"));
            SessionState.SetString(KeyPrefix + "OriginalScene", activeScene.path);
            SessionState.SetInt(
                KeyPrefix + "OriginalSlot",
                PlayerPrefs.GetInt(PeacelandSaveSlots.ActiveSlotPlayerPrefsKey, -1));
            AddLog("Preflight PASS: Save 10 was empty; original scene and active slot were recorded.");

            EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(KeyPrefix + "Running", false))
            {
                return;
            }

            int stage = SessionState.GetInt(KeyPrefix + "Stage", 0);
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                ScheduleStage(stage);
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (stage == 2)
                {
                    Schedule(EditorApplication.EnterPlaymode, 0.75d);
                }
                else if (stage == 3)
                {
                    Schedule(FinishRun, 0.25d);
                }
            }
        }

        private static void ScheduleStage(int stage)
        {
            if (SessionState.GetInt(KeyPrefix + "StartedStage", 0) == stage)
            {
                return;
            }

            SessionState.SetInt(KeyPrefix + "StartedStage", stage);
            if (stage == 1)
            {
                Schedule(BeginFirstPlaySession, 1.5d);
            }
            else if (stage == 2)
            {
                Schedule(BeginSecondPlaySession, 1.5d);
            }
        }

        private static void BeginFirstPlaySession()
        {
            Guard(() =>
            {
                PeacelandSaveService service = PeacelandSaveService.Instance;
                service.ActivateSlotAndStartNewGame(TestSlot);
                int before = service.GetStat(PeacelandStatId.InsightNaivety);
                SessionState.SetInt(KeyPrefix + "StatBefore", before);
                Require(before == 0, "Clean-slot Insight must start at 0.");

                NotebookEntryDefinition entry =
                    AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(EntryAssetPath);
                Require(entry != null && entry.EntryId == EntryId, "Target entry asset could not be loaded.");

                NotebookCollectTrigger trigger = FindTargetTrigger();
                Require(trigger != null, "The source scene has no trigger wired to the target entry.");
                trigger.Collect();

                NotebookEntryStateData collectedState = GetState(service);
                Require(collectedState != null && collectedState.isCollected,
                    "Satellite scene trigger did not persist the target entry.");
                AddLog("Collection PASS: satellite-scene NotebookCollectTrigger persisted " + EntryId + ".");

                SceneManager.LoadScene(NotebookSceneName);
                Schedule(() =>
                {
                    Guard(() =>
                    {
                        NotebookController controller = FindController();
                        Require(controller != null && controller.HasCollected(EntryId),
                            "Notebook UI scene did not restore the collected entry.");
                        controller.OpenNotebook();
                        Schedule(() =>
                        {
                            controller.ShowSection(entry.Section);
                            Schedule(() =>
                            {
                                controller.JumpToEntry(entry.EntryId);
                                Schedule(() => SelectChoiceAndPersist(controller, service), 0.35d);
                            }, 0.35d);
                        }, 1.1d);
                    });
                }, 1.25d);
            });
        }

        private static void SelectChoiceAndPersist(NotebookController controller, PeacelandSaveService service)
        {
            Guard(() =>
            {
                Button choice = UnityEngine.Object.FindObjectsByType<Button>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .FirstOrDefault(button =>
                        button.name == "Record Choice " + (ChoiceIndex + 1)
                        && button.gameObject.activeInHierarchy);
                Require(choice != null, "The practical interpretation button was not rendered.");
                choice.onClick.Invoke();

                Schedule(() =>
                {
                    Guard(() =>
                    {
                        int after = service.GetStat(PeacelandStatId.InsightNaivety);
                        NotebookEntryStateData state = GetState(service);
                        Require(after == SessionState.GetInt(KeyPrefix + "StatBefore", 0) + 1,
                            "Practical choice did not change Insight by exactly +1.");
                        RequireState(state);
                        service.Save();

                        string path = PeacelandSaveService.GetSlotPath(TestSlot);
                        Require(File.Exists(path) && new FileInfo(path).Length > 0,
                            "The slot file was not written to disk.");
                        PeacelandGameSaveData disk =
                            JsonUtility.FromJson<PeacelandGameSaveData>(File.ReadAllText(path));
                        Require(disk != null
                                && disk.stats.Get(PeacelandStatId.InsightNaivety) == after
                                && FindState(disk.notebook) is NotebookEntryStateData diskState
                                && diskState.selectedRecordChoiceId == ChoiceId,
                            "The on-disk JSON does not contain the selected choice and stat.");

                        SessionState.SetInt(KeyPrefix + "ExpectedStat", after);
                        AddLog("Interpretation PASS: actual Record Choice 2 button selected practical; Insight 0 -> 1.");
                        AddLog("Disk save PASS: slot JSON contains collected/reviewed/practical and Insight=1.");

                        SceneManager.LoadScene(TransitionSceneName);
                        Schedule(VerifyFirstSceneTransition, 1.25d);
                    });
                }, 0.3d);
            });
        }

        private static void VerifyFirstSceneTransition()
        {
            Guard(() =>
            {
                VerifyLoadedState("Scene transition");
                AddLog("Scene transition PASS: notebook choice and stat survived loading "
                    + TransitionSceneName + ".");
                SessionState.SetInt(KeyPrefix + "Stage", 2);
                SessionState.SetInt(KeyPrefix + "StartedStage", 0);
                EditorApplication.ExitPlaymode();
            });
        }

        private static void BeginSecondPlaySession()
        {
            Guard(() =>
            {
                PeacelandSaveService service = PeacelandSaveService.Instance;
                service.Load();
                VerifyLoadedState("Play Mode restart");
                AddLog("Play Mode restart PASS: active slot reloaded collected/reviewed/practical and Insight=1.");

                NotebookEntryDefinition entry =
                    AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(EntryAssetPath);
                Require(entry != null, "Target entry asset was unavailable after restart.");
                SceneManager.LoadScene(NotebookSceneName);
                Schedule(() =>
                {
                    Guard(() =>
                    {
                        NotebookController controller = FindController();
                        Require(controller != null && controller.HasCollected(EntryId),
                            "Notebook UI was not available after restart.");
                        controller.OpenNotebook();
                        Schedule(() =>
                        {
                            controller.ShowSection(entry.Section);
                            Schedule(() =>
                            {
                                controller.JumpToEntry(entry.EntryId);
                                Schedule(() => VerifyChoiceIsLocked(controller, service), 0.35d);
                            }, 0.35d);
                        }, 1.1d);
                    });
                }, 1.25d);
            });
        }

        private static void VerifyChoiceIsLocked(NotebookController controller, PeacelandSaveService service)
        {
            Guard(() =>
            {
                bool hasActiveChoice = UnityEngine.Object.FindObjectsByType<Button>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .Any(button => button.name.StartsWith("Record Choice ", StringComparison.Ordinal)
                                   && button.gameObject.activeInHierarchy);
                bool hasRecordedLabel = UnityEngine.Object.FindObjectsByType<TMP_Text>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .Any(text => text.gameObject.activeInHierarchy
                                 && text.text.StartsWith("Recorded:", StringComparison.Ordinal));
                Require(!hasActiveChoice && hasRecordedLabel,
                    "Restarted notebook did not render the choice as locked/recorded.");

                int beforeDuplicate = service.GetStat(PeacelandStatId.InsightNaivety);
                MethodInfo callback = typeof(NotebookController).GetMethod(
                    "HandleRecordChoiceSelected",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require(callback != null, "Duplicate-choice guard callback was not found.");
                callback.Invoke(controller, new object[] { EntryId, ChoiceIndex });
                service.Load();
                Require(service.GetStat(PeacelandStatId.InsightNaivety) == beforeDuplicate,
                    "A repeated choice or reload applied the stat delta again.");
                AddLog("Idempotency PASS: recorded UI has no choice buttons; duplicate callback + Load kept Insight=1.");

                SceneManager.LoadScene(TransitionSceneName);
                Schedule(() =>
                {
                    Guard(() =>
                    {
                        VerifyLoadedState("Second scene transition");
                        AddLog("Second scene transition PASS: restored state remained stable.");
                        SessionState.SetBool(KeyPrefix + "ClosedLoopPassed", true);
                        SessionState.SetInt(KeyPrefix + "Stage", 3);
                        EditorApplication.ExitPlaymode();
                    });
                }, 1.25d);
            });
        }

        private static void VerifyLoadedState(string checkpoint)
        {
            PeacelandSaveService service = PeacelandSaveService.Instance;
            Require(service.ActiveSlotIndex == TestSlot, checkpoint + " restored the wrong active slot.");
            Require(service.GetStat(PeacelandStatId.InsightNaivety)
                    == SessionState.GetInt(KeyPrefix + "ExpectedStat", 1),
                checkpoint + " restored the wrong Insight value.");
            RequireState(GetState(service));
        }

        private static NotebookCollectTrigger FindTargetTrigger()
        {
            FieldInfo entriesField = typeof(NotebookCollectTrigger).GetField(
                "entries",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return UnityEngine.Object.FindObjectsByType<NotebookCollectTrigger>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault(trigger =>
                    entriesField?.GetValue(trigger) is List<NotebookEntryDefinition> entries
                    && entries.Any(entry => entry != null && entry.EntryId == EntryId));
        }

        private static NotebookController FindController()
        {
            return UnityEngine.Object.FindFirstObjectByType<NotebookController>(FindObjectsInactive.Include);
        }

        private static NotebookEntryStateData GetState(PeacelandSaveService service)
        {
            return FindState(service.GetNotebookData());
        }

        private static NotebookEntryStateData FindState(NotebookSaveData notebook)
        {
            return notebook?.states?.FirstOrDefault(state => state.entryId == EntryId);
        }

        private static void RequireState(NotebookEntryStateData state)
        {
            Require(state != null
                    && state.isCollected
                    && state.isReviewed
                    && state.recordChoiceCompleted
                    && state.selectedRecordChoiceIndex == ChoiceIndex
                    && state.selectedRecordChoiceId == ChoiceId,
                "Notebook state is missing collected/reviewed/practical choice data.");
        }

        private static void Guard(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                SessionState.SetString(KeyPrefix + "Failure", exception.GetBaseException().Message);
                AddLog("FAIL: " + exception.GetBaseException().Message);
                SessionState.SetInt(KeyPrefix + "Stage", 3);
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.ExitPlaymode();
                }
                else
                {
                    FinishRun();
                }
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void Schedule(Action action, double delaySeconds)
        {
            scheduledAction = action;
            scheduledAt = EditorApplication.timeSinceStartup + delaySeconds;
            EditorApplication.update -= RunScheduled;
            EditorApplication.update += RunScheduled;
        }

        private static void RunScheduled()
        {
            if (EditorApplication.timeSinceStartup < scheduledAt)
            {
                return;
            }

            EditorApplication.update -= RunScheduled;
            Action action = scheduledAction;
            scheduledAction = null;
            action?.Invoke();
        }

        private static void FinishRun()
        {
            if (!SessionState.GetBool(KeyPrefix + "Running", false))
            {
                return;
            }

            bool closedLoopPassed = SessionState.GetBool(KeyPrefix + "ClosedLoopPassed", false);
            bool harnessPassed = false;
            int harnessErrors = -1;
            int harnessWarnings = -1;
            string originalScene = SessionState.GetString(KeyPrefix + "OriginalScene", string.Empty);

            try
            {
                if (!string.IsNullOrWhiteSpace(originalScene) && File.Exists(originalScene))
                {
                    EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
                }

                NotebookHarnessReport harness = NotebookHarnessEditor.RunValidation(scanScenes: true);
                harnessPassed = harness.Passed;
                harnessErrors = harness.ErrorCount;
                harnessWarnings = harness.WarningCount;
                MethodInfo export = typeof(NotebookHarnessEditor).GetMethod(
                    "ExportReport",
                    BindingFlags.Static | BindingFlags.NonPublic);
                export?.Invoke(null, new object[] { harness });
            }
            catch (Exception exception)
            {
                AddLog("Harness export FAIL: " + exception.GetBaseException().Message);
            }

            int originalSlot = SessionState.GetInt(KeyPrefix + "OriginalSlot", -1);
            PlayerPrefs.SetInt(PeacelandSaveSlots.ActiveSlotPlayerPrefsKey, originalSlot);
            PlayerPrefs.Save();

            string testPath = PeacelandSaveService.GetSlotPath(TestSlot);
            if (File.Exists(testPath))
            {
                File.Delete(testPath);
            }

            bool passed = closedLoopPassed && harnessPassed
                && string.IsNullOrWhiteSpace(SessionState.GetString(KeyPrefix + "Failure", string.Empty));
            string markdown = BuildReport(passed, harnessErrors, harnessWarnings, originalSlot);
            string reportPath = AbsoluteAssetPath(ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, markdown);

            Debug.Log(passed
                ? "Notebook save/load closed loop PASSED. Report: " + ReportPath
                : "Notebook save/load closed loop FAILED. Report: " + ReportPath);
            ClearSession();
        }

        private static string BuildReport(bool passed, int harnessErrors, int harnessWarnings, int originalSlot)
        {
            string failure = SessionState.GetString(KeyPrefix + "Failure", string.Empty);
            string capturedErrors = SessionState.GetString(KeyPrefix + "CapturedErrors", string.Empty);
            return "# Notebook Save/Load Closed-Loop Report\n\n"
                + "- Result: **" + (passed ? "PASS" : "FAIL") + "**\n"
                + "- Started UTC: " + SessionState.GetString(KeyPrefix + "StartedUtc", "unknown") + "\n"
                + "- Finished UTC: " + DateTime.UtcNow.ToString("o") + "\n"
                + "- Isolated slot: Save 10 (index 9; empty before test, deleted after test)\n"
                + "- Original active slot restored: " + (originalSlot >= 0 ? "Save " + (originalSlot + 1) : "none") + "\n"
                + "- Entry: `" + EntryId + "`\n"
                + "- Choice: `" + ChoiceId + "` (actual UI button index " + ChoiceIndex + ")\n"
                + "- Stat: `InsightNaivety` "
                + SessionState.GetInt(KeyPrefix + "StatBefore", 0) + " -> "
                + SessionState.GetInt(KeyPrefix + "ExpectedStat", -1) + "\n"
                + "- Current content harness: " + (harnessErrors < 0
                    ? "not completed"
                    : (harnessErrors == 0 ? "PASS" : "FAIL")
                      + " (" + harnessErrors + " errors, " + harnessWarnings + " warnings)") + "\n"
                + (string.IsNullOrWhiteSpace(failure) ? string.Empty : "- Failure: " + failure + "\n")
                + "\n## Runtime checkpoints\n\n"
                + SessionState.GetString(KeyPrefix + "Log", "- No checkpoints recorded.\n")
                + "\n## Captured runtime errors\n\n"
                + (string.IsNullOrWhiteSpace(capturedErrors) ? "None.\n" : capturedErrors);
        }

        private static void CaptureLog(string condition, string stackTrace, LogType type)
        {
            if (!SessionState.GetBool(KeyPrefix + "Running", false)
                || (type != LogType.Error && type != LogType.Exception && type != LogType.Assert))
            {
                return;
            }

            string current = SessionState.GetString(KeyPrefix + "CapturedErrors", string.Empty);
            SessionState.SetString(KeyPrefix + "CapturedErrors", current + "- " + condition + "\n");
        }

        private static void AddLog(string message)
        {
            string current = SessionState.GetString(KeyPrefix + "Log", string.Empty);
            SessionState.SetString(KeyPrefix + "Log", current + "- " + message + "\n");
        }

        private static void WriteStandaloneFailure(string message)
        {
            string markdown = "# Notebook Save/Load Closed-Loop Report\n\n"
                + "- Result: **FAIL**\n"
                + "- Finished UTC: " + DateTime.UtcNow.ToString("o") + "\n"
                + "- Failure: " + message + "\n";
            string reportPath = AbsoluteAssetPath(ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, markdown);
            Debug.LogError(message);
        }

        private static string AbsoluteAssetPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        private static string RequestPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", RequestFileName));

        private static void ClearSession()
        {
            SessionState.EraseBool(KeyPrefix + "Running");
            SessionState.EraseBool(KeyPrefix + "ClosedLoopPassed");
            foreach (string suffix in new[] { "Stage", "StartedStage", "OriginalSlot", "StatBefore", "ExpectedStat" })
            {
                SessionState.EraseInt(KeyPrefix + suffix);
            }

            foreach (string suffix in new[]
                     {
                         "StartedUtc", "OriginalScene", "Failure", "CapturedErrors", "Log",
                     })
            {
                SessionState.EraseString(KeyPrefix + suffix);
            }
        }
    }
}
#endif
