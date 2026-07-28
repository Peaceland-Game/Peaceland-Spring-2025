using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Peaceland
{
    /// <summary>
    /// One row/button in the save slot panel. Wire references in the scene or via Author menu.
    /// </summary>
    public sealed class PeacelandSaveSlotEntryView : MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private TMP_Text locationText;
        [SerializeField] private TMP_Text dayText;
        [SerializeField] private Image slotBackground;

        private static int draggedSlotIndex = -1;
        private PeacelandSaveSlotPanel ownerPanel;
        private CanvasGroup canvasGroup;
        private bool hasData;

        private static readonly Color EmptyColor = new Color32(217, 217, 217, 255);
        private static readonly Color TownColor = new Color32(137, 213, 226, 255);
        private static readonly Color FloristColor = new Color32(93, 165, 96, 255);
        private static readonly Color WarRoomColor = new Color32(226, 194, 137, 255);
        private static readonly Color RandJColor = new Color32(233, 182, 52, 255);
        private static readonly Color ChildColor = new Color32(255, 124, 124, 255);

        public int SlotIndex => slotIndex;

        public void Configure(int index)
        {
            slotIndex = index;
            if (titleText != null)
            {
                titleText.text = "Save " + (index + 1);
            }
        }

        public void Bind(PeacelandSaveSlotPanel panel)
        {
            ownerPanel = panel;
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => panel.OnSlotSelected(slotIndex));
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() => panel.OnSlotDeleteRequested(slotIndex));
            }
        }

        public void ApplySummary(PeacelandSaveSlotSummary summary)
        {
            hasData = summary.hasData;
            if (titleText != null)
            {
                titleText.text = "Save " + (summary.slotIndex + 1);
            }

            if (!summary.hasData)
            {
                if (locationText != null)
                {
                    locationText.text = "No data";
                }

                if (dayText != null)
                {
                    dayText.text = string.Empty;
                }

                if (slotBackground != null)
                {
                    slotBackground.color = EmptyColor;
                }

                if (statusText != null)
                {
                    statusText.text = locationText == null ? "Empty slot - start a new game" : string.Empty;
                }

                if (detailText != null)
                {
                    detailText.text = "Select to create a new save";
                }

                if (deleteButton != null)
                {
                    deleteButton.gameObject.SetActive(false);
                }

                return;
            }

            if (locationText != null)
            {
                locationText.text = summary.GetDisplayLocationName();
            }

            if (dayText != null)
            {
                dayText.text = "Day " + Mathf.Max(1, summary.currentDay);
            }

            if (slotBackground != null)
            {
                slotBackground.color = ResolveLocationColor(summary.GetDisplayLocationName(), summary.slotIndex);
            }

            if (statusText != null)
            {
                string saved = summary.GetDisplaySavedTimeLocal();
                statusText.text = string.IsNullOrWhiteSpace(saved)
                    ? "Continue"
                    : "Last saved - " + saved;
            }

            if (detailText != null)
            {
                string scene = string.IsNullOrWhiteSpace(summary.lastSceneName)
                    ? "(no scene recorded)"
                    : summary.lastSceneName;
                detailText.text = "Scene: " + scene
                    + "  |  Kindness: " + summary.kindnessCruelty
                    + "  |  Notebook: " + summary.collectedNotebookEntryCount;
            }

            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(true);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!hasData)
            {
                eventData.pointerDrag = null;
                return;
            }

            draggedSlotIndex = slotIndex;
            EnsureCanvasGroup();
            canvasGroup.alpha = 0.65f;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            EnsureCanvasGroup();
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            draggedSlotIndex = -1;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (draggedSlotIndex < 0 || draggedSlotIndex == slotIndex || ownerPanel == null)
            {
                return;
            }

            ownerPanel.OnSlotMoveRequested(draggedSlotIndex, slotIndex);
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private static Color ResolveLocationColor(string locationName, int slotIndex)
        {
            string location = locationName ?? string.Empty;
            if (location.IndexOf("Town", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return TownColor;
            }

            if (location.IndexOf("Florist", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return FloristColor;
            }

            if (location.IndexOf("War", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WarRoomColor;
            }

            if (location.IndexOf("R&J", System.StringComparison.OrdinalIgnoreCase) >= 0
                || location.IndexOf("Romeo", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return RandJColor;
            }

            if (location.IndexOf("Child", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ChildColor;
            }

            Color[] palette = { TownColor, FloristColor, WarRoomColor, RandJColor, ChildColor };
            return palette[Mathf.Abs(slotIndex) % palette.Length];
        }
    }
}
