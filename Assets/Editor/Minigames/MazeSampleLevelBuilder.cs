#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds a small playable maze out of the prefab bank. The bank had no level in it,
/// so nothing in the maze - patrolling, being seen, being sent back, the objective
/// loop - had anywhere to actually run.
/// </summary>
public static class MazeSampleLevelBuilder
{
    private const string PrefabFolder = "Assets/Prefabs/Maze";
    private const string ScenePath = "Assets/Scenes/Maze/MazeSample.unity";

    // Odd dimensions so the border and the pillars line up.
    private const int Width = 15;
    private const int Height = 11;

    private static readonly Vector2Int StartCell = new Vector2Int(1, 1);
    private static readonly Vector2Int ObjectiveACell = new Vector2Int(13, 1);
    private static readonly Vector2Int ObjectiveBCell = new Vector2Int(1, 9);
    private static readonly Vector2Int PenaltyCell = new Vector2Int(7, 5);

    private static readonly Vector2Int[] PatrolCells =
    {
        new Vector2Int(13, 9),
        new Vector2Int(7, 9),
        new Vector2Int(7, 1)
    };

    [MenuItem("Peaceland/Maze/Build Sample Level")]
    public static void BuildSampleLevel()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        // The pause menu that follows the player between scenes selects a button on arrival.
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        BuildWalls();

        Transform startPoint = new GameObject("StartPoint").transform;
        startPoint.position = ToWorld(StartCell);

        GameObject player = Place("PF_Maze_Player", StartCell);
        MazePlayerController2D controller = player.GetComponent<MazePlayerController2D>();
        SetReference(controller, "startPoint", startPoint);

        GameObject objectiveA = Place("PF_Maze_EventTrigger", ObjectiveACell);
        objectiveA.name = "Objective A";
        GameObject objectiveB = Place("PF_Maze_EventTrigger", ObjectiveBCell);
        objectiveB.name = "Objective B";

        GameObject penalty = Place("PF_Maze_PenaltyCell", PenaltyCell);
        SetReference(penalty.GetComponent<MazePenaltyCell2D>(), "startPoint", startPoint);

        GameObject npc = Place("PF_Maze_PatrolNpc", PatrolCells[0]);
        WirePatrol(npc.GetComponent<MazePatrolNpc2D>());

        // There and back again: A, then B, then A once more.
        MazeObjectiveSequence sequence = new GameObject("ObjectiveSequence")
            .AddComponent<MazeObjectiveSequence>();
        WireObjectives(
            sequence,
            objectiveA.GetComponent<MazeEventTrigger2D>(),
            objectiveB.GetComponent<MazeEventTrigger2D>());

