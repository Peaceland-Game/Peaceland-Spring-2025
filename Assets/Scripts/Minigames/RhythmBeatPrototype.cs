using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// A self-contained four-beat click prototype.
/// Beats appear one at a time, with a shrinking timing ring around the click target.
/// Early and late input outside the Good window are both explicit Miss judgements.
/// </summary>
public sealed class RhythmBeatPrototype : MonoBehaviour
{
    // The three verdicts: perfect, good, miss.
    public enum Judgement
    {
        Perfect,
        Good,
        Miss
    }

    // These fields set out the four beat rhythm and when each beat appears.
    [Header("Chart")]
    [SerializeField] private int beatCount = 4;
    [SerializeField] private float beatInterval = 1.2f;
    [SerializeField] private float previewLead = 0.85f;

    // Note the windows are in seconds, while the result UI shows milliseconds.
    [Header("Timing windows in seconds")]
    [SerializeField] private float perfectWindow = 0.05f;
    [SerializeField] private float goodWindow = 0.15f;

    private Canvas canvas;
    private Button beatButton;
    private Image beatRing;
    private Text beatLabel;
    private Text resultLabel;
    private Text progressLabel;
    private Text instructionLabel;
    private Text finishLabel;
    private GameObject beatVisual;

    private float sequenceStartTime;
    private int nextBeatIndex;
    private bool beatVisible;
    private bool sequenceRunning;
    private int perfectCount;
    private int goodCount;
    private int missCount;

    private static Sprite solidSprite;
    private static Sprite circleSprite;
    private static Sprite ringSprite;

    private const float RingStartScale = 1.35f;
    private const float RingEndScale = 0.62f;

    public int BeatCount => beatCount;
    public int ResolvedBeatCount => nextBeatIndex;
    public bool IsRunning => sequenceRunning;
    public float PerfectWindow => perfectWindow;
    public float GoodWindow => goodWindow;

    private void Awake()
    {
        // The prototype scene holds a single root object and builds the UI at runtime, which makes it quick to retune.
        BuildRuntimeUi();
        StartSequence();
    }

    private void Update()
    {
        // Update only shows beats, shrinks the rings, and turns "window passed with no tap" into a miss.
        if (!sequenceRunning)
        {
            return;
        }

        if (nextBeatIndex >= beatCount)
        {
            FinishSequence();
            return;
        }

        float now = Time.unscaledTime;
        float targetTime = GetTargetTime(nextBeatIndex);

        if (!beatVisible && now >= targetTime - previewLead)
        {
            ShowBeat();
        }

        if (!beatVisible)
        {
            return;
        }

        float untilTarget = targetTime - now;
        float remaining = Mathf.Clamp01(untilTarget / Mathf.Max(0.01f, previewLead));
        beatRing.rectTransform.localScale = Vector3.one * Mathf.Lerp(RingEndScale, RingStartScale, remaining);
        beatLabel.text = untilTarget >= 0f ? "CLICK" : "CLICK NOW";

        if (now > targetTime + goodWindow)
        {
            ResolveBeat(Judgement.Miss, now - targetTime, "TOO LATE");
        }
    }

    /// <summary>
    /// Called by the central UI button. The input is judged against the current beat target.
    /// </summary>
    public void OnBeatClicked()
    {
        // The button's onClick lands here; the tap time minus the target time is the error for this beat.
        if (!sequenceRunning || !beatVisible || nextBeatIndex >= beatCount)
        {
            return;
        }

        float offset = Time.unscaledTime - GetTargetTime(nextBeatIndex);
        Judgement judgement = EvaluateOffset(offset);
        string detail = judgement == Judgement.Miss
            ? (offset < 0f ? "TOO EARLY" : "TOO LATE")
            : string.Empty;
        ResolveBeat(judgement, offset, detail);
    }

    public void RestartSequence()
    {
        StartSequence();
    }

    /// <summary>
    /// Pure timing evaluation used by the prototype and by automated verification.
    /// </summary>
    public Judgement EvaluateOffset(float offsetSeconds)
    {
        // A pure function, so this can be unit tested later or driven by another input device.
        float absoluteOffset = Mathf.Abs(offsetSeconds);
        if (absoluteOffset <= Mathf.Max(0f, perfectWindow))
        {
            return Judgement.Perfect;
        }

        if (absoluteOffset <= Mathf.Max(perfectWindow, goodWindow))
        {
            return Judgement.Good;
        }

        return Judgement.Miss;
    }

