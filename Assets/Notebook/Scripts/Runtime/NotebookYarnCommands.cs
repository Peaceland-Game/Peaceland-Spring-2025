using UnityEngine;
using Yarn.Unity;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Yarn command &lt;&lt;collect_note EntryId&gt;&gt;. Put this on the same object as the scene's DialogueRunner.
    /// EntryId is the Entry Id field of a NotebookEntryDefinition asset. Works with or without a notebook in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DialogueRunner))]
    public sealed class NotebookYarnCommands : MonoBehaviour
    {
        private const string CommandName = "collect_note";

        [Tooltip("Filled automatically from this object.")]
        [SerializeField] private DialogueRunner dialogueRunner;
        [Tooltip("Optional. When set, a misspelled entry id logs a warning instead of failing silently.")]
        [SerializeField] private NotebookDatabase database;

        private void Awake()
        {
            if (dialogueRunner == null)
            {
                dialogueRunner = GetComponent<DialogueRunner>();
            }

            dialogueRunner.AddCommandHandler<string>(CommandName, CollectNote);
        }

        private void OnDestroy()
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.RemoveCommandHandler(CommandName);
            }
        }

        /// <summary>Yarn: &lt;&lt;collect_note rj-torn-letter&gt;&gt;.</summary>
        private void CollectNote(string entryId)
        {
            if (database != null && database.GetEntry(entryId) == null)
            {
                Debug.LogWarning("Yarn <<collect_note>>: no notebook entry with id '" + entryId + "' in " + database.name + ".", this);
                return;
            }

            NotebookGlobalBridge.CollectEntry(entryId);
        }
    }
}
