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
        [SerializeField] private PeacelandMinigameProgressBridge progressBridge;
        [SerializeField] private MinigameBehavior whenMinigameStarts;

        [Header("Save Point")]
        [SerializeField] private PeacelandCheckpointTrigger checkpointTrigger;

        public MinigameBehavior TargetMinigame => whenMinigameStarts;

        private void OnEnable()
        {
            if (progressBridge != null)
            {
                progressBridge.MinigameStarted += HandleMinigameStarted;
            }
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
            checkpointTrigger?.Capture();
        }
    }
}