    public string GetTimingSummary()
    {
        return string.Format(
            "Perfect <= {0:0}ms | Good <= {1:0}ms | outside window = Miss",
            perfectWindow * 1000f,
            goodWindow * 1000f);
    }

    private float GetTargetTime(int beatIndex)
    {
        return sequenceStartTime + previewLead + beatIndex * beatInterval;
    }

    private void StartSequence()
    {
        // Reset the progress and the tally but keep the UI, so Restart does not have to rebuild the canvas.
        beatCount = Mathf.Max(1, beatCount);
        beatInterval = Mathf.Max(0.25f, beatInterval);
        previewLead = Mathf.Max(0.1f, previewLead);
        perfectWindow = Mathf.Max(0f, perfectWindow);
        goodWindow = Mathf.Max(perfectWindow, goodWindow);

        sequenceStartTime = Time.unscaledTime;
        nextBeatIndex = 0;
        perfectCount = 0;
        goodCount = 0;
        missCount = 0;
        beatVisible = false;
        sequenceRunning = true;

        if (beatVisual != null)
        {
            beatVisual.SetActive(false);
        }

        if (finishLabel != null)
        {
            finishLabel.gameObject.SetActive(false);
        }

        if (resultLabel != null)
        {
            resultLabel.text = "READY";
            resultLabel.color = new Color(0.78f, 0.84f, 0.92f, 1f);
        }

        RefreshProgress();
    }

    private void ShowBeat()
    {
        // A beat only appears once it enters the preview window, where the player watches it shrink towards its moment.
        beatVisible = true;
        beatVisual.SetActive(true);
        beatRing.rectTransform.localScale = Vector3.one * RingStartScale;
        beatLabel.text = "CLICK";
    }

    private void ResolveBeat(Judgement judgement, float offsetSeconds, string detail)
    {
        // Resolve the current beat, update the tally, and hand the timeline to the next one.
        if (!beatVisible)
        {
            return;
        }

        switch (judgement)
        {
            case Judgement.Perfect:
                perfectCount++;
                resultLabel.text = "PERFECT";
                resultLabel.color = new Color(0.42f, 1f, 0.72f, 1f);
                break;
            case Judgement.Good:
                goodCount++;
                resultLabel.text = "GOOD";
                resultLabel.color = new Color(1f, 0.86f, 0.35f, 1f);
                break;
            default:
                missCount++;
                resultLabel.text = string.IsNullOrEmpty(detail) ? "MISS" : "MISS / " + detail;
                resultLabel.color = new Color(1f, 0.38f, 0.42f, 1f);
                break;
        }

        resultLabel.text += string.Format("  ({0:+0;-0;0}ms)", Mathf.RoundToInt(offsetSeconds * 1000f));
        beatVisible = false;
        beatVisual.SetActive(false);
        nextBeatIndex++;
        RefreshProgress();
    }

    private void FinishSequence()
    {
        // Once all four have resolved, stop updating and show the summary and the restart button.
        sequenceRunning = false;
        beatVisible = false;
        beatVisual.SetActive(false);
        finishLabel.gameObject.SetActive(true);
        finishLabel.text = string.Format(
            "SEQUENCE COMPLETE\nPERFECT {0}   GOOD {1}   MISS {2}\n\nCLICK TO RESTART",
            perfectCount,
            goodCount,
            missCount);
    }

    private void RefreshProgress()
    {
        progressLabel.text = string.Format(
            "BEAT {0}/{1}     PERFECT {2}   GOOD {3}   MISS {4}",
            Mathf.Min(nextBeatIndex + 1, beatCount),
            beatCount,
            perfectCount,
            goodCount,
            missCount);
    }

    private void BuildRuntimeUi()
    {
        // Everything in the prototype UI is built here. The prefab version is RhythmNarrativeMinigame.
        EnsureCamera();
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("RhythmCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreatePanel(canvas.transform, "Background", new Color(0.035f, 0.055f, 0.09f, 1f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Text title = CreateText(canvas.transform, "Title", "RHYTHM / CROWD RESPONSE", 34, font, new Color(0.86f, 0.91f, 1f, 1f));
        SetAnchors(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(620f, 50f));

        instructionLabel = CreateText(canvas.transform, "Instructions", "CLICK THE BEAT INSIDE THE TIMING WINDOW", 20, font, new Color(0.58f, 0.67f, 0.82f, 1f));
        SetAnchors(instructionLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -124f), new Vector2(920f, 38f));

        progressLabel = CreateText(canvas.transform, "Progress", string.Empty, 18, font, new Color(0.67f, 0.76f, 0.9f, 1f));
        SetAnchors(progressLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -172f), new Vector2(920f, 38f));

