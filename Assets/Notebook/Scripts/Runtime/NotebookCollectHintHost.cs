using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Drives collectible hint UI in satellite scenes that do not load the full open-notebook shell.
    /// </summary>
    public class NotebookCollectHintHost : MonoBehaviour
    {
        [SerializeField] private NotebookOverlayView overlayView;
        [SerializeField] private NotebookController notebookController;

        private int activeCollectableSources;

        private void Awake()
        {
            if (overlayView == null)
            {
                overlayView = FindFirstObjectByType<NotebookOverlayView>(FindObjectsInactive.Include);
            }

            if (notebookController == null)
            {
                notebookController = NotebookSceneLookup.FindController();
            }
        }

        public void RegisterCollectableSource()
        {
            NotebookController controller = notebookController != null ? notebookController : NotebookSceneLookup.FindController();
            if (controller != null)
            {
                controller.RegisterCollectableSource();
                return;
            }

            activeCollectableSources++;
            RefreshHint();
        }

        public void UnregisterCollectableSource()
        {
            NotebookController controller = notebookController != null ? notebookController : NotebookSceneLookup.FindController();
            if (controller != null)
            {
                controller.UnregisterCollectableSource();
                return;
            }

            activeCollectableSources = Mathf.Max(0, activeCollectableSources - 1);
            RefreshHint();
        }

        public void PlayCollectedToast(string message)
        {
            if (overlayView == null)
            {
                return;
            }

            overlayView.PlayCollectedToast(message);
        }

        private void RefreshHint()
        {
            if (overlayView == null)
            {
                return;
            }

            overlayView.SetCollectibleHintVisible(activeCollectableSources > 0, activeCollectableSources);
        }
    }
}
