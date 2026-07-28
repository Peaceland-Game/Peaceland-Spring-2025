using UnityEngine;

namespace Peaceland.Notebook.EditableScenePack
{
    /// <summary>
    /// Scene-visible tag for player-facing interactables. Edit in Inspector; never spawn at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotebookSceneInteractMarker : MonoBehaviour
    {
        [SerializeField] private NotebookSceneInteractKind kind = NotebookSceneInteractKind.Other;
        [SerializeField] private NotebookSection section = NotebookSection.Present;
        [SerializeField] private string entryId;
        [SerializeField] private string label;
        [SerializeField] private string notes;

        public NotebookSceneInteractKind Kind => kind;
        public NotebookSection Section => section;
        public string EntryId => entryId;
        public string Label => label;
        public string Notes => notes;

        public void Configure(NotebookSceneInteractKind targetKind, string targetLabel, NotebookSection targetSection = NotebookSection.Directory, string targetEntryId = null)
        {
            kind = targetKind;
            label = targetLabel;
            section = targetSection;
            entryId = targetEntryId;
        }
    }
}
