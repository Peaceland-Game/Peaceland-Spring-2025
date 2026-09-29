#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Peaceland.Editor
{
    /// <summary>
    /// Loads every reachable scene in one Play Mode session and fails on any error or
    /// exception logged while a scene starts up. It is the check a teammate does by
    /// hand - open the scene, press Play, watch the console - done for all of them.
    /// Self-contained on purpose, so it can be dropped onto any branch to compare.
    ///
    /// Unity.exe -batchmode -nographics -projectPath . -executeMethod Peaceland.Editor.PeacelandScenePlaySmoke.RunHeadless -logFile smoke.log
    /// (no -quit: it calls Exit itself, 0 on pass and 1 on failure)
    /// </summary>
    [InitializeOnLoad]
    public static class PeacelandScenePlaySmoke
    {
        private const string PendingKey = "Peaceland.ScenePlaySmoke.Pending";
        private const string ScenesKey = "Peaceland.ScenePlaySmoke.Scenes";
        private const int FramesPerScene = 90;
        private const int WarmupFrames = 30;
        private const double TimeoutSeconds = 600;

        // Every scene a player can reach, plus the ones the notebook and save work own.
        private static readonly string[] OwnedFolders =
        {
            "Assets/Notebook/Scenes",
            "Assets/Peaceland/Scenes",
            "Assets/Scenes/Maze",
            "Assets/Scenes/Rhythm",
        };

        // Wired for save points but left out of Build Settings by the team.
        private static readonly string[] Extra =
        {
            "Assets/Scenes/FlowerMemoryScene.unity",
        };

        private static int framesInPlayMode;
        private static bool started;
        private static double startedAt;

        static PeacelandScenePlaySmoke()
        {
            if (SessionState.GetBool(PendingKey, false))
            {
                EditorApplication.update += Tick;
            }
        }

        [MenuItem("Peaceland/Save/Run Scene Play Smoke (Headless)")]
        public static void RunHeadless()
        {
            string[] scenes = CollectScenePaths();
            SessionState.SetString(ScenesKey, string.Join("|", scenes));
            SessionState.SetBool(PendingKey, true);
            EditorSceneManager.OpenScene(scenes[0], OpenSceneMode.Single);
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
        }

        public static string[] CollectScenePaths()
        {
            IEnumerable<string> fromBuild = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path);
            IEnumerable<string> fromFolders = OwnedFolders
                .Where(AssetDatabase.IsValidFolder)
                .SelectMany(folder => AssetDatabase.FindAssets("t:Scene", new[] { folder }))
                .Select(AssetDatabase.GUIDToAssetPath);
            return fromBuild.Concat(fromFolders).Concat(Extra)
                .Where(path => !string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                .Distinct()
                .OrderBy(path => path)
                .ToArray();
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            if (++framesInPlayMode < WarmupFrames)
            {
                return;
            }

            if (!started)
            {
                started = true;
                startedAt = EditorApplication.timeSinceStartup;
                var host = new GameObject("Scene Play Smoke").AddComponent<PeacelandScenePlaySmokeHost>();
                host.Scenes = SessionState.GetString(ScenesKey, string.Empty)
                    .Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                return;
            }

            string report = PeacelandScenePlaySmokeHost.Report;
            if (report == null)
            {
                if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
                {
                    Finish(2, "[Scene Play Smoke] never reported");
                }

                return;
            }

            Finish(report.Contains("FAIL") ? 1 : 0, report);
        }

        private static void Finish(int exitCode, string message)
        {
            EditorApplication.update -= Tick;
            SessionState.EraseBool(PendingKey);
            SessionState.EraseString(ScenesKey);
            Debug.Log(message);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
            else
            {
                EditorApplication.isPlaying = false;
            }
        }

        /// <summary>Runs inside Play Mode. Reports every error it saw, or PASS.</summary>
        public static IEnumerator Run(string[] scenePaths, System.Action<string> finished)
        {
            var errors = new List<string>();
            string current = string.Empty;
            Application.LogCallback capture = (message, stackTrace, type) =>
            {
                if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
                {
                    return;
                }

                // Project-wide and pre-existing: URP's global settings asset references a missing type.
                if (message.Contains("UniversalRenderPipelineGlobalSettings"))
                {
                    return;
                }

                string site = stackTrace.Split('\n')
                    .Select(line => line.Trim())
                    .FirstOrDefault(line => line.Contains("Assets"));
                errors.Add(current + ": " + type + ": " + message.Split('\n')[0]
                    + (string.IsNullOrEmpty(site) ? string.Empty : "  @ " + site));
            };

            Application.logMessageReceived += capture;
            try
            {
                foreach (string path in scenePaths)
                {
                    current = System.IO.Path.GetFileNameWithoutExtension(path);
                    AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                        path, new LoadSceneParameters(LoadSceneMode.Single));
                    while (load != null && !load.isDone)
                    {
                        yield return null;
                    }

                    for (int frame = 0; frame < FramesPerScene; frame++)
                    {
                        yield return null;
                    }
                }
            }
            finally
            {
                Application.logMessageReceived -= capture;
            }

            string[] distinct = errors.Distinct().ToArray();
            finished(distinct.Length == 0
                ? "[Scene Play Smoke] PASS | " + scenePaths.Length + " scenes started without an error."
                : "[Scene Play Smoke] FAIL | " + distinct.Length + " distinct error(s) (" + errors.Count
                  + " total) across " + scenePaths.Length + " scenes:\n- " + string.Join("\n- ", distinct));
        }
    }

    /// <summary>Host for the smoke coroutine; spawned once Play Mode is up.</summary>
    public sealed class PeacelandScenePlaySmokeHost : MonoBehaviour
    {
        public static string Report;
        public string[] Scenes;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            Report = null;
            StartCoroutine(PeacelandScenePlaySmoke.Run(Scenes, report => Report = report));
        }
    }
}
#endif
