using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Peaceland.Editor
{
    /// <summary>
    /// Runs the Play Mode save validations from the command line, so they do not need
    /// someone clicking a menu item. Entering Play Mode reloads the domain, so the
    /// request is parked in SessionState and picked up again on the other side.
    ///
    /// Unity.exe -batchmode -projectPath . -executeMethod Peaceland.Editor.PeacelandPlayModeRunner.RunRjSmoke -logFile run.log
    /// (no -quit: the runner calls Exit itself, 0 on pass and 1 on failure)
    /// </summary>
    [InitializeOnLoad]
    public static class PeacelandPlayModeRunner
    {
        private const string PendingKey = "Peaceland.PlayModeRunner.Pending";
        private const int WarmupFrames = 30;
        private const int TimeoutFrames = 1200;

        private static int framesInPlayMode;
        private static int framesWaiting;

        static PeacelandPlayModeRunner()
        {
            if (SessionState.GetString(PendingKey, string.Empty).Length > 0)
            {
                EditorApplication.update += Tick;
            }
        }

        [MenuItem("Peaceland/Save/Run Expanded Closed Loop (Headless)")]
        public static void RunExpandedClosedLoop()
        {
            Begin("Assets/Peaceland/Scenes/SaveLoad.unity", "expanded");
        }

        [MenuItem("Peaceland/Save/Run R&J Internal Save Point Smoke (Headless)")]
        public static void RunRjSmoke()
        {
            Begin("Assets/Scenes/R&JMemory/R&JMemoryScene.unity", "rj");
        }

        [MenuItem("Peaceland/Save/Run Flower Internal Save Point Smoke (Headless)")]
        public static void RunFlowerSmoke()
        {
            Begin("Assets/Scenes/FlowerMemoryScene.unity", "flower");
        }

        private static void Begin(string scenePath, string target)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            SessionState.SetString(PendingKey, target);
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying)
            {
                if (++framesWaiting > TimeoutFrames)
                {
                    Finish(2, "never entered Play Mode");
                }

                return;
            }

            // Awake has run by now but Start has not, and the bridges report IsReady from Start.
            if (++framesInPlayMode < WarmupFrames)
            {
                return;
            }

            string target = SessionState.GetString(PendingKey, string.Empty);
            EditorApplication.update -= Tick;
            try
            {
                switch (target)
                {
                    case "expanded":
                        PeacelandExpandedSaveLoadValidator.Run();
                        break;
                    case "rj":
                        PeacelandExpandedSaveLoadValidator.RunRjInternalSavePointSmoke();
                        break;
                    case "flower":
                        PeacelandExpandedSaveLoadValidator.RunFlowerInternalSavePointSmoke();
                        break;
                    default:
                        Finish(2, "unknown target '" + target + "'");
                        return;
                }
            }
            catch (Exception exception)
            {
                Finish(1, target + " FAILED: " + exception);
                return;
            }

            Finish(0, target + " PASSED");
        }

        private static void Finish(int exitCode, string message)
        {
            EditorApplication.update -= Tick;
            SessionState.EraseString(PendingKey);
            Debug.Log("[PlayModeRunner] " + message);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
            else
            {
                // An editor a person has open should be left open; stop the run instead.
                EditorApplication.isPlaying = false;
            }
        }
    }
}
