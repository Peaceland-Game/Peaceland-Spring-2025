#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// The two things the maze was reported broken on - the NPC never seeing anything,
/// and the objectives never taking turns - only show up with the game running, so
/// this drives the sample level in Play Mode and fails loudly if either regresses.
///
/// Unity.exe -batchmode -nographics -projectPath . -executeMethod MazePlayModeCheck.Run -logFile run.log
/// (no -quit: this calls Exit itself, 0 on pass and 1 on failure)
/// </summary>
[InitializeOnLoad]
public static class MazePlayModeCheck
{
    private const string ScenePath = "Assets/Scenes/Maze/MazeSample.unity";
    private const string PendingKey = "Maze.PlayModeCheck.Pending";
    private const int WarmupFrames = 30;
    private const int TimeoutFrames = 1200;

    private static int framesInPlayMode;
    private static int framesWaiting;
    private static bool spawned;

    static MazePlayModeCheck()
    {
        if (SessionState.GetBool(PendingKey, false))
        {
            EditorApplication.update += Tick;
        }
    }

    [MenuItem("Peaceland/Maze/Run Sample Level Check (Play Mode)")]
    public static void Run()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        SessionState.SetBool(PendingKey, true);
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

        // Awake has run by now but Start has not, and the sequence arms its first
        // objective from Start.
        if (++framesInPlayMode < WarmupFrames)
        {
            return;
        }

        if (!spawned)
        {
            spawned = true;
            new GameObject("MazeLoopProbe").AddComponent<MazeLoopProbe>();
            return;
        }

        if (MazeLoopProbe.Report == null)
        {
            if (framesInPlayMode > TimeoutFrames)
            {
                Finish(2, "the probe never reported");
            }

            return;
        }

        Finish(MazeLoopProbe.Failed ? 1 : 0, MazeLoopProbe.Report);
    }

    private static void Finish(int exitCode, string message)
    {
        EditorApplication.update -= Tick;
        SessionState.EraseBool(PendingKey);
        Debug.Log("[MazePlayModeCheck] " + message);
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

/// <summary>Editor-only probe, spawned into Play Mode by MazePlayModeCheck.</summary>
public sealed class MazeLoopProbe : MonoBehaviour
{
    public static string Report;
    public static bool Failed;

    // The sample level is a pillar field: walls sit on even-even cells only, so an
    // odd row is an open corridor and an even one has a pillar every other step.
    private static readonly Vector2 SeerCell = new Vector2(3f, 5f);
    private static readonly Vector2 ClearCell = new Vector2(6f, 5f);
    private static readonly Vector2 BlindCell = new Vector2(3f, 4f);
    private static readonly Vector2 BlockedCell = new Vector2(7f, 4f);
    private static readonly Vector2 ParkingCell = new Vector2(9f, 5f);

    private readonly List<string> failures = new List<string>();

    private void Start()
    {
        Report = null;
        Failed = false;
        StartCoroutine(RunCheck());
    }

    private IEnumerator RunCheck()
    {
        var player = FindFirstObjectByType<MazePlayerController2D>();
        var npc = FindFirstObjectByType<MazePatrolNpc2D>();
        var sequence = FindFirstObjectByType<MazeObjectiveSequence>();
        if (player == null || npc == null || sequence == null)
        {
            Fail("the sample level is missing the player, the NPC or the sequence");
            yield break;
        }

        // Park the route so the eyes can be aimed by hand.
        npc.enabled = false;
        var cone = npc.GetComponentInChildren<MazeVisionCone2D>(true);
        if (cone == null)
        {
            Fail("the NPC has no vision cone");
            yield break;
        }

        if (!Sees(npc, cone, player, SeerCell, ClearCell))
        {
            failures.Add("the NPC cannot see down an open corridor it is facing");
        }

        if (Sees(npc, cone, player, BlindCell, BlockedCell))
        {
            failures.Add("the NPC sees through a wall");
        }

        // The errand list: A, then B, then A again.
        MazeEventTrigger2D first = sequence.CurrentObjective;
        if (sequence.CurrentIndex != 0 || first == null)
        {
            Fail("the sequence did not arm its first objective");
            yield break;
        }

        yield return WalkOnto(player, first);
        if (sequence.CurrentIndex != 1)
        {
            Fail("reaching objective 0 left the sequence on " + sequence.CurrentIndex);
            yield break;
        }

        MazeEventTrigger2D second = sequence.CurrentObjective;
        if (second == first)
        {
            Fail("objective 1 is the same trigger as objective 0, so there is no errand");
            yield break;
        }

        yield return WalkOnto(player, second);
        if (sequence.CurrentIndex != 2 || sequence.CurrentObjective != first)
        {
            Fail("the sequence does not send the player back to the first trigger");
            yield break;
        }

        // Medium punishment: being caught costs the walk, not the objective.
        sequence.ResetCurrentObjective();
        if (sequence.CurrentIndex != 2 || sequence.CurrentObjective != first)
        {
            failures.Add("being caught moved the sequence instead of re-arming the objective");
        }

        yield return WalkOnto(player, first);
        if (!sequence.IsComplete)
        {
            failures.Add("the re-armed objective no longer fires, so a catch is a dead end");
        }

        Report = failures.Count == 0
            ? "Maze sample level PASS: the cone sees and is blocked, and A -> B -> A runs to the end."
            : "Maze sample level FAILED:\n- " + string.Join("\n- ", failures);
        Failed = failures.Count > 0;
    }

    private static bool Sees(
        MazePatrolNpc2D npc,
        MazeVisionCone2D cone,
        MazePlayerController2D player,
        Vector2 npcCell,
        Vector2 targetCell)
    {
        npc.transform.position = npcCell;
        player.transform.position = targetCell;

        // The cone owns its own facing; -90 on Z points its up vector along +X.
        cone.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
        Physics2D.SyncTransforms();
        return cone.CanSee(player.transform.position);
    }

    /// <summary>
    /// Teleports out of the way first so landing on the trigger is always an entry,
    /// then gives the physics step a chance to notice.
    /// </summary>
    private static IEnumerator WalkOnto(MazePlayerController2D player, MazeEventTrigger2D objective)
    {
        player.transform.position = ParkingCell;
        Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate();

        player.transform.position = objective.transform.position;
        Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return null;
        yield return null;
    }

    private void Fail(string reason)
    {
        Failed = true;
        Report = "Maze sample level FAILED: " + reason;
    }
}
#endif
