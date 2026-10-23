using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookJournalCardView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Image iconImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject newBadgeRoot;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private UnityEngine.UI.Button cardButton;

        private Action onViewedAction;
        private Action<ArtifactDefinition> onInspectAction;
        private ArtifactDefinition currentArtifact;

        public void Bind(ArtifactDefinition artifact, bool isNew, Action onViewed, Action<ArtifactDefinition> onInspect = null)
        {
            currentArtifact = artifact;
            onViewedAction = onViewed;
            onInspectAction = onInspect;

            if (artifact == null) return;

            if (titleText != null)
            {
                titleText.text = artifact.Title;
            }

            if (descriptionText != null)
            {
                descriptionText.text = artifact.Description;
            }

            if (iconImage != null)
            {
                if (artifact.Icon != null)
                {
                    iconImage.sprite = artifact.Icon;
                    iconImage.enabled = true;
                    iconImage.preserveAspect = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            if (newBadgeRoot != null)
            {
                newBadgeRoot.SetActive(isNew);
            }

            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(OnCardClicked);
            }
        }

        private void OnCardClicked()
        {
            if (newBadgeRoot != null && newBadgeRoot.activeSelf)
            {
                newBadgeRoot.SetActive(false);
            }
            onViewedAction?.Invoke();
            onInspectAction?.Invoke(currentArtifact);
        }
    }
}
