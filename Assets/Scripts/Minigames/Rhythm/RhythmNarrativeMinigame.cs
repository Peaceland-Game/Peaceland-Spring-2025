using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Yarn.Unity;

[Serializable]
public sealed class RhythmNarrativeStep
{
    // 一个剧情步骤 = 一句叙事文本 + 一个需要完成的 Beat 手势。
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
    // 这个组件负责“剧情文本 -> Beat -> 判定 -> 下一句”的闭环。
    // Miss 不会结束流程，而是重新生成当前 Beat，符合探索/叙事优先的设计。
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

    public int CurrentStepIndex => currentStepIndex;
    public bool IsRunning => running;

    private void Awake()
    {
        // Prefab 可以直接放入场景；UI 和默认步骤会在运行时补齐。
        EnsureUi();
        EnsureDefaultSteps();
        if (startOnAwake)
        {
            StartMinigame();
        }
    }

    public override void StartMinigame()
    {
        // 既可以被 GenericMemManager.NextMinigame 调用，也可以通过 startOnAwake 自动启动。
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
        // 停止协程并销毁当前 Beat 实例，防止切换场景后旧输入继续回调。
        running = false;
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        if (activeBeat != null)
        {
            activeBeat.CancelBeat();
            Destroy(activeBeat.gameObject);
            activeBeat = null;
        }
    }

    public void RestartSequence()
    {
        StartMinigame();
    }

    private IEnumerator RunSequence()
    {
        // 外层循环推进剧情步骤；内层循环负责 Miss 重试当前步骤。
        for (currentStepIndex = 0; currentStepIndex < steps.Length && running; currentStepIndex++)
        {
            RhythmNarrativeStep step = steps[currentStepIndex];
            SetDialogue(step);
            onStepStarted?.Invoke();

            bool resolved = false;
            while (running && !resolved)
            {
                RhythmBeatInteraction template = GetTemplate(step.beatKind);
                if (template == null)
                {
                    Debug.LogError("RhythmNarrativeMinigame is missing a prefab for " + step.beatKind, this);
                    yield break;
                }

                activeBeat = Instantiate(template, beatContainer);
                // 每次重试都创建一个干净的实例，避免残留上一次的 tapIndex/hold 状态。
                activeBeat.transform.localPosition = Vector3.zero;
                activeBeat.transform.localScale = Vector3.one;
                activeBeat.BeginBeat(Time.unscaledTime + Mathf.Max(0.1f, step.leadTime), OnBeatResolved);

                while (running && activeBeat != null && activeBeat.IsActive)
                {
                    yield return null;
                }

                if (!running)
                {
                    yield break;
                }

                if (activeBeat == null)
                {
                    yield break;
                }

                RhythmBeatInteraction completedBeat = activeBeat;
                activeBeat = null;
                Destroy(completedBeat.gameObject);
                if (lastJudgement == RhythmJudgement.Miss)
                {
                    // 失败反馈只表现为“再次要求”，不设置 Game Over 或惩罚传送。
                    onStepMissed?.Invoke();
                    resultLabel.text = "MISS — TRY AGAIN";
                    yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, retryDelay));
                }
                else
                {
                    // Perfect 和 Good 都允许剧情继续，只在反馈文字上区分质量。
                    resolved = true;
                    onStepSucceeded?.Invoke();
                    resultLabel.text = lastJudgement.ToString().ToUpperInvariant();
                    yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, nextStepDelay));
                }
            }
        }

        if (running)
        {
            running = false;
            resultLabel.text = "STORY BEAT COMPLETE";
            onSequenceComplete?.Invoke();
        }
    }

    private RhythmJudgement lastJudgement;

    private void OnBeatResolved(RhythmBeatInteraction beat, RhythmJudgement judgement, float offset)
    {
        // Beat 组件不直接依赖剧情系统，只通过这个回调交付判定结果。
        lastJudgement = judgement;
        resultLabel.text = judgement.ToString().ToUpperInvariant();
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
        // Prefab 未配置步骤时，提供四种手势各一次的可运行示例。
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
        // 这里创建最小剧情 UI；正式项目可以在 Prefab 上替换成自己的对话框和肖像。
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
