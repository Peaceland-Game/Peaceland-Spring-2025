using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Peaceland
{
    /// <summary>
    /// Standalone save-slot scene. Selecting a slot prepares isolated save and notebook data;
    /// gameplay loading remains opt-in until the main game flow is ready.
    /// </summary>
    public sealed class PeacelandSaveLoadSceneController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private PeacelandSaveSlotPanel slotPanel;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text feedbackText;

        [Header("Navigation")]
        [SerializeField] private string returnSceneName = "DemoStart";
        [SerializeField] private string firstGameplaySceneName = "NoteBookTesting";
        [SerializeField] private bool loadGameplaySceneAfterSelection;
        [SerializeField] private bool clearActiveSlotOnOpen = true;

        private bool selectionInProgress;
        private int pendingDeleteSlotIndex = -1;

        private void Awake()
        {
            PeacelandGameBootstrap.EnsureExists();

            if (clearActiveSlotOnOpen)
            {
                PeacelandSaveService.Instance.ClearActiveSlotSelection();
            }

            if (slotPanel != null)
            {
                slotPanel.InitializeStandalone(SelectSlot, RequestDeleteSlot, MoveSlot);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(ReturnToPreviousScene);
                backButton.onClick.AddListener(ReturnToPreviousScene);
            }

            SetFeedback("Select a save. Each slot owns separate notebook data.");
        }

        private void SelectSlot(int slotIndex)
        {
            if (selectionInProgress)
            {
                return;
            }

            selectionInProgress = true;
            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            saveService.TryGetSlotSummary(slotIndex, out PeacelandSaveSlotSummary summary);

            bool ready;
            if (summary.hasData)
            {
                ready = saveService.ActivateSlotAndContinue(slotIndex);
            }
            else
            {
                saveService.ActivateSlotAndStartNewGame(slotIndex);
                ready = true;
            }

            if (!ready)
            {
                SetFeedback("Save " + (slotIndex + 1) + " could not be loaded.");
                selectionInProgress = false;
                return;
            }

            saveService.TryGetNotebookSnapshot(slotIndex, out Peaceland.Notebook.NotebookSaveData notebook);
            int notebookStateCount = notebook != null && notebook.states != null ? notebook.states.Count : 0;
            SetFeedback("Save " + (slotIndex + 1) + " ready | notebook states: " + notebookStateCount);
            slotPanel.Refresh();

            if (loadGameplaySceneAfterSelection)
            {
                LoadGameplayScene(saveService.GetLastSceneName());
                return;
            }

            selectionInProgress = false;
        }

        private void LoadGameplayScene(string lastSceneName)
        {
            string targetScene = !string.IsNullOrWhiteSpace(lastSceneName)
                && Application.CanStreamedLevelBeLoaded(lastSceneName)
                    ? lastSceneName
                    : firstGameplaySceneName;

            if (!string.IsNullOrWhiteSpace(targetScene) && Application.CanStreamedLevelBeLoaded(targetScene))
            {
                SceneManager.LoadScene(targetScene);
                return;
            }

            SetFeedback("The main game scene is not connected yet. The selected save remains active.");
            selectionInProgress = false;
        }

        private void RequestDeleteSlot(int slotIndex)
        {
            if (pendingDeleteSlotIndex != slotIndex)
            {
                pendingDeleteSlotIndex = slotIndex;
                SetFeedback("Click X again to delete Save " + (slotIndex + 1) + ".");
                return;
            }

            pendingDeleteSlotIndex = -1;
            if (PeacelandSaveService.Instance.DeleteSlot(slotIndex))
            {
                slotPanel.Refresh();
                SetFeedback("Save " + (slotIndex + 1) + " deleted.");
            }
        }

        private void MoveSlot(int sourceSlotIndex, int destinationSlotIndex)
        {
            pendingDeleteSlotIndex = -1;
            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            saveService.TryGetSlotSummary(destinationSlotIndex, out PeacelandSaveSlotSummary destination);
            if (destination.hasData)
            {
                SetFeedback("Save " + (destinationSlotIndex + 1) + " already has data. Move cancelled.");
                return;
            }

            if (!saveService.MoveSlot(sourceSlotIndex, destinationSlotIndex))
            {
                SetFeedback("Could not move Save " + (sourceSlotIndex + 1) + ".");
                return;
            }

            slotPanel.Refresh();
            SetFeedback(
                "Moved Save " + (sourceSlotIndex + 1)
                + " to Save " + (destinationSlotIndex + 1) + ".");
        }

        private void ReturnToPreviousScene()
        {
            if (!string.IsNullOrWhiteSpace(returnSceneName)
                && Application.CanStreamedLevelBeLoaded(returnSceneName))
            {
                SceneManager.LoadScene(returnSceneName);
                return;
            }

            SetFeedback("Return scene is not available in Build Settings.");
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
            }

            Debug.Log("[SaveLoad] " + message);
        }
    }
}
