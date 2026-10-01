using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Forwards DragManager.OnCompleted to NotebookCollectTrigger.Collect().
    /// NOTE: Fall 26 demo scenes use F26_DragManager, which is a copy. Point this at
    /// that scene's drag manager, or add the same hook there.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotebookCollectOnDragCompleted : MonoBehaviour
    {
        [SerializeField] private DragManager source;
        [SerializeField] private NotebookCollectTrigger collectTrigger;

        private void OnEnable()
        {
            if (source != null)
            {
                source.OnCompleted += HandleCompleted;
            }
        }

        private void OnDisable()
        {
            if (source != null)
            {
                source.OnCompleted -= HandleCompleted;
            }
        }

        private void HandleCompleted()
        {
            if (collectTrigger != null)
            {
                collectTrigger.Collect();
            }
        }
    }
}
