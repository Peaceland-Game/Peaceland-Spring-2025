using System;
using System.Collections;
using UnityEngine;

namespace Peaceland
{
    /// <summary>
    /// Writes GenericMemManager minigame index into the active save so Florist / R&amp;J
    /// progress survives scene reloads. Requires an active save slot.
    /// </summary>
    [RequireComponent(typeof(GenericMemManager))]
    public sealed class PeacelandMinigameProgressBridge : MonoBehaviour
    {
        [Header("Progress Identity")]
        [Tooltip("Stable key shared by this scene's checkpoint milestones.")]
        [SerializeField] private string progressKey;
        [SerializeField] private GenericMemManager manager;

        [Header("Restore")]
        [SerializeField] private bool restoreOnStart = true;

        private int observedMinigame = int.MinValue;
        private bool ready;

        public event Action<MinigameBehavior> MinigameStarted;

        public GenericMemManager Manager => manager;
        public bool IsReady => ready;
        public string ProgressKey => progressKey;

        private IEnumerator Start()
        {
            manager ??= GetComponent<GenericMemManager>();
            yield return null;

            if (restoreOnStart)
            {
                RestoreProgress();
            }

            observedMinigame = manager != null
                ? manager.CurrentMinigame
                : int.MinValue;
            ready = true;
        }

        private void Update()
        {
            PollProgress();
        }

        public void PollProgress()
        {
            if (!ready || manager == null || manager.CurrentMinigame == observedMinigame)
            {
                return;
            }

            observedMinigame = manager.CurrentMinigame;
            MinigameStarted?.Invoke(GetCurrentMinigame());
        }

        /// <summary>Writes CurrentMinigame / CurrentOrder (and florist flower index) into the active save.</summary>
        public void CaptureProgress()
        {
            if (manager == null || string.IsNullOrWhiteSpace(progressKey))
            {
                return;
            }

            PeacelandSaveService service = PeacelandSaveService.Instance;
            service.SetProgressInt(progressKey + "/minigame", manager.CurrentMinigame);
            service.SetProgressInt(progressKey + "/order", manager.CurrentOrder);

            if (manager is FlowerShopManager)
            {
                service.SetProgressInt(
                    progressKey + "/flower",
                    FlowerShopManager.currentFlower);
            }
        }

        /// <summary>Restores GenericMemManager to the saved minigame/order. No-ops without an active slot.</summary>
        public bool RestoreProgress()
        {
            if (manager == null
                || string.IsNullOrWhiteSpace(progressKey)
                || !PeacelandSaveService.Instance.HasActiveSlot)
            {
                return false;
            }

            PeacelandSaveService service = PeacelandSaveService.Instance;
            if (!service.TryGetProgressInt(
                    progressKey + "/minigame",
                    out int minigameIndex))
            {
                return false;
            }

            service.TryGetProgressInt(progressKey + "/order", out int orderIndex);
            if (manager is FlowerShopManager
                && service.TryGetProgressInt(progressKey + "/flower", out int flowerIndex))
            {
                FlowerShopManager.currentFlower = flowerIndex;
            }

            manager.RestoreProgress(minigameIndex, orderIndex);
            observedMinigame = manager.CurrentMinigame;
            return true;
        }

        private MinigameBehavior GetCurrentMinigame()
        {
            return manager != null && manager.CurrentMinigame >= 0
                ? manager.GetCurrentMinigame()
                : null;
        }
    }
}
