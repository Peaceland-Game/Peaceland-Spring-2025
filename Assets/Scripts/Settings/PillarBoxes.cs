using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class CameraSpacePillarboxEnforcer : MonoBehaviour
{
    [SerializeField]
    public RectTransform leftBar;
    [SerializeField]
    public RectTransform rightBar;

    private RectTransform canvasRect;
    private const float TargetAspect = 16f / 9f; // 1920x1080 is 16:9

    void Start()
    {
        // Grab the RectTransform of the Canvas this script is attached to
        canvasRect = GetComponent<RectTransform>();

        // Force the side bars to be solid black immediately on launch
        if (leftBar != null && leftBar.GetComponent<Image>() != null)
            leftBar.GetComponent<Image>().color = Color.black;

        if (rightBar != null && rightBar.GetComponent<Image>() != null)
            rightBar.GetComponent<Image>().color = Color.black;

        CalculatePillarboxes();
    }

    void Update()
    {
        // Recalculate sizes every frame in case the window size changes
        CalculatePillarboxes();
    }

    /// <summary>
    /// Automatically adjusts the sizes for the pillar boxes on either side based on how far from a 16:9 aspect ratio the screen is
    /// </summary>
    void CalculatePillarboxes()
    {
        if (leftBar == null || rightBar == null || canvasRect == null) return;

        // Use Canvas scale dimensions to stay accurate to Camera space limits
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        float currentAspect = canvasWidth / canvasHeight;

        // If the viewport aspect ratio is wider than 16:9 (e.g., Ultrawide monitors)
        if (currentAspect > TargetAspect)
        {
            // Determine how wide the 16:9 box should be relative to the canvas height
            float targetWidth = canvasHeight * TargetAspect;

            // Distribute the remaining canvas unit space equally between both bars
            float totalBarSpace = canvasWidth - targetWidth;
            float individualBarWidth = totalBarSpace / 2f;

            // Apply widths to the bars. Left-stretch and Right-stretch handles positioning.
            leftBar.sizeDelta = new Vector2(individualBarWidth, 0);
            rightBar.sizeDelta = new Vector2(individualBarWidth, 0);
        }
        else
        {
            // If the monitor matches 16:9 or is narrower, retract the side bars entirely
            leftBar.sizeDelta = Vector2.zero;
            rightBar.sizeDelta = Vector2.zero;
        }
    }
}
