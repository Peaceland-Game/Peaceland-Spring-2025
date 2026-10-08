using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Collects the trigger's entries when a drag puzzle is finished.
    /// Works with the original DragManager and with the Fall 26 demo's F26_DragManager; fill in whichever the scene uses.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotebookCollectOnDragCompleted : MonoBehaviour
    {
        [Tooltip("Drag puzzle in older scenes. Leave empty if the scene uses F26_DragManager.")]
        [SerializeField] private DragManager source;
        [Tooltip("Drag puzzle in Fall 26 demo scenes (letter puzzle).")]
        [SerializeField] private F26_DragManager fall26Source;
        [Tooltip("The trigger whose entries are collected when the puzzle is done.")]
        [SerializeField] private NotebookCollectTrigger collectTrigger;

        private void Reset()
        {
            collectTrigger = GetComponent<NotebookCollectTrigger>();
            if (collectTrigger != null)
            {
                // Finishing the puzzle should be the only way in, not clicking the object.
                collectTrigger.SetClickToCollect(false);
            }
        }

        private void OnEnable()
        {
            if (source == null && fall26Source == null)
            {
                Debug.LogWarning(name + ": set Source or Fall26 Source, otherwise finishing the puzzle collects nothing.", this);
            }

            if (collectTrigger == null)
            {
                Debug.LogWarning(name + ": Collect Trigger is empty, so nothing will be collected.", this);
            }

            if (source != null)
            {
                source.OnCompleted += HandleCompleted;
            }

            if (fall26Source != null)
            {
                fall26Source.OnCompleted += HandleCompleted;
            }
        }

        private void OnDisable()
        {
            if (source != null)
            {
                source.OnCompleted -= HandleCompleted;
            }

            if (fall26Source != null)
            {
                fall26Source.OnCompleted -= HandleCompleted;
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
