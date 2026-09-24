using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland
{
    public sealed class PeacelandCheckpointEntryView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button selectButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private GameObject activeMarker;

        private PeacelandCheckpointSummary summary;

        public void Bind(
            PeacelandCheckpointSummary checkpoint,
            Action<PeacelandCheckpointSummary> selected)
        {
            summary = checkpoint;
            if (titleText != null)
            {
                titleText.text = summary.GetDisplayName();
            }

            if (detailText != null)
            {
                detailText.text = summary.sceneName + "\n" + summary.GetDisplaySavedTimeLocal();
            }

            if (activeMarker != null)
            {
                activeMarker.SetActive(summary.isActive);
            }

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => selected?.Invoke(summary));
            }
        }
    }
}
