using System;
using System.Collections.Generic;
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
        [Header("Dynamic Slots")]
        [SerializeField] private PeacelandSaveSlotEntryView slotEntryTemplate;
        [SerializeField] private Transform slotEntryContainer;
        [Min(1)]
        [SerializeField] private int initialVisibleSlotCount =
            PeacelandSaveSlots.InitialVisibleSlotCount;
        [Min(1)]
        [SerializeField] private int emptySlotBuffer = 1;

        private readonly List<PeacelandSaveSlotEntryView> runtimeEntries =
            new List<PeacelandSaveSlotEntryView>();

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
            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            EnsureSlotEntries(PeacelandSaveService.GetVisibleSlotCount(
                initialVisibleSlotCount,
                emptySlotBuffer));

            for (int i = 0; i < slotEntries.Length; i++)
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

        private void EnsureSlotEntries(int requiredCount = -1)
        {
            if (runtimeEntries.Count == 0)
            {
                PeacelandSaveSlotEntryView[] authoredEntries =
                    slotEntries != null && slotEntries.Length > 0
                        ? slotEntries
                        : GetComponentsInChildren<PeacelandSaveSlotEntryView>(true);

                foreach (PeacelandSaveSlotEntryView entry in authoredEntries)
                {
                    if (entry != null && !runtimeEntries.Contains(entry))
                    {
                        runtimeEntries.Add(entry);
                    }
                }
            }

            if (slotEntryTemplate == null && runtimeEntries.Count > 0)
            {
                slotEntryTemplate = runtimeEntries[0];
            }

            if (slotEntryContainer == null && slotEntryTemplate != null)
            {
                slotEntryContainer = slotEntryTemplate.transform.parent;
            }

            int targetCount = requiredCount > 0
                ? requiredCount
                : Mathf.Max(initialVisibleSlotCount, runtimeEntries.Count);

            while (runtimeEntries.Count < targetCount && slotEntryTemplate != null)
            {
                PeacelandSaveSlotEntryView entry = Instantiate(
                    slotEntryTemplate,
                    slotEntryContainer);
                entry.name = "Save Slot " + (runtimeEntries.Count + 1);
                runtimeEntries.Add(entry);
            }

            slotEntries = runtimeEntries.ToArray();
        }
    }
}
