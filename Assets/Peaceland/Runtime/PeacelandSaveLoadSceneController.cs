using System.Collections.Generic;
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
        [SerializeField] private PeacelandCheckpointListPanel checkpointPanel;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text feedbackText;

        [Header("Navigation")]
        [SerializeField] private string returnSceneName = "DemoStart";
        [SerializeField] private string firstGameplaySceneName = "DemoDisclaimer";
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

            if (summary.isCorrupt)
            {
                SetFeedback(
                    "Save " + (slotIndex + 1)
                    + " is damaged and was not overwritten. Delete it explicitly to reuse the slot.");
                selectionInProgress = false;
                return;
            }

            bool ready;
            if (summary.hasData)
            {
                if (checkpointPanel != null
                    && saveService.TryGetCheckpointSummaries(
                        slotIndex,
                        out List<PeacelandCheckpointSummary> checkpoints)
                    && checkpoints.Count > 1
                    && checkpointPanel.Show(slotIndex, checkpoints, SelectCheckpoint))
                {
                    SetFeedback(
                        "Save " + (slotIndex + 1) + " has "
                        + checkpoints.Count + " checkpoints. Choose one to continue.");
                    selectionInProgress = false;
                    return;
                }

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

        private void SelectCheckpoint(PeacelandCheckpointSummary checkpoint)
        {
            if (selectionInProgress)
            {
                return;
            }

            selectionInProgress = true;
            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            if (!saveService.ActivateSlotAtCheckpoint(
                    checkpoint.slotIndex,
                    checkpoint.checkpointId))
            {
                SetFeedback("That checkpoint could not be loaded.");
                selectionInProgress = false;
                return;
            }

            slotPanel.Refresh();
            SetFeedback(
                "Loaded " + checkpoint.GetDisplayName()
                + " from Save " + (checkpoint.slotIndex + 1) + ".");

            if (loadGameplaySceneAfterSelection)
            {
                LoadGameplayScene(checkpoint.sceneName);
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
            if (checkpointPanel != null)
            {
                checkpointPanel.Hide();
            }

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
