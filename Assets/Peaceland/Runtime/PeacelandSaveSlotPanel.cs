using System;
using UnityEngine;

namespace Peaceland
{
    /// <summary>
    /// Save slot picker shown after pressing Start on the title screen.
    /// </summary>
    public sealed class PeacelandSaveSlotPanel : MonoBehaviour
    {
        [SerializeField] private PeacelandSaveSlotEntryView[] slotEntries;
        [SerializeField] private GameObject root;

        private PeacelandGameStartController startController;
        private Action<int> standaloneSlotSelected;
        private Action<int> standaloneDeleteRequested;
        private Action<int, int> standaloneMoveRequested;

        public void Initialize(PeacelandGameStartController controller)
        {
            startController = controller;
            standaloneSlotSelected = null;
            standaloneDeleteRequested = null;
            standaloneMoveRequested = null;
            EnsureSlotEntries();
            Refresh();
            Hide();
        }

        public void InitializeStandalone(
            Action<int> slotSelected,
            Action<int> deleteRequested = null,
            Action<int, int> moveRequested = null)
        {
            startController = null;
            standaloneSlotSelected = slotSelected;
            standaloneDeleteRequested = deleteRequested;
            standaloneMoveRequested = moveRequested;
            EnsureSlotEntries();
            Refresh();

            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void Show()
        {
            Refresh();
            if (root != null)
            {
                root.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void Refresh()
        {
            EnsureSlotEntries();
            PeacelandSaveService saveService = PeacelandSaveService.Instance;

            for (int i = 0; i < slotEntries.Length && i < PeacelandSaveSlots.SlotCount; i++)
            {
                PeacelandSaveSlotEntryView entry = slotEntries[i];
                if (entry == null)
                {
                    continue;
                }

                entry.Configure(i);
                entry.Bind(this);
                saveService.TryGetSlotSummary(i, out PeacelandSaveSlotSummary summary);
                entry.ApplySummary(summary);
            }
        }

        public void OnSlotSelected(int slotIndex)
        {
            if (standaloneSlotSelected != null)
            {
                standaloneSlotSelected.Invoke(slotIndex);
                return;
            }

            if (startController == null)
            {
                Debug.LogError("PeacelandSaveSlotPanel has no PeacelandGameStartController.");
                return;
            }

            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            saveService.TryGetSlotSummary(slotIndex, out PeacelandSaveSlotSummary summary);
            if (summary.hasData)
            {
                startController.ContinueFromSlot(slotIndex);
            }
            else
            {
                startController.StartNewGameInSlot(slotIndex);
            }
        }

        public void OnSlotDeleteRequested(int slotIndex)
        {
            if (standaloneDeleteRequested != null)
            {
                standaloneDeleteRequested.Invoke(slotIndex);
                return;
            }

            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            saveService.TryGetSlotSummary(slotIndex, out PeacelandSaveSlotSummary summary);
            if (!summary.hasData)
            {
                return;
            }

            saveService.DeleteSlot(slotIndex);
            Refresh();
        }

        public void OnSlotMoveRequested(int sourceSlotIndex, int destinationSlotIndex)
        {
            if (standaloneMoveRequested != null)
            {
                standaloneMoveRequested.Invoke(sourceSlotIndex, destinationSlotIndex);
                return;
            }

            if (PeacelandSaveService.Instance.MoveSlot(sourceSlotIndex, destinationSlotIndex))
            {
                Refresh();
            }
        }

        private void EnsureSlotEntries()
        {
            if (slotEntries != null && slotEntries.Length >= PeacelandSaveSlots.SlotCount)
            {
                return;
            }

            slotEntries = GetComponentsInChildren<PeacelandSaveSlotEntryView>(true);
        }
    }
}
