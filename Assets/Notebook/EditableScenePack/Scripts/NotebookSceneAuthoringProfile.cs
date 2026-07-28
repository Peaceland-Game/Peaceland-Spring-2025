using System.Collections.Generic;
using UnityEngine;

namespace Peaceland.Notebook.EditableScenePack
{
    /// <summary>
    /// Scene root policy: use Hierarchy-authored UI; do not rebuild notebook shell on Play.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotebookSceneAuthoringProfile : MonoBehaviour
    {
        [Tooltip("When true, NotebookTestSceneBootstrap will NOT call EnsureSetup() on Play.")]
        [SerializeField] private bool disableRuntimeBootstrapRebuild = true;

        [Tooltip("Still auto-collect pagination dummy entries on Play (test readability only).")]
        [SerializeField] private bool collectDummyEntriesOnPlay = true;

        [Tooltip("Optional explicit list; use Refresh Interacts menu to sync from markers in scene.")]
        [SerializeField] private List<NotebookSceneInteractMarker> sceneInteracts = new List<NotebookSceneInteractMarker>();

        public bool DisableRuntimeBootstrapRebuild => disableRuntimeBootstrapRebuild;
        public bool CollectDummyEntriesOnPlay => collectDummyEntriesOnPlay;
        public IReadOnlyList<NotebookSceneInteractMarker> SceneInteracts => sceneInteracts;

        public void SetInteracts(List<NotebookSceneInteractMarker> markers)
        {
            sceneInteracts = markers ?? new List<NotebookSceneInteractMarker>();
        }

        public static NotebookSceneAuthoringProfile FindInScene()
        {
            return FindFirstObjectByType<NotebookSceneAuthoringProfile>();
        }
    }
}
