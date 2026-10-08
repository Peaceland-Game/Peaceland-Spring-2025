using UnityEngine;

namespace Peaceland
{
    /// <summary>
    /// When a named minigame starts, writes GenericMemManager progress and captures a checkpoint.
    /// Put this next to PeacelandMinigameProgressBridge on Florist / R&amp;J memory scenes.
    /// </summary>
    public sealed class PeacelandMinigameCheckpointMilestone : MonoBehaviour
    {
        [Header("When")]
        [Tooltip("The progress bridge on this scene's memory manager.")]
        [SerializeField] private PeacelandMinigameProgressBridge progressBridge;
        [Tooltip("Save when this minigame (from the manager's Minigames list) starts. The first minigame works too.")]
        [SerializeField] private MinigameBehavior whenMinigameStarts;

        [Header("Save Point")]
        [Tooltip("Checkpoint to record. Optional: without it, only the minigame index is saved.")]
        [SerializeField] private PeacelandCheckpointTrigger checkpointTrigger;

        public MinigameBehavior TargetMinigame => whenMinigameStarts;

        private void OnEnable()
        {
            if (progressBridge == null || whenMinigameStarts == null)
            {
                Debug.LogWarning(
                    name + ": checkpoint milestone needs both Progress Bridge and When Minigame Starts, so it will never save.",
                    this);
                return;
            }

            progressBridge.MinigameStarted += HandleMinigameStarted;
        }

        private void OnDisable()
        {
            if (progressBridge != null)
            {
                progressBridge.MinigameStarted -= HandleMinigameStarted;
            }
        }

        private void HandleMinigameStarted(MinigameBehavior minigame)
        {
            if (minigame == null || minigame != whenMinigameStarts)
            {
                return;
            }

            progressBridge.CaptureProgress();
            if (checkpointTrigger != null)
            {
                checkpointTrigger.Capture();
            }
        }
    }
}
