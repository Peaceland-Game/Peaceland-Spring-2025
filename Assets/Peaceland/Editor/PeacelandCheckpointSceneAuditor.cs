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
    public static class PeacelandCheckpointSceneAuditor
    {
        private const string PolicyObjectName = "Peaceland Scene Checkpoint Policy";
        private const string SaveLoadScenePath = "Assets/Peaceland/Scenes/SaveLoad.unity";

        [MenuItem("Peaceland/Save/Configure Enabled Scene Checkpoints")]
        public static void ConfigureEnabledScenes()
        {
            EnsureOpenScenesAreSaved();
            EnsureSaveLoadSceneEnabled();
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            int created = 0;

            try
            {
                foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
                {
                    if (!buildScene.enabled || !File.Exists(buildScene.path))
                    {
                        continue;
                    }

                    Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                    PeacelandSceneCheckpointPolicy policy =
                        FindInScene<PeacelandSceneCheckpointPolicy>(scene);
                    if (policy != null)
                    {
                        if (!policy.RecordAsGameplayCheckpoint
                            && string.IsNullOrWhiteSpace(policy.ExclusionReason))
                        {
                            SerializedObject existingPolicy = new SerializedObject(policy);
                            existingPolicy.FindProperty("exclusionReason").stringValue =
                                "Navigation scene; preserve the previous gameplay checkpoint.";
                            existingPolicy.ApplyModifiedPropertiesWithoutUndo();
                            EditorSceneManager.MarkSceneDirty(scene);
                            EditorSceneManager.SaveScene(scene);
                        }

                        continue;
                    }

                    GameObject host = new GameObject(PolicyObjectName);
                    SceneManager.MoveGameObjectToScene(host, scene);
                    policy = host.AddComponent<PeacelandSceneCheckpointPolicy>();

                    bool isNavigationScene =
                        FindInScene<PeacelandSaveLoadSceneController>(scene) != null
                        || FindInScene<PeacelandGameStartController>(scene) != null;

                    SerializedObject serializedPolicy = new SerializedObject(policy);
                    serializedPolicy.FindProperty("recordAsGameplayCheckpoint").boolValue =
                        !isNavigationScene;
                    serializedPolicy.FindProperty("exclusionReason").stringValue =
                        isNavigationScene
                            ? "Navigation scene; preserve the previous gameplay checkpoint."
                            : string.Empty;
                    serializedPolicy.ApplyModifiedPropertiesWithoutUndo();

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    created++;
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }

            Debug.Log("[SaveLoad Matrix] Added explicit checkpoint policy to "
                + created + " enabled scene(s). Existing policies were preserved.");
        }

        [MenuItem("Peaceland/Save/Validate Scene Checkpoint Matrix")]
        public static void ValidateSceneMatrix()
        {
            EnsureOpenScenesAreSaved();
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            List<string> failures = new List<string>();
            int checkpointCount = 0;
            int excludedCount = 0;

            try
            {
                foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
                {
                    if (!buildScene.enabled)
                    {
                        continue;
                    }

                    if (!File.Exists(buildScene.path))
                    {
                        failures.Add(buildScene.path + " is enabled but missing.");
                        continue;
                    }

                    Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                    PeacelandSceneCheckpointPolicy[] policies =
                        FindAllInScene<PeacelandSceneCheckpointPolicy>(scene);

                    if (policies.Length != 1)
                    {
                        failures.Add(buildScene.path
                            + " must contain exactly one checkpoint policy; found "
                            + policies.Length + ".");
                        continue;
                    }

                    PeacelandSceneCheckpointPolicy policy = policies[0];
                    if (policy.RecordAsGameplayCheckpoint)
                    {
                        checkpointCount++;
                    }
                    else
                    {
                        excludedCount++;
                        if (string.IsNullOrWhiteSpace(policy.ExclusionReason))
                        {
                            failures.Add(buildScene.path
                                + " is excluded without an Inspector reason.");
                        }
                    }
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }

            if (failures.Count > 0)
            {
                throw new InvalidOperationException(
                    "Scene checkpoint matrix failed:\n- " + string.Join("\n- ", failures));
            }

            Debug.Log("[SaveLoad Matrix] PASS - " + checkpointCount
                + " resumable scene(s), " + excludedCount
                + " explicit navigation/non-checkpoint scene(s), 0 missing policies.");
        }

        internal static List<string> GetEnabledCheckpointSceneNames()
        {
            EnsureOpenScenesAreSaved();
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            List<string> sceneNames = new List<string>();

            try
            {
                foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
                {
                    if (!buildScene.enabled || !File.Exists(buildScene.path))
                    {
                        continue;
                    }

                    Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                    PeacelandSceneCheckpointPolicy policy =
                        FindInScene<PeacelandSceneCheckpointPolicy>(scene);
                    if (policy != null && policy.RecordAsGameplayCheckpoint)
                    {
                        sceneNames.Add(scene.name);
                    }
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }

            return sceneNames;
        }

        private static void EnsureSaveLoadSceneEnabled()
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path != SaveLoadScenePath)
                {
                    continue;
                }

                if (!scenes[i].enabled)
                {
                    scenes[i] = new EditorBuildSettingsScene(SaveLoadScenePath, true);
                    EditorBuildSettings.scenes = scenes.ToArray();
                }

                return;
            }

            scenes.Add(new EditorBuildSettingsScene(SaveLoadScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureOpenScenesAreSaved()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    throw new InvalidOperationException(
                        "Save or discard changes in '" + scene.name
                        + "' before running the scene matrix.");
                }
            }
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            T[] matches = FindAllInScene<T>(scene);
            return matches.Length > 0 ? matches[0] : null;
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            List<T> matches = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                matches.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return matches.ToArray();
        }
    }
}
#endif
