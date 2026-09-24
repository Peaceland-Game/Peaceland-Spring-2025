#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MazePrefabBankBuilder
{
    private const string ScenePath = "Assets/Scenes/Maze/MazePrototype.unity";
    private const string PrefabFolder = "Assets/Prefabs/Maze";
    private const string RootPrefabPath = PrefabFolder + "/PF_Maze_MinigameRoot.prefab";

    [MenuItem("Peaceland/Maze/Create Prefab Bank From Prototype")]
    public static void CreatePrefabBank()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder(PrefabFolder);
        EnsureLayers();
        NormalizeElementPrefabs();

        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedForBuild = !scene.isLoaded;
        if (openedForBuild)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        try
        {
            GameObject root = scene.GetRootGameObjects()
                .FirstOrDefault(item => item.name == "MazePrototypeRoot");
            if (root == null)
            {
                throw new InvalidOperationException("MazePrototypeRoot was not found in " + ScenePath);
            }

            ValidateSource(root);
            string currentPrefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
            if (currentPrefab == RootPrefabPath)
            {
                PrefabUtility.ApplyPrefabInstance(root, InteractionMode.AutomatedAction);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(
                    root, RootPrefabPath, InteractionMode.AutomatedAction, out bool success);
                if (!success)
                {
                    throw new InvalidOperationException("Unity could not create " + RootPrefabPath);
                }
            }

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Maze prefab bank created. Complete root: " + RootPrefabPath);
        }
        finally
        {
            if (openedForBuild && scene.isLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    [MenuItem("Peaceland/Maze/Validate Prefab Bank")]
    public static void ValidatePrefabBank()
    {
        GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(RootPrefabPath);
        if (root == null)
        {
            throw new InvalidOperationException("Missing " + RootPrefabPath);
        }

        ValidateSource(root);
        string[] required =
        {
            "PF_Maze_Player.prefab",
            "PF_Maze_PatrolNpc.prefab",
            "PF_Maze_EventTrigger.prefab",
            "PF_Maze_PenaltyCell.prefab",
            "PF_Maze_VisionOccluder.prefab",
            "PF_Maze_Wall.prefab"
        };

        foreach (string fileName in required)
        {
            string path = PrefabFolder + "/" + fileName;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                throw new InvalidOperationException("Missing " + path);
            }
        }

        Debug.Log("Maze prefab bank validation PASS: 6 elements + complete minigame root.");
    }

    [MenuItem("Peaceland/Maze/Open Prefab Playtest")]
    public static void OpenPlaytest()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static void ValidateSource(GameObject root)
    {
        Require<MazeGameBootstrap>(root);
        Require<MazePlayerController2D>(root);
        Require<MazePatrolNpc2D>(root);
        Require<MazeEventTrigger2D>(root);
        Require<MazePenaltyCell2D>(root);
    }

    private static void EnsureLayers()
    {
        string[] required =
        {
            "MazePhysical",
            "MazeVisionOccluder",
            "MazeEventTrigger",
            "MazePenalty"
        };
        UnityEngine.Object tagManager =
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
        SerializedObject serialized = new SerializedObject(tagManager);
        SerializedProperty layers = serialized.FindProperty("layers");

        foreach (string layerName in required)
        {
            bool exists = false;
            for (int index = 8; index < layers.arraySize; index++)
            {
                if (layers.GetArrayElementAtIndex(index).stringValue == layerName)
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                continue;
            }

            for (int index = 8; index < layers.arraySize; index++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(index);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = layerName;
                    break;
                }
            }
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void NormalizeElementPrefabs()
    {
        SetPrefabLayer("PF_Maze_Wall.prefab", "MazePhysical");
        SetPrefabLayer("PF_Maze_VisionOccluder.prefab", "MazeVisionOccluder");
        SetPrefabLayer("PF_Maze_EventTrigger.prefab", "MazeEventTrigger");
        SetPrefabLayer("PF_Maze_PenaltyCell.prefab", "MazePenalty");

        ConfigureMasks("PF_Maze_Player.prefab");
        ConfigureMasks("PF_Maze_PatrolNpc.prefab");
    }

    private static void SetPrefabLayer(string fileName, string layerName)
    {
        string path = PrefabFolder + "/" + fileName;
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        int layer = LayerMask.NameToLayer(layerName);
        MazeLayerBinding binding = root.GetComponent<MazeLayerBinding>();
        if (binding == null)
        {
            binding = root.AddComponent<MazeLayerBinding>();
        }
        binding.Configure(layerName);
        foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
        {
            item.gameObject.layer = layer;
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void ConfigureMasks(string fileName)
    {
        string path = PrefabFolder + "/" + fileName;
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        MazePlayerController2D player = root.GetComponentInChildren<MazePlayerController2D>(true);
        if (player != null)
        {
            SerializedObject serializedPlayer = new SerializedObject(player);
            serializedPlayer.FindProperty("blockingMask").intValue =
                LayerMask.GetMask("MazePhysical", "Wall");
            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();
        }

        foreach (MazeVisionCone2D cone in root.GetComponentsInChildren<MazeVisionCone2D>(true))
        {
            SerializedObject serializedCone = new SerializedObject(cone);
            serializedCone.FindProperty("occlusionMask").intValue =
                LayerMask.GetMask("MazeVisionOccluder", "Wall");
            serializedCone.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void Require<T>(GameObject root) where T : Component
    {
        if (root.GetComponentInChildren<T>(true) == null)
        {
            throw new InvalidOperationException(
                root.name + " is missing required component " + typeof(T).Name);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }
}
#endif
