using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum RhythmBeatKind
{
    // Tap a story button, to move the story on or answer a line.
    StoryTap,
    // Hold the button down; letting go early is a miss.
    Hold,
    // Tap again on each of several scheduled beats.
    MultiTap,
    // Drag the button a set distance, with the drag inside the timing window.
    Move
}

public enum RhythmJudgement
{
    // Within the perfect window of the target time.
    Perfect,
    // Outside perfect, but still inside the good window.
    Good,
    // Too early, too late, released early, or never finished.
    Miss
}

/// <summary>
/// Reusable UI beat interaction used by the rhythm prefab bank.
/// The four modes share timing rules but expose different input gestures.
/// </summary>
[RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
public sealed class RhythmBeatInteraction : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    // The shared input and timing core behind all four beat prefabs.
    // They reuse this one script and differ only by beatKind and the fields below.
    // Shared timing fields. Everything runs on unscaledTime, so a pause or a slow motion effect cannot shift the judgement.
    [Header("Beat")]
    [SerializeField] private RhythmBeatKind beatKind = RhythmBeatKind.StoryTap;
    [SerializeField] private float previewLead = 0.85f;
    [SerializeField] private float perfectWindow = 0.05f;
    [SerializeField] private float goodWindow = 0.15f;

    // Hold only: the press has to land inside the window, then last holdDuration seconds.
    [Header("Hold")]
    [SerializeField] private float holdDuration = 0.75f;

    // MultiTap only: each tap's target time steps on by multiTapInterval.
    [Header("Multi Tap")]
    [SerializeField] private int requiredTaps = 3;
    [SerializeField] private float multiTapInterval = 0.18f;

    // Move only: resolves once the drag covers the given pixel distance from where it started.
    [Header("Move")]
    [SerializeField] private float requiredMoveDistance = 120f;

    [Header("Events")]
    [SerializeField] private UnityEvent onPerfect;
    [SerializeField] private UnityEvent onGood;
    [SerializeField] private UnityEvent onMiss;

    [Header("Prefab references")]
    [SerializeField] private Image ringImage;
    [SerializeField] private Text label;
    private CanvasGroup canvasGroup;
    private float targetTime;
    private float holdStartTime;
    private Vector2 pointerStart;
    private int tapIndex;
    private RhythmJudgement accumulatedJudgement = RhythmJudgement.Perfect;
    private bool activeBeat;
    private bool holding;
    private Action<RhythmBeatInteraction, RhythmJudgement, float> resolvedCallback;

    private static Sprite solidSprite;
    private static Sprite ringSprite;

    public RhythmBeatKind BeatKind => beatKind;
    public bool IsActive => activeBeat;
    public float PerfectWindow => perfectWindow;
    public float GoodWindow => goodWindow;

    private void Awake()
    {
        // A prefab only has to store the bare root; the ring and the label are built here when they are missing.
        canvasGroup = GetComponent<CanvasGroup>();
        EnsureVisuals();
        HideBeat();
    }

    private void Update()
    {
        // Besides driving the ring, Update times out into a miss, so a player who never taps cannot stall the sequence.
        if (!activeBeat)
        {
            return;
        }

        float now = Time.unscaledTime;
        float expectedTime = targetTime + tapIndex * multiTapInterval;
        float remaining = Mathf.Clamp01((expectedTime - now) / Mathf.Max(0.01f, previewLead));
        ringImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.62f, 1.35f, remaining);

        if (beatKind == RhythmBeatKind.Hold && holding)
        {
            if (now >= holdStartTime + holdDuration)
            {
                Resolve(EvaluateOffset(holdStartTime - targetTime), holdStartTime - targetTime);
                return;
            }

            label.text = "HOLD " + Mathf.Max(0f, holdStartTime + holdDuration - now).ToString("0.0");
            return;
        }

        if (beatKind == RhythmBeatKind.MultiTap)
        {
            label.text = string.Format("TAP {0}/{1}", tapIndex + 1, requiredTaps);
        }

        float timeout = beatKind == RhythmBeatKind.Hold
            ? targetTime + goodWindow + holdDuration
            : expectedTime + goodWindow;
        if (now > timeout)
        {
            Resolve(RhythmJudgement.Miss, now - expectedTime);
        }
    }

    public void BeginBeat(float beatTargetTime, Action<RhythmBeatInteraction, RhythmJudgement, float> callback)
    {
        // The sequence controller spawns an instance, then calls BeginBeat with an absolute target time and the callback to report back through.
        targetTime = beatTargetTime;
        resolvedCallback = callback;
        tapIndex = 0;
        accumulatedJudgement = RhythmJudgement.Perfect;
        holding = false;
        activeBeat = true;
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        ringImage.rectTransform.localScale = Vector3.one * 1.35f;
        label.text = GetInitialLabel();
    }

    public void CancelBeat()
    {
        activeBeat = false;
        holding = false;
        resolvedCallback = null;
        HideBeat();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // StoryTap and MultiTap come through the click event; Hold and Move need the pointer events.
        if (!activeBeat || beatKind == RhythmBeatKind.Hold || beatKind == RhythmBeatKind.Move)
        {
            return;
        }

        float expectedTime = targetTime + tapIndex * multiTapInterval;
        float offset = Time.unscaledTime - expectedTime;
        RhythmJudgement judgement = EvaluateOffset(offset);
        if (judgement == RhythmJudgement.Miss)
        {
            Resolve(RhythmJudgement.Miss, offset);
            return;
        }

        if (beatKind == RhythmBeatKind.StoryTap)
        {
            Resolve(judgement, offset);
            return;
        }

        if (judgement == RhythmJudgement.Good)
        {
            accumulatedJudgement = RhythmJudgement.Good;
        }

        tapIndex++;
        if (tapIndex >= Mathf.Max(1, requiredTaps))
        {
            Resolve(accumulatedJudgement, offset);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Hold locks in its starting error on press; Move records where the drag began.
        if (!activeBeat)
        {
            return;
        }

        if (beatKind == RhythmBeatKind.Hold)
        {
            float offset = Time.unscaledTime - targetTime;
            RhythmJudgement judgement = EvaluateOffset(offset);
            if (judgement == RhythmJudgement.Miss)
            {
                Resolve(RhythmJudgement.Miss, offset);
                return;
            }

            holding = true;
            holdStartTime = Time.unscaledTime;
            accumulatedJudgement = judgement;
            label.text = "HOLD";
        }
        else if (beatKind == RhythmBeatKind.Move)
        {
            pointerStart = eventData.position;
            label.text = "MOVE";
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!activeBeat || beatKind != RhythmBeatKind.Hold || !holding)
        {
            return;
        }

        holding = false;
        Resolve(RhythmJudgement.Miss, Time.unscaledTime - targetTime);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!activeBeat || beatKind != RhythmBeatKind.Move)
        {
            return;
        }

        float distance = Vector2.Distance(pointerStart, eventData.position);
        if (distance < requiredMoveDistance)
        {
            return;
        }

        float offset = Time.unscaledTime - targetTime;
        Resolve(EvaluateOffset(offset), offset);
    }

    public RhythmJudgement EvaluateOffset(float offsetSeconds)
    {
        // One timing check, so all four gestures share the same perfect / good / miss rules.
        float absoluteOffset = Mathf.Abs(offsetSeconds);
        if (absoluteOffset <= Mathf.Max(0f, perfectWindow))
        {
            return RhythmJudgement.Perfect;
        }

        if (absoluteOffset <= Mathf.Max(perfectWindow, goodWindow))
        {
            return RhythmJudgement.Good;
        }

        return RhythmJudgement.Miss;
    }

    private void Resolve(RhythmJudgement judgement, float offset)
    {
        // Resolve runs once: fire the Inspector event, hide the visuals, tell the story controller.
        if (!activeBeat)
        {
            return;
        }

        activeBeat = false;
        holding = false;
        switch (judgement)
        {
            case RhythmJudgement.Perfect:
                onPerfect?.Invoke();
                break;
            case RhythmJudgement.Good:
                onGood?.Invoke();
                break;
            default:
                onMiss?.Invoke();
                break;
        }

        Action<RhythmBeatInteraction, RhythmJudgement, float> callback = resolvedCallback;
        resolvedCallback = null;
        HideBeat();
        callback?.Invoke(this, judgement, offset);
    }

    private string GetInitialLabel()
    {
        switch (beatKind)
        {
            case RhythmBeatKind.Hold:
                return "HOLD";
            case RhythmBeatKind.MultiTap:
                return "TAP 1/" + Mathf.Max(1, requiredTaps);
            case RhythmBeatKind.Move:
                return "MOVE";
            default:
                return "CLICK";
        }
    }

    private void EnsureVisuals()
    {
        // Build the button itself, the ring that shrinks around it, and the text label at runtime.
        Image body = GetComponent<Image>();
        body.sprite = GetSolidSprite();
        body.raycastTarget = true;

        Transform ringTransform = transform.Find("TimingRing");
        if (ringTransform == null)
        {
            GameObject ringObject = new GameObject("TimingRing", typeof(RectTransform), typeof(Image));
            ringObject.transform.SetParent(transform, false);
            ringTransform = ringObject.transform;
        }

        ringImage = ringTransform.GetComponent<Image>();
        ringImage.sprite = GetRingSprite();
        ringImage.color = new Color(0.31f, 0.77f, 1f, 0.75f);
        ringImage.raycastTarget = false;
        RectTransform ringRect = ringImage.rectTransform;
        ringRect.anchorMin = Vector2.zero;
        ringRect.anchorMax = Vector2.one;
        ringRect.offsetMin = Vector2.zero;
        ringRect.offsetMax = Vector2.zero;
        ringRect.SetAsFirstSibling();

        Transform labelTransform = transform.Find("Label");
        if (labelTransform == null)
        {
            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(transform, false);
            labelTransform = labelObject.transform;
        }

        label = labelTransform.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.transform.SetAsLastSibling();
    }

    private void HideBeat()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private static Sprite GetSolidSprite()
    {
        if (solidSprite == null)
        {
            solidSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        return solidSprite;
    }

    private static Sprite GetRingSprite()
    {
        // Generate the ring sprite from a runtime texture rather than depend on outside UI art.
        if (ringSprite != null)
        {
            return ringSprite;
        }

        const int textureSize = 128;
        const float outerRadius = 62f;
        const float innerRadius = 54f;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false, true);
        Color32[] pixels = new Color32[textureSize * textureSize];
        float center = (textureSize - 1) * 0.5f;
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                pixels[y * textureSize + x] = distance <= outerRadius && distance >= innerRadius
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(255, 255, 255, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        ringSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        return ringSprite;
    }
}
