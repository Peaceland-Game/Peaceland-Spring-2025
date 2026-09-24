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
        RebakeElementLayers();

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

    /// <summary>
    /// The element prefabs were baked against MazePhysical / MazeVisionOccluder /
    /// MazeEventTrigger / MazePenalty, none of which exist in this project, so their
    /// layer indices landed on whatever happened to occupy those slots. The maze now
    /// adds no layers at all: solid things go on Wall, everything else stays on Default
    /// and does its work through trigger colliders.
    /// </summary>
    [MenuItem("Peaceland/Maze/Rebake Element Layers")]
    public static void RebakeElementLayers()
    {
        int wall = LayerMask.NameToLayer(MazeLayers.WallLayerName);
        if (wall < 0)
        {
            throw new InvalidOperationException(
                "The project has no " + MazeLayers.WallLayerName + " layer; the maze needs it to block anything.");
        }

        // Solid: stops a step, a path and a sight line.
        SetPrefabLayer("PF_Maze_Wall.prefab", wall);
        SetPrefabLayer("PF_Maze_VisionOccluder.prefab", wall);

        // Triggers: must not block movement or vision, so they stay on Default.
        SetPrefabLayer("PF_Maze_EventTrigger.prefab", 0);
        SetPrefabLayer("PF_Maze_PenaltyCell.prefab", 0);

        ConfigureMasks("PF_Maze_Player.prefab");
        ConfigureMasks("PF_Maze_PatrolNpc.prefab");
        AssetDatabase.SaveAssets();
        Debug.Log("Maze element layers rebaked onto " + MazeLayers.WallLayerName + " / Default.");
    }

    private static void SetPrefabLayer(string fileName, int layer)
    {
        string path = PrefabFolder + "/" + fileName;
        GameObject root = PrefabUtility.LoadPrefabContents(path);

        // MazeLayerBinding existed only to resolve those missing layer names at runtime.
        MazeLayerBinding binding = root.GetComponent<MazeLayerBinding>();
        if (binding != null)
        {
            UnityEngine.Object.DestroyImmediate(binding, true);
        }

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
            serializedPlayer.FindProperty("blockingMask").intValue = MazeLayers.Blocking;
            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();
        }

        foreach (MazeVisionCone2D cone in root.GetComponentsInChildren<MazeVisionCone2D>(true))
        {
            SerializedObject serializedCone = new SerializedObject(cone);
            serializedCone.FindProperty("occlusionMask").intValue = MazeLayers.Blocking;
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
