using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum RhythmBeatKind
{
    // 点击剧情按钮：用于推进剧情或回应台词。
    StoryTap,
    // 按住按钮一段时间，提前松开会 Miss。
    Hold,
    // 在多个预定时间点连续点击。
    MultiTap,
    // 拖动按钮达到指定距离，并且拖动发生在判定窗口内。
    Move
}

public enum RhythmJudgement
{
    // 与目标时间的偏差不超过 Perfect 窗口。
    Perfect,
    // 超过 Perfect 但仍在 Good 窗口内。
    Good,
    // 过早、过晚、松手过早或动作未完成。
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
    // 这个组件是四种 Beat Prefab 的共同输入和判定内核。
    // Prefab 之间主要通过 beatKind 和参数差异来复用同一份代码。
    // 基础节奏参数。所有时间都使用 unscaledTime，避免暂停或慢动作改变判定。
    [Header("Beat")]
    [SerializeField] private RhythmBeatKind beatKind = RhythmBeatKind.StoryTap;
    [SerializeField] private float previewLead = 0.85f;
    [SerializeField] private float perfectWindow = 0.05f;
    [SerializeField] private float goodWindow = 0.15f;

    // Hold 专用参数：按下必须先落在时间窗口内，然后持续 holdDuration 秒。
    [Header("Hold")]
    [SerializeField] private float holdDuration = 0.75f;

    // MultiTap 专用参数：每次点击的目标时间按 multiTapInterval 递进。
    [Header("Multi Tap")]
    [SerializeField] private int requiredTaps = 3;
    [SerializeField] private float multiTapInterval = 0.18f;

    // Move 专用参数：从按下位置移动到指定像素距离后结算。
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
        // Prefab 只需要保存最小的根节点；光圈和文字缺失时由这里补齐。
        canvasGroup = GetComponent<CanvasGroup>();
        EnsureVisuals();
        HideBeat();
    }

    private void Update()
    {
        // 除了更新视觉光圈，Update 还负责自动 Miss，避免玩家不点击时序列卡住。
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
        // Sequence 控制器在生成实例后调用 BeginBeat，传入绝对目标时间和结算回调。
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
        // StoryTap 和 MultiTap 通过点击事件处理；Hold/Move 使用专属指针事件。
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
        // Hold 在按下时锁定起始偏差；Move 在按下时记录拖动起点。
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
        // 统一的时间判定函数，保证四种手势共享同一套 Perfect/Good/Miss 规则。
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
        // Resolve 只执行一次：触发 Inspector UnityEvent、隐藏视觉，并通知剧情控制器。
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
        // 运行时补充按钮本体、同心收缩光圈和文字标签。
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
        // 用运行时纹理生成简单环形 Sprite，避免依赖外部 UI 美术资源。
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
