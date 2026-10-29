using UnityEngine;
using UnityEngine.SceneManagement;

namespace Peaceland
{
    /// <summary>Inspector-authored checkpoint that can be called by a UnityEvent.</summary>
    public sealed class PeacelandCheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private string checkpointKey;
        [SerializeField] private string displayName;
        [SerializeField] private bool replaceMatchingCheckpoint;

        public void Capture()
        {
            Scene scene = gameObject.scene.IsValid()
                ? gameObject.scene
                : SceneManager.GetActiveScene();

            PeacelandSaveService.Instance.CaptureCheckpoint(
                string.IsNullOrWhiteSpace(checkpointKey)
                    ? scene.name + "/" + gameObject.name
                    : checkpointKey,
                string.IsNullOrWhiteSpace(displayName)
                    ? gameObject.name
                    : displayName,
                scene.name,
                replaceMatchingCheckpoint);
        }
    }
}
