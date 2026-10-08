#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yarn.Unity;

/// <summary>
/// Drives the rhythm playtest scenes with simulated pointer events and fails if the
/// judgement, the early/late feedback, the retry loop, the hold timeout or the Yarn
/// command regress.
///
/// Unity.exe -batchmode -nographics -projectPath . -executeMethod RhythmPlayModeCheck.Run -logFile run.log
/// Unity.exe -batchmode -nographics -projectPath . -executeMethod RhythmPlayModeCheck.RunYarn -logFile yarn.log
/// (no -quit: this calls Exit itself, 0 on pass and 1 on failure)
/// </summary>
[InitializeOnLoad]
public static class RhythmPlayModeCheck
{
    private const string BeatScene = "Assets/Scenes/Rhythm/RhythmPrefabPlaytest.unity";
    private const string YarnScene = "Assets/Scenes/Rhythm/RhythmYarnPlaytest.unity";
    private const string PendingKey = "Rhythm.PlayModeCheck.Pending";
    private const int WarmupFrames = 30;
    private const int TimeoutFrames = 1200;
    // The probes wait on real delays (lead, retry, hold, a line view advancing), and
    // batchmode frames are far shorter than a human's, so the budget is in seconds.
    private const double ProbeTimeoutSeconds = 120;

    private static int framesInPlayMode;
    private static int framesWaiting;
    private static bool spawned;
    private static double spawnedAt;

    static RhythmPlayModeCheck()
    {
        if (SessionState.GetString(PendingKey, string.Empty).Length > 0)
        {
            EditorApplication.update += Tick;
        }
    }

    [MenuItem("Peaceland/Rhythm/Run Beat Check (Play Mode)")]
    public static void Run()
    {
        Begin(BeatScene, "beat");
    }

    [MenuItem("Peaceland/Rhythm/Run Yarn Command Check (Play Mode)")]
    public static void RunYarn()
    {
        Begin(YarnScene, "yarn");
    }

    private static void Begin(string scenePath, string probe)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        SessionState.SetString(PendingKey, probe);
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

        if (++framesInPlayMode < WarmupFrames)
        {
            return;
        }

        if (!spawned)
        {
            spawned = true;
            spawnedAt = EditorApplication.timeSinceStartup;
            GameObject host = new GameObject("RhythmProbe");
            if (SessionState.GetString(PendingKey, string.Empty) == "yarn")
            {
                host.AddComponent<RhythmYarnProbe>();
            }
            else
            {
                host.AddComponent<RhythmBeatProbe>();
            }

            return;
        }

        if (RhythmProbe.Report == null)
        {
            if (EditorApplication.timeSinceStartup - spawnedAt > ProbeTimeoutSeconds)
            {
                Finish(2, "the probe never reported");
            }

            return;
        }

