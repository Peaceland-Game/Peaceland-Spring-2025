using UnityEngine;

namespace Peaceland
{
    /// <summary>
    /// Story flags and the resume scene. Same JSON as notebook + stats.
    /// </summary>
    public sealed class PeacelandProgress : MonoBehaviour
    {
        private static PeacelandProgress instance;

        public static PeacelandProgress Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PeacelandProgress>();
                }

                if (instance == null)
                {
                    PeacelandGameBootstrap.EnsureExists();
                    instance = FindFirstObjectByType<PeacelandProgress>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public bool HasFlag(string flagId)
        {
            return PeacelandSaveService.Instance.HasProgressFlag(flagId);
        }

        /// <summary>
        /// Sets a named flag on the active slot (example: memory1_completed).
        /// </summary>
        public void SetFlag(string flagId, bool value = true)
        {
            PeacelandSaveService.Instance.SetProgressFlag(flagId, value);
        }

        public void ClearFlag(string flagId)
        {
            PeacelandSaveService.Instance.SetProgressFlag(flagId, false);
        }

        public string GetCurrentSceneCheckpoint()
        {
            return PeacelandSaveService.Instance.GetLastSceneName();
        }

        public void SetCurrentSceneCheckpoint(string sceneName)
        {
            SetCurrentSceneCheckpoint(sceneName, sceneName, sceneName, true);
        }

        /// <summary>
        /// Writes the resume scene onto the active slot. SaveLoad itself should not call this;
        /// put PeacelandSceneCheckpointPolicy on that scene with recordAsGameplayCheckpoint off.
        /// </summary>
        public void SetCurrentSceneCheckpoint(
            string sceneName,
            string checkpointKey,
            string displayName,
            bool replaceMatchingKey)
        {
            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            if (!saveService.HasActiveSlot)
            {
                saveService.SetLastSceneName(sceneName);
                return;
            }

            saveService.CaptureCheckpoint(
                string.IsNullOrWhiteSpace(checkpointKey) ? sceneName : checkpointKey,
                string.IsNullOrWhiteSpace(displayName) ? sceneName : displayName,
                sceneName,
                replaceMatchingKey);
        }
    }
}
