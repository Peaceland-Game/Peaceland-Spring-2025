using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
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
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        public void Configure(NotebookController controller)
        {
            notebookController = controller;
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
    }
}