        Finish(RhythmProbe.Failed ? 1 : 0, RhythmProbe.Report);
    }

    private static void Finish(int exitCode, string message)
    {
        EditorApplication.update -= Tick;
        SessionState.EraseString(PendingKey);
        Debug.Log("[RhythmPlayModeCheck] " + message);
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

/// <summary>What the two probes share: the verdict, simulated input, and waiting on a beat.</summary>
public abstract class RhythmProbe : MonoBehaviour
{
    public static string Report;
    public static bool Failed;

    protected readonly List<string> failures = new List<string>();
    protected RhythmNarrativeMinigame rhythm;
    protected RhythmBeatInteraction found;
    private int frames;
    private float started;

    private void Start()
    {
        Report = null;
        Failed = false;
        // Batchmode runs frames as fast as it can; a player gets about sixteen milliseconds
        // of slop per frame, so the timing is checked at that rate, not at a thousand fps.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        started = Time.unscaledTime;
        StartCoroutine(Drive());
    }

    private void Update()
    {
        frames++;
    }

    private IEnumerator Drive()
    {
        yield return RunCheck();
        if (Report != null)
        {
            yield break;
        }

        float elapsed = Time.unscaledTime - started;
        string rate = elapsed > 0f ? (frames / elapsed).ToString("0") : "?";
        Report = failures.Count == 0
            ? PassMessage() + " (" + rate + " fps)"
            : GetType().Name + " FAILED:\n- " + string.Join("\n- ", failures);
        Failed = failures.Count > 0;
    }

    protected abstract IEnumerator RunCheck();
    protected abstract string PassMessage();

    protected IEnumerator WaitForBeat(float seconds = 3f)
    {
        // A retry only spawns after retryDelay, so this waits on the clock, not on frames.
        found = null;
        float deadline = Time.unscaledTime + seconds;
        while (Time.unscaledTime < deadline)
        {
            RhythmBeatInteraction beat = rhythm.ActiveBeat;
            if (beat != null && beat.IsActive)
            {
                found = beat;
                yield break;
            }

            yield return null;
        }

        Fail("no beat appeared within " + seconds + "s");
    }

    protected static IEnumerator WaitUntilUnscaled(float time)
    {
        while (Time.unscaledTime < time)
        {
            yield return null;
        }
    }

    protected static void Click(RhythmBeatInteraction beat)
    {
        ExecuteEvents.Execute(beat.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }

    protected static void PointerDown(RhythmBeatInteraction beat, Vector2 position = default)
    {
        ExecuteEvents.Execute(beat.gameObject, new PointerEventData(EventSystem.current) { position = position }, ExecuteEvents.pointerDownHandler);
    }

    protected static void PointerUp(RhythmBeatInteraction beat)
    {
        ExecuteEvents.Execute(beat.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
    }

    protected static void Drag(RhythmBeatInteraction beat, Vector2 position)
    {
        ExecuteEvents.Execute(beat.gameObject, new PointerEventData(EventSystem.current) { position = position }, ExecuteEvents.dragHandler);
    }

    protected void Expect(bool condition, string failure)
    {
        if (!condition)
        {
            failures.Add(failure);
        }
    }

    protected void Fail(string reason)
    {
        Failed = true;
        Report = GetType().Name + " FAILED: " + reason;
    }
}

/// <summary>Every gesture, judged and fed back, on the prefab playtest scene.</summary>
public sealed class RhythmBeatProbe : RhythmProbe
{
    private const float Lead = 0.4f;
    private Text result;

    protected override string PassMessage()
    {
        return "Rhythm beat PASS: late is Good and says so, early misses and retries, holds time out at the window and fail on release, multi-tap and move land.";
    }

    protected override IEnumerator RunCheck()
    {
        rhythm = FindFirstObjectByType<RhythmNarrativeMinigame>();
        if (rhythm == null)
        {
            Fail("the playtest scene has no RhythmNarrativeMinigame");
            yield break;
        }

        // The scene starts its own four-step run on Awake; take the wheel.
        rhythm.StopMinigame();
        result = rhythm.transform.Find("Result").GetComponent<Text>();

        // 1. A late click inside the good window: judged Good, told it was late.
        Coroutine play = StartCoroutine(rhythm.PlayBeatUntilSuccess(RhythmBeatKind.StoryTap, Lead));
        yield return WaitForBeat();
        RhythmBeatInteraction beat = found;
        if (beat == null) { yield break; }

        yield return WaitUntilUnscaled(beat.TargetTime + 0.07f);
        Click(beat);
        yield return play;
        Expect(rhythm.LastJudgement == RhythmJudgement.Good, "a click 70ms late is not Good: " + result.text);
        Expect(result.text.Contains("GOOD") && result.text.Contains("+"), "the Good verdict does not show a positive offset: " + result.text);

        // 2. Too early is a miss that says so, and the beat comes back for another go.
        play = StartCoroutine(rhythm.PlayBeatUntilSuccess(RhythmBeatKind.StoryTap, Lead));
        yield return WaitForBeat();
        beat = found;
        if (beat == null) { yield break; }

        Click(beat);
        yield return null;
        Expect(rhythm.LastJudgement == RhythmJudgement.Miss, "an immediate click is not a Miss: " + result.text);
        Expect(result.text.Contains("TOO EARLY"), "the early miss does not say TOO EARLY: " + result.text);

        yield return WaitForBeat();
        RhythmBeatInteraction retry = found;
        if (retry == null) { yield break; }

        Expect(retry != beat, "the miss did not spawn a fresh beat");
        yield return WaitUntilUnscaled(retry.TargetTime);
        Click(retry);
        yield return play;
        Expect(rhythm.LastJudgement == RhythmJudgement.Perfect, "a click on the target is not Perfect: " + result.text);

        // 3. A hold nobody presses times out at the good window, not a hold-length later.
        play = StartCoroutine(rhythm.PlayBeatUntilSuccess(RhythmBeatKind.Hold, Lead));
        yield return WaitForBeat();
        beat = found;
        if (beat == null) { yield break; }

        float target = beat.TargetTime;
        float goodWindow = beat.GoodWindow;
        while (beat != null && beat.IsActive)
        {
            yield return null;
        }

        float resolvedAt = Time.unscaledTime;
        Expect(rhythm.LastJudgement == RhythmJudgement.Miss, "an untouched hold did not miss");
        Expect(result.text.Contains("NO INPUT"), "the untouched hold does not say NO INPUT: " + result.text);
        Expect(resolvedAt < target + goodWindow + 0.25f,
            "the untouched hold waited " + (resolvedAt - target).ToString("0.00") + "s past the target before missing");

        // 4. Letting go early is its own miss; holding it out lands.
        yield return WaitForBeat();
        retry = found;
        if (retry == null) { yield break; }

        yield return WaitUntilUnscaled(retry.TargetTime);
        PointerDown(retry);
        yield return null;
        PointerUp(retry);
        yield return null;
        Expect(result.text.Contains("LET GO EARLY"), "releasing a hold early does not say LET GO EARLY: " + result.text);

        yield return WaitForBeat();
        retry = found;
        if (retry == null) { yield break; }

        yield return WaitUntilUnscaled(retry.TargetTime);
        PointerDown(retry);
        yield return play;
        Expect(rhythm.LastJudgement != RhythmJudgement.Miss, "a hold kept for its full duration did not land: " + result.text);

        // 5. Multi-tap: three clicks on the beat's own interval.
        play = StartCoroutine(rhythm.PlayBeatUntilSuccess(RhythmBeatKind.MultiTap, Lead));
        yield return WaitForBeat();
        beat = found;
        if (beat == null) { yield break; }

        for (int tap = 0; tap < 3; tap++)
        {
            yield return WaitUntilUnscaled(beat.TargetTime + tap * beat.MultiTapInterval);
            Click(beat);
        }

        yield return play;
        Expect(rhythm.LastJudgement != RhythmJudgement.Miss, "three taps on the interval did not land: " + result.text);

        // 6. Move: press, then drag past the required distance on the target.
        play = StartCoroutine(rhythm.PlayBeatUntilSuccess(RhythmBeatKind.Move, Lead));
        yield return WaitForBeat();
        beat = found;
        if (beat == null) { yield break; }

        PointerDown(beat, Vector2.zero);
        yield return WaitUntilUnscaled(beat.TargetTime);
        Drag(beat, new Vector2(400f, 0f));
        yield return play;
        Expect(rhythm.LastJudgement != RhythmJudgement.Miss, "a drag on the target did not land: " + result.text);

        rhythm.StopMinigame();
    }
}

/// <summary>A Yarn script asks for two beats; the dialogue must wait on each and move on after.</summary>
public sealed class RhythmYarnProbe : RhythmProbe
{
    protected override string PassMessage()
    {
        return "Rhythm Yarn PASS: <<rhythm_beat>> blocks the line on a miss, retries, and lets the script continue once the beat lands.";
    }

    protected override IEnumerator RunCheck()
    {
        rhythm = FindFirstObjectByType<RhythmNarrativeMinigame>();
        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();
        if (rhythm == null || runner == null)
        {
            Fail("the Yarn playtest scene needs a RhythmNarrativeMinigame and a DialogueRunner");
            yield break;
        }

        Expect(!runner.IsDialogueRunning, "the dialogue was already running before the probe started it");
        runner.StartDialogue(RhythmPrefabBankBuilder.YarnPlaytestNode);

        // Line one, then the first command: a StoryTap.
        yield return WaitForBeat(10f);
        RhythmBeatInteraction beat = found;
        if (beat == null) { yield break; }

        Expect(beat.BeatKind == RhythmBeatKind.StoryTap, "the first beat the script asked for is " + beat.BeatKind + ", not StoryTap");
        Expect(runner.IsDialogueRunning, "the dialogue did not stay open while the beat was up");

        // A miss must not release the line.
        Click(beat);
        yield return null;
        Expect(rhythm.LastJudgement == RhythmJudgement.Miss, "an immediate click did not miss");
        yield return WaitForBeat();
        RhythmBeatInteraction retry = found;
        if (retry == null) { yield break; }

        Expect(runner.IsDialogueRunning, "a miss let the dialogue move on");
        yield return WaitUntilUnscaled(retry.TargetTime);
        Click(retry);

        // Line two, then the second command: a Hold. Its arrival is the proof the script advanced.
        yield return WaitForBeat(10f);
        beat = found;
        if (beat == null) { yield break; }

        Expect(beat.BeatKind == RhythmBeatKind.Hold, "the second beat the script asked for is " + beat.BeatKind + ", not Hold");
        yield return WaitUntilUnscaled(beat.TargetTime);
        PointerDown(beat);
        while (beat != null && beat.IsActive)
        {
            yield return null;
        }

        Expect(rhythm.LastJudgement != RhythmJudgement.Miss, "the held beat did not land");

        // Line three has no beat after it, so the dialogue should end on its own.
        float deadline = Time.unscaledTime + 10f;
        while (runner.IsDialogueRunning && Time.unscaledTime < deadline)
        {
            yield return null;
        }

        Expect(!runner.IsDialogueRunning, "the dialogue never finished after the last beat landed");
    }
}
#endif
