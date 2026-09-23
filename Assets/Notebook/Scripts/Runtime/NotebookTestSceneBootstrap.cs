using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Legacy-compatible binder for scenes that still reference the old bootstrap type.
    /// UI and test controls must be authored as prefab instances.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class NotebookTestSceneBootstrap : MonoBehaviour
    {
        [Header("Prefab references")]
        [SerializeField] private NotebookController notebookController;
        [SerializeField] private NotebookUIShellReferences shell;
        [SerializeField] private NotebookOpenButton openButton;
        [SerializeField] private NotebookTestHarness testHarness;

        [Header("Compatibility")]
        [SerializeField] private bool bindOnEnable = true;

        private void Reset()
        {
            FindReferences();
        }

        private void OnEnable()
        {
            if (bindOnEnable)
            {
                Bind();
            }
        }

        /// <summary>
        /// Compatibility entry point retained for older authoring tools and scenes.
        /// It binds existing prefab instances and never creates UI or data assets.
        /// </summary>
        public void EditorEnsureSetup()
        {
            Bind();
        }

        [ContextMenu("Bind Existing Notebook Prefabs")]
        public void Bind()
        {
            FindReferences();
            if (notebookController == null || shell == null)
            {
                Debug.LogWarning(
                    "Notebook prefab binding skipped. Add NotebookProductionSceneUI.prefab to the scene.",
                    this);
                return;
            }

            shell.ApplyToController(notebookController);
            if (openButton != null)
            {
                openButton.Configure(notebookController);
            }

            if (testHarness != null)
            {
                testHarness.Configure(notebookController, shell);
            }
        }

        private void FindReferences()
        {
            if (notebookController == null)
            {
                notebookController = FindFirstObjectByType<NotebookController>(FindObjectsInactive.Include);
            }

            if (shell == null)
            {
                shell = NotebookSceneLookup.FindShell();
            }

            if (openButton == null)
            {
                openButton = NotebookSceneLookup.FindOpenButton();
            }

            if (testHarness == null)
            {
                testHarness = FindFirstObjectByType<NotebookTestHarness>(FindObjectsInactive.Include);
            }
        }
    }
}
