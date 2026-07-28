using UnityEngine;

namespace Peaceland
{
    /// <summary>Controls whether loading this scene should replace the active save's resume checkpoint.</summary>
    public sealed class PeacelandSceneCheckpointPolicy : MonoBehaviour
    {
        [SerializeField] private bool recordAsGameplayCheckpoint = true;

        public bool RecordAsGameplayCheckpoint => recordAsGameplayCheckpoint;
    }
}
