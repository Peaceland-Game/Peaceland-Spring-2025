using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Icon / button that toggles the book. Lives on NotebookProductionSceneUI.
    /// If no controller is assigned, clicking returns to the notebook home scene.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class NotebookOpenButton : MonoBehaviour
    {
        [SerializeField] private NotebookController notebookController;
        [SerializeField] private Button button;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void Awake()
        {
            WireButton();
        }

        private void OnEnable()
        {
            WireButton();
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        /// <summary>Assigns the book this icon should toggle. Called when the production prefab is placed.</summary>
        public void Configure(NotebookController controller)
        {
            notebookController = controller;
            WireButton();
        }

        private void HandleClick()
        {
            NotebookController controller = ResolveController();
            if (controller == null)
            {
                NotebookReturnHomeButton.ReturnHome();
                return;
            }

            controller.ToggleNotebook();
        }

        private NotebookController ResolveController()
        {
            if (notebookController != null)
            {
                return notebookController;
            }

            notebookController = FindFirstObjectByType<NotebookController>();
            return notebookController;
        }

        private void WireButton()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }
}
