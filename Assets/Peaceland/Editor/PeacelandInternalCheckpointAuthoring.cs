#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Peaceland.Editor
{
    public static class PeacelandInternalCheckpointAuthoring
    {
        private const string RjScene =
            "Assets/Scenes/R&JMemory/R&JMemoryScene.unity";
        private const string FlowerScene =
            "Assets/Scenes/FlowerMemoryScene.unity";

        [MenuItem("Peaceland/Save/Author Internal Save Points")]
        public static void Author()
        {
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                AuthorScene(
                    RjScene,
                    "rj-memory",
                    new[]
                    {
                        new SavePointDefinition(
                            "After Letter Puzzle",
                            "DialogueMinigames/AfterPuzzle",
                            "rj/letter-assembled",
                            "Letter Reassembled"),
                        new SavePointDefinition(
                            "Ruzica House",
                            "DialogueMinigames/RuzicaHouse",
                            "rj/ruzica-house",
                            "Ruzica's House"),
                    });

                AuthorScene(
                    FlowerScene,
                    "flower-memory",
                    new[]
                    {
                        new SavePointDefinition(
                            "After First Orders",
                            "DialogueMinigames/TeacherDialogueMinigame Start",
                            "flower/first-orders",
                            "First Orders Complete"),
                        new SavePointDefinition(
                            "Boris Visit",
                            "DialogueMinigames/BorisDialogueMinigame",
                            "flower/boris-visit",
                            "Boris's Visit"),
                        new SavePointDefinition(
                            "Children Visit",
                            "DialogueMinigames/ChildDialogueMinigame Start",
                            "flower/children-visit",
                            "Children's Visit"),
                        new SavePointDefinition(
                            "End Of Shift",
                            "DialogueMinigames/FlowerEnd 9",
                            "flower/end-of-shift",
                            "End of Shift"),
                    });

                AssetDatabase.SaveAssets();
                Debug.Log(
                    "[Save Points] Authored 2 R&J and 4 Flower Shop milestones.");
            }
            finally
            {
                if (previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }
        }

        private static void AuthorScene(
            string scenePath,
            string progressKey,
            IReadOnlyList<SavePointDefinition> definitions)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GenericMemManager manager =
                UnityEngine.Object.FindFirstObjectByType<GenericMemManager>(
                    FindObjectsInactive.Include);
            if (manager == null)
            {
                throw new InvalidOperationException(
                    scenePath + " has no GenericMemManager.");
            }

            PeacelandMinigameProgressBridge bridge =
                manager.GetComponent<PeacelandMinigameProgressBridge>();
            if (bridge == null)
            {
                bridge = manager.gameObject.AddComponent<PeacelandMinigameProgressBridge>();
            }

            SerializedObject bridgeObject = new SerializedObject(bridge);
            bridgeObject.FindProperty("progressKey").stringValue = progressKey;
            bridgeObject.FindProperty("manager").objectReferenceValue = manager;
            bridgeObject.FindProperty("restoreOnStart").boolValue = true;
            bridgeObject.ApplyModifiedPropertiesWithoutUndo();

            Transform root = manager.transform.Find("Peaceland Save Points");
            if (root == null)
            {
                root = new GameObject("Peaceland Save Points").transform;
                root.SetParent(manager.transform, false);
            }

            foreach (SavePointDefinition definition in definitions)
            {
                MinigameBehavior target =
                    FindMinigameByPath(scene, definition.minigamePath);
                if (target == null)
                {
                    throw new InvalidOperationException(
                        scenePath + " is missing " + definition.minigamePath + ".");
                }

                AuthorMilestone(root, bridge, target, definition);
            }

            EnsureScenePolicy(scene, progressKey);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AuthorMilestone(
            Transform root,
            PeacelandMinigameProgressBridge bridge,
            MinigameBehavior target,
            SavePointDefinition definition)
        {
            Transform existing = root.Find("Save Point - " + definition.objectName);
            GameObject savePoint = existing != null
                ? existing.gameObject
                : new GameObject("Save Point - " + definition.objectName);
            savePoint.transform.SetParent(root, false);

            PeacelandCheckpointTrigger trigger =
                savePoint.GetComponent<PeacelandCheckpointTrigger>();
            if (trigger == null)
            {
                trigger = savePoint.AddComponent<PeacelandCheckpointTrigger>();
            }

            SerializedObject triggerObject = new SerializedObject(trigger);
            triggerObject.FindProperty("checkpointKey").stringValue =
                definition.checkpointKey;
            triggerObject.FindProperty("displayName").stringValue =
                definition.displayName;
            triggerObject.FindProperty("replaceMatchingCheckpoint").boolValue = true;
            triggerObject.ApplyModifiedPropertiesWithoutUndo();

            PeacelandMinigameCheckpointMilestone milestone =
                savePoint.GetComponent<PeacelandMinigameCheckpointMilestone>();
            if (milestone == null)
            {
                milestone =
                    savePoint.AddComponent<PeacelandMinigameCheckpointMilestone>();
            }

            SerializedObject milestoneObject = new SerializedObject(milestone);
            milestoneObject.FindProperty("progressBridge").objectReferenceValue = bridge;
            milestoneObject.FindProperty("whenMinigameStarts").objectReferenceValue =
                target;
            milestoneObject.FindProperty("checkpointTrigger").objectReferenceValue =
                trigger;
            milestoneObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static MinigameBehavior FindMinigameByPath(
            Scene scene,
            string expectedPath)
        {
            MinigameBehavior[] minigames =
                Resources.FindObjectsOfTypeAll<MinigameBehavior>();
            foreach (MinigameBehavior minigame in minigames)
            {
                if (minigame.gameObject.scene == scene
                    && GetPath(minigame.transform) == expectedPath)
                {
                    return minigame;
                }
            }

            return null;
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        private static void EnsureScenePolicy(Scene scene, string progressKey)
        {
            PeacelandSceneCheckpointPolicy policy =
                UnityEngine.Object.FindFirstObjectByType<PeacelandSceneCheckpointPolicy>(
                    FindObjectsInactive.Include);
            if (policy == null)
            {
                GameObject policyObject =
                    new GameObject("Peaceland Scene Checkpoint Policy");
                SceneManager.MoveGameObjectToScene(policyObject, scene);
                policy = policyObject.AddComponent<PeacelandSceneCheckpointPolicy>();
            }

            SerializedObject serializedPolicy = new SerializedObject(policy);
            serializedPolicy.FindProperty("recordAsGameplayCheckpoint").boolValue = true;
            serializedPolicy.FindProperty("checkpointKey").stringValue =
                "scene/" + progressKey;
            serializedPolicy.FindProperty("checkpointDisplayName").stringValue =
                scene.name;
            serializedPolicy.FindProperty("replaceMatchingCheckpoint").boolValue = true;
            serializedPolicy.ApplyModifiedPropertiesWithoutUndo();
        }

        private readonly struct SavePointDefinition
        {
            public readonly string objectName;
            public readonly string minigamePath;
            public readonly string checkpointKey;
            public readonly string displayName;

            public SavePointDefinition(
                string objectName,
                string minigamePath,
                string checkpointKey,
                string displayName)
            {
                this.objectName = objectName;
                this.minigamePath = minigamePath;
                this.checkpointKey = checkpointKey;
                this.displayName = displayName;
            }
        }
    }
}
#endif
