#if UNITY_EDITOR
using System.IO;
using Peaceland.Notebook;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Peaceland.Notebook.Editor
{
    public static class NotebookProductionSceneAuthoring
    {
        private const string DataFolder = "Assets/Notebook/Data";
        private const string ProductionPrefabPath =
            "Assets/Notebook/Prefabs/NotebookProductionSceneUI.prefab";
        private const string NewspaperAssetPath =
            DataFolder + "/NotebookEntry_PresentMuseumNewspaper.asset";
        private const string WarRoomScenePath = "Assets/Scenes/WarRoom.unity";
        private const string FlowerMemoryScenePath = "Assets/Scenes/FlowerMemoryScene.unity";
        private const string RandJMemoryScenePath = "Assets/Scenes/R&JMemory/R&JMemoryScene.unity";

        private static readonly string[] PresentScenePaths =
        {
            "Assets/Scenes/R&JMemory/Demo_RJ_Present.unity",
            "Assets/Scenes/Museum Intro.unity",
            "Assets/Scenes/PresentLobby.unity",
            "Assets/Scenes/TownSquare.unity",
            "Assets/Scenes/WarRoom.unity",
            "Assets/Scenes/PDayIntro.unity",
        };

        [MenuItem("Peaceland/Notebook/Production/Install Present Scenes")]
        private static void InstallPresentScenesMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            InstallPresentScenes();
        }

        public static void InstallPresentScenes()
        {
            string originalScenePath = SceneManager.GetActiveScene().path;
            EnsureProductionPrefab();
            NotebookEntryDefinition newspaperEntry = EnsureNewspaperEntry();
            NotebookContentAuthoring.SyncNotebookDatabaseFromAssets();

            int installedCount = 0;
            for (int i = 0; i < PresentScenePaths.Length; i++)
            {
                string scenePath = PresentScenePaths[i];
                if (!File.Exists(scenePath))
                {
                    Debug.LogWarning("Notebook production install skipped missing scene: " + scenePath);
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                InstallNotebookUi();

                if (scenePath.EndsWith("Demo_RJ_Present.unity"))
                {
                    WireNewspaper(scene, newspaperEntry);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                installedCount++;
            }

            AssetDatabase.SaveAssets();
            if (!string.IsNullOrWhiteSpace(originalScenePath) && File.Exists(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }

            Debug.Log("Notebook production install completed for " + installedCount + " Present scene(s).");
        }

        [MenuItem("Peaceland/Notebook/Production/Install Real Gameplay Cases")]
        private static void InstallRealGameplayCasesMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            InstallRealGameplayCases();
        }

        public static void InstallRealGameplayCases()
        {
            EnsureProductionPrefab();
            NotebookEntryDefinition photographs = EnsureEntry(
                "NotebookEntry_WarRoomPhotographs",
                "present.warroom.photographs",
                "War Room Photographs",
                "Photographs displayed in the War Room preserve a curated view of people and events.",
                20);
            NotebookEntryDefinition cleoni = EnsureEntry(
                "NotebookEntry_WarRoomMedalCleoni",
                "present.warroom.medal.cleoni",
                "Cleoni Medal",
                "A medal attributed to Cleoni, displayed among the War Room artifacts.",
                30);
            NotebookEntryDefinition eskani = EnsureEntry(
                "NotebookEntry_WarRoomMedalEskani",
                "present.warroom.medal.eskani",
                "Eskani Medal",
                "A medal attributed to Eskani, displayed among the War Room artifacts.",
                40);
            NotebookEntryDefinition thebrean = EnsureEntry(
                "NotebookEntry_WarRoomMedalThebrean",
                "present.warroom.medal.thebrean",
                "Thebrean Medal",
                "A medal attributed to Thebrean, displayed among the War Room artifacts.",
                50);

            NotebookContentAuthoring.SyncNotebookDatabaseFromAssets();
            InstallWarRoomCases(photographs, cleoni, eskani, thebrean);
            InstallFlowerMinigameCase();
            InstallRandJMinigameCase();
            InstallYarnStatBridges();
            AssetDatabase.SaveAssets();
            Debug.Log("Notebook real gameplay cases installed in all available target scenes.");
        }

        private static void InstallNotebookUi()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProductionPrefabPath);
            if (prefab == null)
            {
                throw new MissingReferenceException(
                    "Notebook production prefab is missing: " + ProductionPrefabPath);
            }

            GameObject keptRoot = null;
            NotebookController[] existingControllers = Object.FindObjectsByType<NotebookController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < existingControllers.Length; i++)
            {
                GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(
                    existingControllers[i].gameObject);
                GameObject candidate = instanceRoot != null
                    ? instanceRoot
                    : existingControllers[i].gameObject;
                bool isProductionPrefab =
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(candidate)
                    == ProductionPrefabPath;
                if (keptRoot == null && isProductionPrefab)
                {
                    keptRoot = candidate;
                    continue;
                }

                Object.DestroyImmediate(candidate);
            }

            Transform[] sceneObjects = Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < sceneObjects.Length; i++)
            {
                Transform candidate = sceneObjects[i];
                if (candidate.parent == null
                    && candidate.gameObject != keptRoot
                    && candidate.name == "NotebookProductionSceneUI")
                {
                    Object.DestroyImmediate(candidate.gameObject);
                }
            }

            GameObject legacyCanvas = GameObject.Find("Notebook Canvas");
            if (legacyCanvas != null && legacyCanvas.transform.root.gameObject != keptRoot)
            {
                Object.DestroyImmediate(legacyCanvas);
            }

            GameObject legacySystem = GameObject.Find("Notebook System");
            if (legacySystem != null && legacySystem.transform.root.gameObject != keptRoot)
            {
                Object.DestroyImmediate(legacySystem);
            }

            if (keptRoot == null)
            {
                PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
            }
        }

        private static void EnsureProductionPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ProductionPrefabPath) == null)
            {
                throw new MissingReferenceException(
                    "Notebook production prefab is the UI source of truth and is missing: "
                    + ProductionPrefabPath);
            }
        }

        private static NotebookEntryDefinition EnsureNewspaperEntry()
        {
            NotebookEntryDefinition entry =
                AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(NewspaperAssetPath);
            if (entry == null)
            {
                entry = ScriptableObject.CreateInstance<NotebookEntryDefinition>();
                AssetDatabase.CreateAsset(entry, NewspaperAssetPath);
            }

            SerializedObject serializedEntry = new SerializedObject(entry);
            serializedEntry.FindProperty("entryId").stringValue = "present.museum.newspaper";
            serializedEntry.FindProperty("section").enumValueIndex = (int)NotebookSection.Present;
            serializedEntry.FindProperty("categoryId").stringValue = "readables";
            serializedEntry.FindProperty("categoryDisplayName").stringValue = "Readables";
            serializedEntry.FindProperty("title").stringValue = "Museum Newspaper";
            serializedEntry.FindProperty("bodyText").stringValue =
                "A newspaper shown before the museum visit. Its framing may shape how the events inside are understood.";
            serializedEntry.FindProperty("requiresRecordChoice").boolValue = false;
            serializedEntry.FindProperty("sortOrder").intValue = 10;
            serializedEntry.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(entry);
            return entry;
        }

        private static NotebookEntryDefinition EnsureEntry(
            string assetName,
            string entryId,
            string title,
            string body,
            int sortOrder)
        {
            string assetPath = DataFolder + "/" + assetName + ".asset";
            NotebookEntryDefinition entry =
                AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(assetPath);
            if (entry == null)
            {
                entry = ScriptableObject.CreateInstance<NotebookEntryDefinition>();
                AssetDatabase.CreateAsset(entry, assetPath);
            }

            SerializedObject serializedEntry = new SerializedObject(entry);
            serializedEntry.FindProperty("entryId").stringValue = entryId;
            serializedEntry.FindProperty("section").enumValueIndex = (int)NotebookSection.Present;
            serializedEntry.FindProperty("categoryId").stringValue = "war-room-artifacts";
            serializedEntry.FindProperty("categoryDisplayName").stringValue = "War Room Artifacts";
            serializedEntry.FindProperty("title").stringValue = title;
            serializedEntry.FindProperty("bodyText").stringValue = body;
            serializedEntry.FindProperty("requiresRecordChoice").boolValue = false;
            serializedEntry.FindProperty("sortOrder").intValue = sortOrder;
            serializedEntry.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(entry);
            return entry;
        }

        private static void InstallWarRoomCases(
            NotebookEntryDefinition photographs,
            NotebookEntryDefinition cleoni,
            NotebookEntryDefinition eskani,
            NotebookEntryDefinition thebrean)
        {
            if (!File.Exists(WarRoomScenePath))
            {
                Debug.LogWarning("War Room Notebook install skipped missing scene: " + WarRoomScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(WarRoomScenePath, OpenSceneMode.Single);
            InstallNotebookUi();

            ViewDescription[] descriptions = Object.FindObjectsByType<ViewDescription>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < descriptions.Length; i++)
            {
                ViewDescription source = descriptions[i];
                NotebookEntryDefinition entry = ResolveWarRoomEntry(
                    source.gameObject.name,
                    photographs,
                    cleoni,
                    eskani,
                    thebrean);
                if (entry == null)
                {
                    continue;
                }

                Transform hook = EnsureIntegrationChild(
                    "War Room Readables",
                    source.gameObject.name + " Notebook Entry");
                NotebookCollectTrigger trigger = GetOrAdd<NotebookCollectTrigger>(hook.gameObject);
                ConfigureTrigger(trigger, entry);

                NotebookCollectOnDescriptionViewed adapter =
                    GetOrAdd<NotebookCollectOnDescriptionViewed>(hook.gameObject);
                SerializedObject serializedAdapter = new SerializedObject(adapter);
                serializedAdapter.FindProperty("source").objectReferenceValue = source;
                serializedAdapter.FindProperty("collectTrigger").objectReferenceValue = trigger;
                serializedAdapter.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(adapter);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static NotebookEntryDefinition ResolveWarRoomEntry(
            string objectName,
            NotebookEntryDefinition photographs,
            NotebookEntryDefinition cleoni,
            NotebookEntryDefinition eskani,
            NotebookEntryDefinition thebrean)
        {
            if (objectName.IndexOf("photograph", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return photographs;
            }

            if (objectName.IndexOf("medal", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                return null;
            }

            if (objectName.IndexOf("Cleoni", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return cleoni;
            }

            if (objectName.IndexOf("Eskani", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return eskani;
            }

            return objectName.IndexOf("Thebrean", System.StringComparison.OrdinalIgnoreCase) >= 0
                ? thebrean
                : null;
        }

        private static void InstallFlowerMinigameCase()
        {
            Scene scene = EditorSceneManager.OpenScene(FlowerMemoryScenePath, OpenSceneMode.Single);
            InstallNotebookUi();

            FlowerArrangeMinigame minigame = Object.FindFirstObjectByType<FlowerArrangeMinigame>(
                FindObjectsInactive.Include);
            NotebookEntryDefinition entry = AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(
                DataFolder + "/NotebookEntry_Memory1FloristFlower.asset");
            if (minigame == null || entry == null)
            {
                Debug.LogWarning("Flower arrangement Notebook hook is missing its minigame or entry.");
            }
            else
            {
                SerializedObject serializedMinigame = new SerializedObject(minigame);
                GameObject arrangeRoot =
                    serializedMinigame.FindProperty("arrangeMinigame").objectReferenceValue as GameObject;
                DragManager dragManager = arrangeRoot != null
                    ? arrangeRoot.GetComponent<DragManager>()
                    : null;

                Transform hook = EnsureIntegrationChild(
                    "Minigame Completions",
                    "Flower Arrangement Complete");
                NotebookCollectTrigger trigger = GetOrAdd<NotebookCollectTrigger>(hook.gameObject);
                ConfigureTrigger(trigger, entry);
                NotebookCollectOnDragCompleted adapter =
                    GetOrAdd<NotebookCollectOnDragCompleted>(hook.gameObject);
                SerializedObject serializedAdapter = new SerializedObject(adapter);
                serializedAdapter.FindProperty("source").objectReferenceValue = dragManager;
                serializedAdapter.FindProperty("collectTrigger").objectReferenceValue = trigger;
                serializedAdapter.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void InstallRandJMinigameCase()
        {
            Scene scene = EditorSceneManager.OpenScene(RandJMemoryScenePath, OpenSceneMode.Single);
            InstallNotebookUi();

            LetterPuzzleMinigame minigame = Object.FindFirstObjectByType<LetterPuzzleMinigame>(
                FindObjectsInactive.Include);
            NotebookEntryDefinition entry = AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>(
                DataFolder + "/NotebookEntry_Memory2RJLetter.asset");
            if (minigame == null || entry == null)
            {
                Debug.LogWarning("R&J letter Notebook hook is missing its minigame or entry.");
            }
            else
            {
                SerializedObject serializedMinigame = new SerializedObject(minigame);
                UnityEngine.UI.Button continueButton =
                    serializedMinigame.FindProperty("continueButton").objectReferenceValue
                        as UnityEngine.UI.Button;
                Transform hook = EnsureIntegrationChild(
                    "Minigame Completions",
                    "Assembled Letter Confirmed");
                NotebookCollectTrigger trigger = GetOrAdd<NotebookCollectTrigger>(hook.gameObject);
                ConfigureTrigger(trigger, entry);
                AddPersistentCollectListener(continueButton, trigger);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void InstallYarnStatBridges()
        {
            string[] gameplayScenes =
            {
                PresentScenePaths[0],
                PresentScenePaths[1],
                PresentScenePaths[2],
                PresentScenePaths[3],
                PresentScenePaths[4],
                PresentScenePaths[5],
                FlowerMemoryScenePath,
                RandJMemoryScenePath,
            };

            for (int i = 0; i < gameplayScenes.Length; i++)
            {
                string scenePath = gameplayScenes[i];
                if (!File.Exists(scenePath))
                {
                    Debug.LogWarning("Yarn stat bridge install skipped missing scene: " + scenePath);
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Yarn.Unity.DialogueRunner[] runners =
                    Object.FindObjectsByType<Yarn.Unity.DialogueRunner>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None);
                bool changed = false;
                for (int r = 0; r < runners.Length; r++)
                {
                    Yarn.Unity.DialogueRunner runner = runners[r];
                    Peaceland.YarnStatCommands commands =
                        runner.GetComponent<Peaceland.YarnStatCommands>();
                    if (commands == null)
                    {
                        commands = runner.gameObject.AddComponent<Peaceland.YarnStatCommands>();
                        changed = true;
                    }

                    SerializedObject serializedCommands = new SerializedObject(commands);
                    serializedCommands.FindProperty("dialogueRunner").objectReferenceValue = runner;
                    serializedCommands.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(commands);
                }

                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        private static Transform EnsureIntegrationChild(string groupName, string childName)
        {
            GameObject root = GameObject.Find("Notebook Integrations");
            if (root == null)
            {
                root = new GameObject("Notebook Integrations");
            }

            Transform group = root.transform.Find(groupName);
            if (group == null)
            {
                group = new GameObject(groupName).transform;
                group.SetParent(root.transform, false);
            }

            Transform child = group.Find(childName);
            if (child == null)
            {
                child = new GameObject(childName).transform;
                child.SetParent(group, false);
            }

            return child;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static void ConfigureTrigger(
            NotebookCollectTrigger trigger,
            NotebookEntryDefinition entry)
        {
            SerializedObject serializedTrigger = new SerializedObject(trigger);
            SerializedProperty entries = serializedTrigger.FindProperty("entries");
            entries.arraySize = 1;
            entries.GetArrayElementAtIndex(0).objectReferenceValue = entry;
            serializedTrigger.FindProperty("disableAfterCollect").boolValue = false;
            serializedTrigger.FindProperty("collectOnPointerClick").boolValue = false;
            serializedTrigger.FindProperty("collectOnMouseDown").boolValue = false;
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(trigger);
        }

        private static void AddPersistentCollectListener(
            UnityEngine.UI.Button button,
            NotebookCollectTrigger trigger)
        {
            if (button == null)
            {
                Debug.LogWarning("Notebook collect listener could not find its Continue button.");
                return;
            }

            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentTarget(i) == trigger
                    && button.onClick.GetPersistentMethodName(i) == nameof(trigger.Collect))
                {
                    return;
                }
            }

            UnityEventTools.AddPersistentListener(button.onClick, trigger.Collect);
            EditorUtility.SetDirty(button);
        }

        private static void WireNewspaper(Scene scene, NotebookEntryDefinition entry)
        {
            Demo_RJMuseumIntro manager = Object.FindFirstObjectByType<Demo_RJMuseumIntro>(
                FindObjectsInactive.Include);
            if (manager == null || manager.continueButton == null)
            {
                Debug.LogWarning(
                    "Notebook newspaper hook could not find Demo_RJMuseumIntro/Continue in " + scene.path + ".");
                return;
            }

            GameObject integrationRoot = GameObject.Find("Notebook Integrations");
            if (integrationRoot == null)
            {
                integrationRoot = new GameObject("Notebook Integrations");
            }

            Transform existing = integrationRoot.transform.Find("Newspaper Read");
            GameObject hookObject = existing != null
                ? existing.gameObject
                : new GameObject("Newspaper Read", typeof(NotebookCollectTrigger));
            hookObject.transform.SetParent(integrationRoot.transform, false);

            NotebookCollectTrigger trigger = hookObject.GetComponent<NotebookCollectTrigger>();
            SerializedObject serializedTrigger = new SerializedObject(trigger);
            SerializedProperty entries = serializedTrigger.FindProperty("entries");
            entries.arraySize = 1;
            entries.GetArrayElementAtIndex(0).objectReferenceValue = entry;
            serializedTrigger.FindProperty("disableAfterCollect").boolValue = false;
            serializedTrigger.FindProperty("collectOnPointerClick").boolValue = false;
            serializedTrigger.FindProperty("collectOnMouseDown").boolValue = false;
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();

            if (manager.newsPaper != null && manager.newsPaper.sprite != null)
            {
                SerializedObject serializedEntry = new SerializedObject(entry);
                serializedEntry.FindProperty("image").objectReferenceValue = manager.newsPaper.sprite;
                serializedEntry.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(entry);
            }

            bool alreadyWired = false;
            for (int i = 0; i < manager.continueButton.onClick.GetPersistentEventCount(); i++)
            {
                if (manager.continueButton.onClick.GetPersistentTarget(i) == trigger
                    && manager.continueButton.onClick.GetPersistentMethodName(i) == nameof(trigger.Collect))
                {
                    alreadyWired = true;
                    break;
                }
            }

            if (!alreadyWired)
            {
                UnityEventTools.AddPersistentListener(manager.continueButton.onClick, trigger.Collect);
            }

            EditorUtility.SetDirty(manager.continueButton);
            EditorUtility.SetDirty(trigger);
        }
    }
}
#endif
