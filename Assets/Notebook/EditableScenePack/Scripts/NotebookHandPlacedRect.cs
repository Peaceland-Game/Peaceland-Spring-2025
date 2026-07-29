using UnityEngine;

namespace Peaceland.Notebook.EditableScenePack
{
    /// <summary>
    /// Attach to any notebook UI RectTransform you place by hand in the Scene view.
    /// When Respect Scene Layout is on, runtime/bootstrap/authoring helpers must not overwrite this rect.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class NotebookHandPlacedRect : MonoBehaviour
    {
        [SerializeField] private bool respectSceneLayout = true;
        [SerializeField] private NotebookUILayoutRole layoutRole = NotebookUILayoutRole.Unspecified;
        [TextArea(1, 3)]
        [SerializeField] private string authorNote;

        public bool RespectSceneLayout => respectSceneLayout;
        public NotebookUILayoutRole LayoutRole => layoutRole;
        public string AuthorNote => authorNote;

        public void SetRole(NotebookUILayoutRole role, string note = null)
        {
            layoutRole = role;
            if (note != null)
            {
                authorNote = note;
            }
        }
    }
}
