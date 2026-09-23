using UnityEngine;

namespace Peaceland
{
    /// <summary>Controls whether loading this scene should replace the active save's resume checkpoint.</summary>
    public sealed class PeacelandSceneCheckpointPolicy : MonoBehaviour
    {
        [Header("Save / Load")]
        [Tooltip("When enabled, loading this scene updates the active save slot's resume scene.")]
        [SerializeField] private bool recordAsGameplayCheckpoint = true;

        [Tooltip("Required when this scene is intentionally excluded from resume checkpoints.")]
        [SerializeField] private string exclusionReason;

        [Header("Checkpoint Identity")]
        [Tooltip("Stable authored key. Leave empty to use the scene name.")]
        [SerializeField] private string checkpointKey;

        [Tooltip("Player-facing checkpoint title. Leave empty to use the scene name.")]
        [SerializeField] private string checkpointDisplayName;

        [Tooltip("Replace the previous automatic checkpoint for this key instead of growing history on every scene load.")]
        [SerializeField] private bool replaceMatchingCheckpoint = true;

        public bool RecordAsGameplayCheckpoint => recordAsGameplayCheckpoint;
        public string ExclusionReason => exclusionReason;
        public string CheckpointKey => checkpointKey;
        public string CheckpointDisplayName => checkpointDisplayName;
        public bool ReplaceMatchingCheckpoint => replaceMatchingCheckpoint;
    }
}