        // Medium punishment: caught means the walk is lost, not the story.
        WireCaughtToObjectiveReset(npc.GetComponent<MazePatrolNpc2D>(), sequence);

        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Scenes/Maze");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Maze sample level built at " + ScenePath);
    }

    [MenuItem("Peaceland/Maze/Validate Sample Level")]
    public static void ValidateSampleLevel()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
        {
            throw new InvalidOperationException(
                "No sample level. Run Peaceland/Maze/Build Sample Level first.");
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var failures = new List<string>();
        int wallLayer = LayerMask.NameToLayer(MazeLayers.WallLayerName);

        // Physics queries read the last synced transforms, and nothing has stepped yet.
        Physics2D.SyncTransforms();

        GameObject wallParent = Array.Find(
            scene.GetRootGameObjects(),
            item => item.name == "Walls");

        if (wallParent == null)
        {
            failures.Add("no walls");
        }
        else
        {
            // The container is bookkeeping; only what hangs under it has to be solid.
            foreach (Transform wall in wallParent.transform)
            {
                foreach (Transform part in wall.GetComponentsInChildren<Transform>(true))
                {
                    if (part.gameObject.layer != wallLayer)
                    {
                        failures.Add($"{wall.name}/{part.name} sits on layer {part.gameObject.layer}");
                        break;
                    }
                }
            }
        }

        MazePlayerController2D player = UnityEngine.Object.FindFirstObjectByType<MazePlayerController2D>();
        MazePatrolNpc2D npc = UnityEngine.Object.FindFirstObjectByType<MazePatrolNpc2D>();
        MazeObjectiveSequence sequence = UnityEngine.Object.FindFirstObjectByType<MazeObjectiveSequence>();

        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null) failures.Add("no EventSystem, so the pause menu would throw here");
        if (player == null) failures.Add("no player");
        if (npc == null) failures.Add("no patrol NPC");
        if (sequence == null) failures.Add("no objective sequence");

        if (npc != null && npc.GetComponentInChildren<MazeVisionCone2D>(true) == null)
        {
            failures.Add("the NPC has no vision cone, so it can never see anything");
        }

        if (npc != null && CountPersistentListeners(npc, "onPlayerCaught") == 0)
        {
            failures.Add("onPlayerCaught is not wired, so being caught costs nothing");
        }

        if (sequence != null)
        {
            var objectives = GetPrivate<MazeEventTrigger2D[]>(sequence, "objectives");
            if (objectives == null || objectives.Length == 0)
            {
                failures.Add("the sequence has no objectives");
            }
            else
            {
                for (int index = 0; index < objectives.Length; index++)
                {
                    if (objectives[index] == null)
                    {
                        failures.Add($"objective {index} is empty");
                    }
                }
            }
        }

        // The point of the level: every leg of the errand has to be walkable.
        AssertReachable(failures, StartCell, ObjectiveACell, "start -> A");
        AssertReachable(failures, ObjectiveACell, ObjectiveBCell, "A -> B");
        AssertReachable(failures, ObjectiveBCell, ObjectiveACell, "B -> A");
        AssertReachable(failures, StartCell, PatrolCells[0], "start -> first patrol point");

        if (failures.Count > 0)
        {
            throw new InvalidOperationException(
                "Maze sample level FAILED:\n- " + string.Join("\n- ", failures));
        }

        Debug.Log("Maze sample level PASS: layers, wiring and all three legs are walkable.");
    }

    private static void AssertReachable(List<string> failures, Vector2Int from, Vector2Int to, string label)
    {
        bool reachable = MazeGridPathfinder2D.TryBuildPath(
            ToWorld(from),
            ToWorld(to),
            1f,
            new Vector2(0.55f, 0.55f),
            MazeLayers.Blocking,
            out List<Vector2> path);

        if (!reachable || path.Count == 0)
        {
            failures.Add($"{label} is not walkable");
        }
    }

    private static void BuildWalls()
    {
        GameObject prefab = LoadPrefab("PF_Maze_Wall");
        var parent = new GameObject("Walls").transform;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                bool border = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;

                // A pillar field rather than a hand-drawn maze: every odd row and column
                // stays open, so the level cannot accidentally wall an objective off.
                bool pillar = x % 2 == 0 && y % 2 == 0;
                if (!border && !pillar)
                {
                    continue;
                }

                GameObject wall = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                wall.name = $"Wall {x},{y}";
                wall.transform.position = ToWorld(new Vector2Int(x, y));
            }
        }

        parent.name = "Walls";
    }

    private static void WirePatrol(MazePatrolNpc2D npc)
    {
        var points = new Transform[PatrolCells.Length];
        var parent = new GameObject("PatrolPoints").transform;
        for (int index = 0; index < PatrolCells.Length; index++)
        {
            var point = new GameObject($"Patrol {index}").transform;
            point.SetParent(parent);
            point.position = ToWorld(PatrolCells[index]);
            points[index] = point;
        }

        SerializedObject serialized = new SerializedObject(npc);
        SerializedProperty array = serialized.FindProperty("patrolPoints");
        array.arraySize = points.Length;
        for (int index = 0; index < points.Length; index++)
        {
            array.GetArrayElementAtIndex(index).objectReferenceValue = points[index];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireObjectives(
        MazeObjectiveSequence sequence,
        MazeEventTrigger2D objectiveA,
        MazeEventTrigger2D objectiveB)
    {
        MazeEventTrigger2D[] order = { objectiveA, objectiveB, objectiveA };
        SerializedObject serialized = new SerializedObject(sequence);
        SerializedProperty array = serialized.FindProperty("objectives");
        array.arraySize = order.Length;
        for (int index = 0; index < order.Length; index++)
        {
            array.GetArrayElementAtIndex(index).objectReferenceValue = order[index];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireCaughtToObjectiveReset(MazePatrolNpc2D npc, MazeObjectiveSequence sequence)
    {
        UnityEvent caught = GetPrivate<UnityEvent>(npc, "onPlayerCaught");
        if (caught == null)
        {
            return;
        }

        UnityEventTools.AddVoidPersistentListener(
            caught, new UnityAction(sequence.ResetCurrentObjective));
        EditorUtility.SetDirty(npc);
    }

    private static int CountPersistentListeners(MonoBehaviour owner, string fieldName)
    {
        UnityEvent unityEvent = GetPrivate<UnityEvent>(owner, fieldName);
        return unityEvent == null ? 0 : unityEvent.GetPersistentEventCount();
    }

    private static T GetPrivate<T>(object owner, string fieldName) where T : class
    {
        FieldInfo field = owner.GetType().GetField(
            fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(owner) as T;
    }

    private static void SetReference(MonoBehaviour owner, string fieldName, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(owner);
        serialized.FindProperty(fieldName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject Place(string prefabName, Vector2Int cell)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(LoadPrefab(prefabName));
        instance.transform.position = ToWorld(cell);
        return instance;
    }

    private static GameObject LoadPrefab(string prefabName)
    {
        string path = PrefabFolder + "/" + prefabName + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            throw new InvalidOperationException("Missing " + path);
        }

        return prefab;
    }

    private static void CreateCamera()
    {
        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 7f;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.transform.position = new Vector3(Width * 0.5f, Height * 0.5f, -10f);
    }

    private static Vector3 ToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x, cell.y, 0f);
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