        resultLabel = CreateText(canvas.transform, "Result", "READY", 30, font, new Color(0.78f, 0.84f, 0.92f, 1f));
        SetAnchors(resultLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -205f), new Vector2(920f, 60f));

        beatVisual = new GameObject("BeatTarget", typeof(RectTransform));
        beatVisual.transform.SetParent(canvas.transform, false);
        RectTransform beatRect = beatVisual.GetComponent<RectTransform>();
        SetAnchors(beatRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(260f, 260f));

        GameObject ringObject = new GameObject("TimingRing", typeof(RectTransform), typeof(Image));
        ringObject.transform.SetParent(beatVisual.transform, false);
        beatRing = ringObject.GetComponent<Image>();
        beatRing.sprite = GetRingSprite();
        beatRing.color = new Color(0.31f, 0.77f, 1f, 0.62f);
        beatRing.type = Image.Type.Simple;
        beatRing.raycastTarget = false;
        SetAnchors(beatRing.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        GameObject buttonObject = new GameObject("BeatButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        buttonObject.transform.SetParent(beatVisual.transform, false);
        beatButton = buttonObject.GetComponent<Button>();
        Image buttonImage = buttonObject.GetComponent<Image>();
        ConfigureImageSprite(buttonImage, new Color(0.1f, 0.22f, 0.38f, 0.98f));
        Outline outline = buttonObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.82f, 1f, 0.85f);
        outline.effectDistance = new Vector2(3f, 3f);
        SetAnchors(buttonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(164f, 164f));
        beatButton.onClick.AddListener(OnBeatClicked);

        beatLabel = CreateText(buttonObject.transform, "Label", "CLICK", 30, font, Color.white);
        SetAnchors(beatLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        finishLabel = CreateText(canvas.transform, "Finish", string.Empty, 28, font, new Color(0.82f, 0.9f, 1f, 1f));
        SetAnchors(finishLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(920f, 240f));
        finishLabel.gameObject.SetActive(false);
        finishLabel.gameObject.AddComponent<Button>().onClick.AddListener(RestartSequence);

        Text footer = CreateText(canvas.transform, "Footer", "PERFECT ±50ms    GOOD ±150ms    OUTSIDE = MISS", 18, font, new Color(0.46f, 0.56f, 0.72f, 1f));
        SetAnchors(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 72f), new Vector2(920f, 38f));
    }

    private static Text CreateText(Transform parent, string name, string content, int fontSize, Font font, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Image CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);
        Image image = panelObject.GetComponent<Image>();
        image.sprite = GetSolidSprite();
        image.color = color;
        image.type = Image.Type.Simple;
        SetAnchors(image.rectTransform, anchorMin, anchorMax, position, size);
        return image;
    }

    private static void ConfigureImageSprite(Image image, Color color)
    {
        image.sprite = GetCircleSprite();
        image.color = color;
        image.type = Image.Type.Simple;
    }

    private static Sprite GetSolidSprite()
    {
        if (solidSprite == null)
        {
            solidSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            solidSprite.name = "RhythmSolidSprite_Runtime";
        }

        return solidSprite;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
        {
            return circleSprite;
        }

        const int textureSize = 96;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false, true);
        Color32[] pixels = new Color32[textureSize * textureSize];
        float center = (textureSize - 1) * 0.5f;
        float radius = center - 1f;
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                pixels[y * textureSize + x] = dx * dx + dy * dy <= radius * radius
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(255, 255, 255, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        texture.name = "RhythmCircleTexture_Runtime";
        circleSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        circleSprite.name = "RhythmCircleSprite_Runtime";
        return circleSprite;
    }

    private static Sprite GetRingSprite()
    {
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
        texture.name = "RhythmRingTexture_Runtime";
        ringSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        ringSprite.name = "RhythmRingSprite_Runtime";
        return ringSprite;
    }

    private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void EnsureCamera()
    {
        // Only add a camera when running this scene on its own, so a camera the project already has is left alone.
        if (Camera.main != null)
        {
            return;
        }

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void EnsureEventSystem()
    {
        // A uGUI Button needs an EventSystem before it sees mouse or touch events.
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystemObject.transform.position = Vector3.zero;
    }
}
