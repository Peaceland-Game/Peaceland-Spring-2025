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
        [Tooltip("The inspectable whose description the player opens.")]
        [SerializeField] private ViewDescription source;
        [Tooltip("The trigger whose entries are collected when the description is opened.")]
        [SerializeField] private NotebookCollectTrigger collectTrigger;

        private void Reset()
        {
            source = GetComponent<ViewDescription>();
            collectTrigger = GetComponent<NotebookCollectTrigger>();
            if (collectTrigger != null)
            {
                // Opening the description should be the only way in, not clicking the object.
                collectTrigger.SetClickToCollect(false);
            }
        }

        private void OnEnable()
        {
            if (source == null || collectTrigger == null)
            {
                Debug.LogWarning(name + ": set both Source and Collect Trigger, otherwise viewing the description collects nothing.", this);
            }

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
