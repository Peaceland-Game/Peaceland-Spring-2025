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
    [Tooltip("The four PF_RhythmBeat_* prefabs from Assets/Prefabs/Rhythm, one per gesture.")]
    [SerializeField] private RhythmBeatInteraction storyTapPrefab;
    [SerializeField] private RhythmBeatInteraction holdPrefab;
    [SerializeField] private RhythmBeatInteraction multiTapPrefab;
    [SerializeField] private RhythmBeatInteraction movePrefab;

    [Header("Sequence")]
    [Tooltip("Lines and gestures in order. Ignored when Start Node is set (Yarn drives the beats then).")]
    [SerializeField] private RhythmNarrativeStep[] steps;
    [Tooltip("Seconds before a missed beat is asked for again.")]
    [SerializeField] private float retryDelay = 0.6f;
    [Tooltip("Seconds between a landed beat and the next line.")]
    [SerializeField] private float nextStepDelay = 0.45f;
    [Tooltip("Start by itself when the scene loads. Leave off when a memory manager starts it.")]
    [SerializeField] private bool startOnAwake;

    [Header("Optional Yarn entry")]
    [Tooltip("Optional. Found in the scene when empty and Start Node is set.")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [Tooltip("Yarn node to play instead of the Steps list. Put <<rhythm_beat Kind>> lines in it. The minigame finishes when the node ends.")]
    [SerializeField] private string startNode;

    [Header("Story hooks")]
    [SerializeField] private UnityEvent onStepStarted;
    [SerializeField] private UnityEvent onStepSucceeded;
    [SerializeField] private UnityEvent onStepMissed;
    [Tooltip("Runs once when every step has landed, or when the Yarn node ends.")]
    [SerializeField] private UnityEvent onSequenceComplete;

    [Header("Prefab UI references")]
    [Tooltip("Optional. Anything left empty is built at runtime.")]
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

        if (!string.IsNullOrWhiteSpace(startNode))
        {
            if (dialogueRunner == null)
            {
                dialogueRunner = FindFirstObjectByType<DialogueRunner>();
            }

            if (dialogueRunner == null)
            {
                Debug.LogWarning(name + ": Start Node is set but the scene has no DialogueRunner; playing the Steps list instead.", this);
            }
            else
            {
                // Yarn owns the beats through <<rhythm_beat>>; running the step sequence too
                // would have the two destroy each other's active beat.
                if (!dialogueRunner.IsDialogueRunning)
                {
                    dialogueRunner.StartDialogue(startNode);
                }

                sequenceCoroutine = StartCoroutine(WaitForYarnNode());
                return;
            }
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

    /// <summary>
    /// Yarn mode: the node plays its own beats through &lt;&lt;rhythm_beat&gt;&gt;, so the minigame is done when the node is.
    /// Polled rather than hooked to onDialogueComplete, which Yarn also raises when something calls Stop().
    /// </summary>
    private IEnumerator WaitForYarnNode()
    {
        // Give the runner a moment to actually start the node.
        float waited = 0f;
        while (running && !dialogueRunner.IsDialogueRunning && waited < 0.5f)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        while (running && dialogueRunner.IsDialogueRunning)
        {
            yield return null;
        }

        sequenceCoroutine = null;
        if (running)
        {
            running = false;
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
        // Anything assigned in the Inspector is kept; only the missing pieces are built.
        if (speakerLabel != null && lineLabel != null && resultLabel != null && beatContainer != null)
        {
            return;
        }

        if (speakerLabel == null)
        {
            Canvas canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (beatContainer == null)
        {
            beatContainer = CreateRect("BeatContainer", transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 260f));
        }

        if (speakerLabel == null)
        {
            speakerLabel = CreateText("Speaker", transform, 30, new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(900f, 50f));
        }

        if (lineLabel == null)
        {
            lineLabel = CreateText("StoryLine", transform, 24, new Vector2(0.5f, 1f), new Vector2(0f, -145f), new Vector2(1100f, 70f));
        }

        if (resultLabel == null)
        {
            resultLabel = CreateText("Result", transform, 30, new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(800f, 60f));
        }
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
