using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland
{
    public sealed class PeacelandCheckpointListPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text headingText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform entryContainer;
        [SerializeField] private PeacelandCheckpointEntryView entryTemplate;

        private readonly List<PeacelandCheckpointEntryView> entries =
            new List<PeacelandCheckpointEntryView>();
        private Action<PeacelandCheckpointSummary> selected;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }

        }

        public bool Show(
            int slotIndex,
            IReadOnlyList<PeacelandCheckpointSummary> checkpoints,
            Action<PeacelandCheckpointSummary> onSelected)
        {
            if (entryTemplate == null || checkpoints == null || checkpoints.Count == 0)
            {
                return false;
            }

            selected = onSelected;
            EnsureEntries(checkpoints.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                bool visible = i < checkpoints.Count;
                entries[i].gameObject.SetActive(visible);
                if (visible)
                {
                    entries[i].Bind(checkpoints[i], Select);
                }
            }

            if (headingText != null)
            {
                headingText.text = "SAVE " + (slotIndex + 1) + " CHECKPOINTS";
            }

            (root != null ? root : gameObject).SetActive(true);
            return true;
        }

        public void Hide()
        {
            selected = null;
            (root != null ? root : gameObject).SetActive(false);
        }

        private void Select(PeacelandCheckpointSummary checkpoint)
        {
            Action<PeacelandCheckpointSummary> callback = selected;
            Hide();
            callback?.Invoke(checkpoint);
        }

        private void EnsureEntries(int count)
        {
            if (entries.Count == 0)
            {
                entries.Add(entryTemplate);
            }

            Transform parent = entryContainer != null
                ? entryContainer
                : entryTemplate.transform.parent;
            while (entries.Count < count)
            {
                PeacelandCheckpointEntryView entry = Instantiate(entryTemplate, parent);
                entry.name = "Checkpoint " + (entries.Count + 1);
                entries.Add(entry);
            }
        }
    }
}
