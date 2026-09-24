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
    [Tooltip("Used when BeginBeat is not given a lead of its own.")]
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

    // A ring alone is a reaction test. Ticks spaced evenly across the lead, ending on the
    // target, give the player a tempo to lock to before the moment arrives.
    [Header("Sound")]
    [Tooltip("Ticks before the target. 0 plays only the target itself.")]
    [SerializeField, Range(0, 4)] private int countIn = 2;
    [Tooltip("Optional. Leave empty for a generated tick.")]
    [SerializeField] private AudioClip countInClip;
    [Tooltip("Optional. Leave empty for a generated tick a fifth higher.")]
    [SerializeField] private AudioClip targetClip;
    [SerializeField, Range(0f, 1f)] private float cueVolume = 0.6f;

    [Header("Events")]
    [SerializeField] private UnityEvent onPerfect;
    [SerializeField] private UnityEvent onGood;
    [SerializeField] private UnityEvent onMiss;

    [Header("Prefab references")]
    [SerializeField] private Image ringImage;
    [SerializeField] private Text label;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private float targetTime;
    private float holdStartTime;
    private Vector2 pointerStart;
    private int tapIndex;
    private RhythmJudgement accumulatedJudgement = RhythmJudgement.Perfect;
    private bool activeBeat;
    private bool holding;
    private float nextCueTime;
    private float cuePeriod;
    private Action<RhythmBeatInteraction, RhythmJudgement, float, string> resolvedCallback;

    private static Sprite solidSprite;
    private static Sprite ringSprite;
    private static AudioClip generatedCountIn;
    private static AudioClip generatedTarget;

    public RhythmBeatKind BeatKind => beatKind;
    public bool IsActive => activeBeat;
    public float PerfectWindow => perfectWindow;
    public float GoodWindow => goodWindow;
    public float TargetTime => targetTime;

    private void Awake()
    {
        // A prefab only has to store the bare root; the ring and the label are built here when they are missing.
        canvasGroup = GetComponent<CanvasGroup>();
        EnsureVisuals();
        EnsureAudio();
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
        PlayDueCues(now, expectedTime);

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

        // Without a press the window closes at goodWindow like every other kind; the hold
        // duration only matters once a hold has started, and that branch returned above.
        if (now > expectedTime + goodWindow)
        {
            Resolve(RhythmJudgement.Miss, now - expectedTime, "NO INPUT");
        }
    }

    public void BeginBeat(float beatTargetTime, float lead, Action<RhythmBeatInteraction, RhythmJudgement, float, string> callback)
    {
        // The sequence controller spawns an instance, then calls BeginBeat with an absolute target time and the callback to report back through.
        // The ring and the count-in both run off the lead the caller chose, so a step with a
        // longer lead does not spend its first half with the ring pinned at full size.
        targetTime = beatTargetTime;
        previewLead = Mathf.Max(0.1f, lead);
        resolvedCallback = callback;
        tapIndex = 0;
        accumulatedJudgement = RhythmJudgement.Perfect;
        holding = false;
        activeBeat = true;
        cuePeriod = countIn > 0 ? previewLead / countIn : 0f;
        nextCueTime = countIn > 0 ? targetTime - previewLead : targetTime;
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
            Resolve(RhythmJudgement.Miss, offset, EarlyOrLate(offset));
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
            return;
        }

        // The next tap is closer than a count-in period, so it gets its own single tick.
        nextCueTime = targetTime + tapIndex * multiTapInterval;
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
                Resolve(RhythmJudgement.Miss, offset, EarlyOrLate(offset));
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
        Resolve(RhythmJudgement.Miss, Time.unscaledTime - targetTime, "LET GO EARLY");
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
        RhythmJudgement judgement = EvaluateOffset(offset);
        Resolve(judgement, offset, judgement == RhythmJudgement.Miss ? EarlyOrLate(offset) : null);
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

    private static string EarlyOrLate(float offset)
    {
        return offset < 0f ? "TOO EARLY" : "TOO LATE";
    }

    private void Resolve(RhythmJudgement judgement, float offset, string detail = null)
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

        Action<RhythmBeatInteraction, RhythmJudgement, float, string> callback = resolvedCallback;
        resolvedCallback = null;
        HideBeat();
        callback?.Invoke(this, judgement, offset, detail);
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

    private void PlayDueCues(float now, float expectedTime)
    {
        // Ticks that fell due since the last frame, the last of them on the target itself.
        while (nextCueTime <= expectedTime + 0.001f && now >= nextCueTime)
        {
            bool onTarget = nextCueTime >= expectedTime - 0.001f;
            AudioClip clip = onTarget ? (targetClip != null ? targetClip : generatedTarget)
                                      : (countInClip != null ? countInClip : generatedCountIn);
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, cueVolume);
            }

            if (onTarget || cuePeriod <= 0f)
            {
                nextCueTime = float.MaxValue;
                break;
            }

            nextCueTime += cuePeriod;
        }
    }

    private void EnsureVisuals()
    {
        // Build the button itself, the ring that shrinks around it, and the text label when
        // they are missing. What the prefab already has is left exactly as the Editor set it;
        // only sprites that cannot be stored in a prefab are filled in.
        Image body = GetComponent<Image>();
        if (body.sprite == null)
        {
            body.sprite = GetSolidSprite();
        }

        body.raycastTarget = true;

        if (ringImage == null)
        {
            Transform ringTransform = transform.Find("TimingRing");
            ringImage = ringTransform != null ? ringTransform.GetComponent<Image>() : CreateRing();
        }

        if (ringImage.sprite == null)
        {
            ringImage.sprite = GetRingSprite();
        }

        ringImage.raycastTarget = false;

        if (label == null)
        {
            Transform labelTransform = transform.Find("Label");
            label = labelTransform != null ? labelTransform.GetComponent<Text>() : CreateLabel();
        }

        if (label.font == null)
        {
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        label.raycastTarget = false;
    }

    private Image CreateRing()
    {
        GameObject ringObject = new GameObject("TimingRing", typeof(RectTransform), typeof(Image));
        ringObject.transform.SetParent(transform, false);
        Image ring = ringObject.GetComponent<Image>();
        ring.color = new Color(0.31f, 0.77f, 1f, 0.75f);
        Stretch(ring.rectTransform);
        ring.rectTransform.SetAsFirstSibling();
        return ring;
    }

    private Text CreateLabel()
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(transform, false);
        Text text = labelObject.GetComponent<Text>();
        text.fontSize = 28;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        Stretch(text.rectTransform);
        text.rectTransform.SetAsLastSibling();
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void EnsureAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        if (generatedCountIn == null)
        {
            generatedCountIn = GenerateTick("RhythmCountInTick", 880f);
            generatedTarget = GenerateTick("RhythmTargetTick", 1320f);
        }
    }

    private static AudioClip GenerateTick(string clipName, float frequency)
    {
        // A short decaying sine, made here so the bank does not have to ship audio files.
        const int sampleRate = 44100;
        const float seconds = 0.06f;
        int sampleCount = Mathf.RoundToInt(sampleRate * seconds);
        float[] samples = new float[sampleCount];
        for (int index = 0; index < sampleCount; index++)
        {
            float t = index / (float)sampleRate;
            float envelope = Mathf.Exp(-t * 60f);
            samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
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
