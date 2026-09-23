using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Forwards ViewDescription.OnViewDescriptionClicked to NotebookCollectTrigger.Collect().
    /// Use on War Room medals / photo inspectables that unlock a note when the description is opened.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotebookCollectOnDescriptionViewed : MonoBehaviour
    {
        [SerializeField] private ViewDescription source;
        [SerializeField] private NotebookCollectTrigger collectTrigger;

        private void OnEnable()
        {
            if (source != null)
            {
                source.OnViewDescriptionClicked += HandleDescriptionViewed;
            }
        }

        private void OnDisable()
        {
            if (source != null)
            {
                source.OnViewDescriptionClicked -= HandleDescriptionViewed;
            }
        }

        private void HandleDescriptionViewed(object sender, System.EventArgs args)
        {
            if (collectTrigger != null)
            {
                collectTrigger.Collect();
            }
        }
    }
}
