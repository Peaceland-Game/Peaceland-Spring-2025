#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Drives the rhythm playtest scene with simulated pointer events and fails if the
/// judgement, the early/late feedback, the retry loop or the hold timeout regress.
///
/// Unity.exe -batchmode -nographics -projectPath . -executeMethod RhythmPlayModeCheck.Run -logFile run.log
/// (no -quit: this calls Exit itself, 0 on pass and 1 on failure)
/// </summary>
[InitializeOnLoad]
public static class RhythmPlayModeCheck
{
    private const string ScenePath = "Assets/Scenes/Rhythm/RhythmPrefabPlaytest.unity";
    private const string PendingKey = "Rhythm.PlayModeCheck.Pending";
    private const int WarmupFrames = 30;
    private const int TimeoutFrames = 1200;
    // The probe waits on real delays (lead, retry, hold), and batchmode frames are far
    // shorter than a human's, so its budget is in seconds rather than frames.
    private const double ProbeTimeoutSeconds = 90;

    private static int framesInPlayMode;
    private static int framesWaiting;
    private static bool spawned;
    private static double spawnedAt;

    static RhythmPlayModeCheck()
    {
        if (SessionState.GetBool(PendingKey, false))
        {
            EditorApplication.update += Tick;
        }
    }

    [MenuItem("Peaceland/Rhythm/Run Beat Check (Play Mode)")]
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

        if (++framesInPlayMode < WarmupFrames)
        {
            return;
        }

        if (!spawned)
        {
            spawned = true;
            spawnedAt = EditorApplication.timeSinceStartup;
            new GameObject("RhythmBeatProbe").AddComponent<RhythmBeatProbe>();
            return;
        }

        if (RhythmBeatProbe.Report == null)
        {
            if (EditorApplication.timeSinceStartup - spawnedAt > ProbeTimeoutSeconds)
            {
                Finish(2, "the probe never reported");
            }

            return;
        }

        Finish(RhythmBeatProbe.Failed ? 1 : 0, RhythmBeatProbe.Report);
    }

    private static void Finish(int exitCode, string message)
    {
        EditorApplication.update -= Tick;
        SessionState.EraseBool(PendingKey);
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

/// <summary>Editor-only probe, spawned into Play Mode by RhythmPlayModeCheck.</summary>
public sealed class RhythmBeatProbe : MonoBehaviour
{
    public static string Report;
    public static bool Failed;

    private const float Lead = 0.4f;
    private readonly List<string> failures = new List<string>();
    private RhythmNarrativeMinigame rhythm;
    private Text result;
    private RhythmBeatInteraction found;

    private void Start()
    {
        Report = null;
        Failed = false;
        StartCoroutine(RunCheck());
    }

    private IEnumerator RunCheck()
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

        // 4. Letting go early is its own miss.
        yield return WaitForBeat();
        retry = found;
        if (retry == null) { yield break; }

        yield return WaitUntilUnscaled(retry.TargetTime);
        PointerDown(retry);
        yield return null;
        PointerUp(retry);
        yield return null;
        Expect(result.text.Contains("LET GO EARLY"), "releasing a hold early does not say LET GO EARLY: " + result.text);

        rhythm.StopMinigame();
        StopCoroutine(play);

        Report = failures.Count == 0
            ? "Rhythm beat PASS: late is Good and says so, early misses and retries, holds time out at the window and fail on release."
            : "Rhythm beat FAILED:\n- " + string.Join("\n- ", failures);
        Failed = failures.Count > 0;
    }

    private IEnumerator WaitForBeat()
    {
        // A retry only spawns after retryDelay, so this waits on the clock, not on frames.
        found = null;
        float deadline = Time.unscaledTime + 3f;
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

        Fail("no beat appeared");
    }

    private static IEnumerator WaitUntilUnscaled(float time)
    {
        while (Time.unscaledTime < time)
        {
            yield return null;
        }
    }

    private static void Click(RhythmBeatInteraction beat)
    {
        ExecuteEvents.Execute(beat.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }

    private static void PointerDown(RhythmBeatInteraction beat)
    {
        ExecuteEvents.Execute(beat.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
    }

    private static void PointerUp(RhythmBeatInteraction beat)
    {
        ExecuteEvents.Execute(beat.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
    }

    private void Expect(bool condition, string failure)
    {
        if (!condition)
        {
            failures.Add(failure);
        }
    }

    private void Fail(string reason)
    {
        Failed = true;
        Report = "Rhythm beat FAILED: " + reason;
    }
}
#endif
