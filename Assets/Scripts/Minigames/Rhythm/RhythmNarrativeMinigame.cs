using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Yarn.Unity;

[Serializable]
public sealed class RhythmNarrativeStep
{
    // One story step is one line of narration plus one beat gesture to complete.
    public string speaker = "THE ORGANIZER";

    [TextArea(2, 5)]
    public string line = "Keep the rhythm. Keep listening.";

    public RhythmBeatKind beatKind = RhythmBeatKind.StoryTap;
    public float leadTime = 0.85f;
}

/// <summary>
/// Story-facing rhythm sequence. A miss repeats the request instead of ending the game.
/// UnityEvents are exposed for Animator, Timeline, Yarn, portraits, and sound hooks.
/// </summary>
[RequireComponent(typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public sealed class RhythmNarrativeMinigame : MinigameBehavior
{
    // This component runs the loop of story line -> beat -> verdict -> next line.
    // A miss does not end the run, it respawns the current beat, which keeps the emphasis on the story.
    [Header("Beat prefabs")]
    [SerializeField] private RhythmBeatInteraction storyTapPrefab;
    [SerializeField] private RhythmBeatInteraction holdPrefab;
    [SerializeField] private RhythmBeatInteraction multiTapPrefab;
    [SerializeField] private RhythmBeatInteraction movePrefab;

    [Header("Sequence")]
    [SerializeField] private RhythmNarrativeStep[] steps;
    [SerializeField] private float retryDelay = 0.6f;
    [SerializeField] private float nextStepDelay = 0.45f;
    [SerializeField] private bool startOnAwake;

    [Header("Optional Yarn entry")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string startNode;

    [Header("Story hooks")]
    [SerializeField] private UnityEvent onStepStarted;
    [SerializeField] private UnityEvent onStepSucceeded;
    [SerializeField] private UnityEvent onStepMissed;
    [SerializeField] private UnityEvent onSequenceComplete;

    [Header("Prefab UI references")]
    [SerializeField] private Text speakerLabel;
    [SerializeField] private Text lineLabel;
    [SerializeField] private Text resultLabel;
    [SerializeField] private Transform beatContainer;
    private RhythmBeatInteraction activeBeat;
    private Coroutine sequenceCoroutine;
    private int currentStepIndex = -1;
    private bool running;
    private RhythmJudgement lastJudgement;

    public int CurrentStepIndex => currentStepIndex;
    public bool IsRunning => running;
    public RhythmJudgement LastJudgement => lastJudgement;

    /// <summary>The beat the player is being asked for right now, or null between beats.</summary>
    public RhythmBeatInteraction ActiveBeat => activeBeat;

    private void Awake()
    {
        // The prefab can be dropped straight into a scene; the UI and the default steps fill themselves in at runtime.
        EnsureUi();
        EnsureDefaultSteps();
        if (startOnAwake)
        {
            StartMinigame();
        }
    }

    public override void StartMinigame()
    {
        // Can be driven by GenericMemManager.NextMinigame, or start itself through startOnAwake.
        StopMinigame();
        EnsureUi();
        EnsureDefaultSteps();
        running = true;
        currentStepIndex = -1;

        if (dialogueRunner != null && !string.IsNullOrWhiteSpace(startNode) && !dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue(startNode);
        }

        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    public override void StopMinigame()
    {
        // Stop the coroutines and destroy the live beat, so old input cannot call back after a scene change.
        running = false;
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        DiscardActiveBeat();
    }

    public void RestartSequence()
    {
        StartMinigame();
    }

    /// <summary>
    /// One beat, asked for again after every miss until it lands. This is the unit a
    /// Yarn command waits on, so a script can put a beat between any two lines.
    /// Ends early, without success, if StopMinigame discards the beat.
    /// </summary>
    public IEnumerator PlayBeatUntilSuccess(RhythmBeatKind kind, float leadTime)
    {
        EnsureUi();
        DiscardActiveBeat();

        RhythmBeatInteraction template = GetTemplate(kind);
        if (template == null)
        {
            Debug.LogError("RhythmNarrativeMinigame is missing a prefab for " + kind, this);
            yield break;
        }

        while (true)
        {
            activeBeat = Instantiate(template, beatContainer);
            // Each retry gets a clean instance, so no tapIndex or hold state survives from the last attempt.
            activeBeat.transform.localPosition = Vector3.zero;
            activeBeat.transform.localScale = Vector3.one;
            float lead = Mathf.Max(0.1f, leadTime);
            activeBeat.BeginBeat(Time.unscaledTime + lead, lead, OnBeatResolved);

            while (activeBeat != null && activeBeat.IsActive)
            {
                yield return null;
            }

            if (activeBeat == null)
            {
                // Discarded from outside: the caller stopped the minigame, not the player.
                yield break;
            }

            RhythmBeatInteraction completedBeat = activeBeat;
            activeBeat = null;
            Destroy(completedBeat.gameObject);
            if (lastJudgement == RhythmJudgement.Miss)
            {
                // Failure only ever means being asked again - no game over, no punishing teleport.
                onStepMissed?.Invoke();
                yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, retryDelay));
                continue;
            }

            // Perfect and good both let the story continue; only the feedback line tells them apart.
            onStepSucceeded?.Invoke();
            yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, nextStepDelay));
            yield break;
        }
    }

    private IEnumerator RunSequence()
    {
        // The outer loop walks the story steps; PlayBeatUntilSuccess retries each one after a miss.
        for (currentStepIndex = 0; currentStepIndex < steps.Length && running; currentStepIndex++)
        {
            RhythmNarrativeStep step = steps[currentStepIndex];
            SetDialogue(step);
            onStepStarted?.Invoke();
            yield return PlayBeatUntilSuccess(step.beatKind, step.leadTime);

            if (!running || lastJudgement == RhythmJudgement.Miss)
            {
                yield break;
            }
        }

        if (running)
        {
            running = false;
            resultLabel.text = "STORY BEAT COMPLETE";
            onSequenceComplete?.Invoke();
        }
    }

    private void OnBeatResolved(RhythmBeatInteraction beat, RhythmJudgement judgement, float offset, string detail)
    {
        // The beat component knows nothing about the story system and reports its verdict through this callback alone.
        // Early or late, and by how much, is what lets the player correct on the retry.
        lastJudgement = judgement;
        string text = judgement.ToString().ToUpperInvariant();
        if (!string.IsNullOrEmpty(detail))
        {
            text += " / " + detail;
        }

        text += string.Format("  ({0:+0;-0;0}ms)", Mathf.RoundToInt(offset * 1000f));
        if (judgement == RhythmJudgement.Miss)
        {
            text += "  -  TRY AGAIN";
        }

        resultLabel.text = text;
    }

    private void DiscardActiveBeat()
    {
        if (activeBeat == null)
        {
            return;
        }

        activeBeat.CancelBeat();
        Destroy(activeBeat.gameObject);
        activeBeat = null;
    }

    private RhythmBeatInteraction GetTemplate(RhythmBeatKind kind)
    {
        switch (kind)
        {
            case RhythmBeatKind.Hold:
                return holdPrefab;
            case RhythmBeatKind.MultiTap:
                return multiTapPrefab;
            case RhythmBeatKind.Move:
                return movePrefab;
            default:
                return storyTapPrefab;
        }
    }

    private void SetDialogue(RhythmNarrativeStep step)
    {
        speakerLabel.text = step.speaker;
        lineLabel.text = step.line;
        resultLabel.text = "READY";
    }

    private void EnsureDefaultSteps()
    {
        // When a prefab has no steps configured, offer a runnable example with one of each gesture.
        if (steps != null && steps.Length > 0)
        {
            return;
        }

        steps = new[]
        {
            new RhythmNarrativeStep { speaker = "THE ORGANIZER", line = "Listen first. Then answer.", beatKind = RhythmBeatKind.StoryTap },
            new RhythmNarrativeStep { speaker = "THE ORGANIZER", line = "Stay with the pressure.", beatKind = RhythmBeatKind.Hold },
            new RhythmNarrativeStep { speaker = "THE ORGANIZER", line = "Again. Again. Again.", beatKind = RhythmBeatKind.MultiTap },
            new RhythmNarrativeStep { speaker = "THE ORGANIZER", line = "Move when the crowd moves.", beatKind = RhythmBeatKind.Move }
        };
    }

    private void EnsureUi()
    {
        // A minimal story UI. A real scene would swap in its own dialogue box and portraits on the prefab.
        if (speakerLabel != null)
        {
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        beatContainer = CreateRect("BeatContainer", transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 260f));
        speakerLabel = CreateText("Speaker", transform, 30, new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(900f, 50f));
        lineLabel = CreateText("StoryLine", transform, 24, new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(1100f, 70f));
        resultLabel = CreateText("Result", transform, 30, new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(800f, 60f));
    }

    private static RectTransform CreateRect(string objectName, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject objectInstance = new GameObject(objectName, typeof(RectTransform));
        objectInstance.transform.SetParent(parent, false);
        RectTransform rect = objectInstance.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Text CreateText(string objectName, Transform parent, int fontSize, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject objectInstance = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        objectInstance.transform.SetParent(parent, false);
        Text text = objectInstance.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return text;
    }
}
