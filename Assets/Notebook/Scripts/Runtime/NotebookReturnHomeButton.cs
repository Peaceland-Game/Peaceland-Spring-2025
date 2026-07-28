using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Satellite test scenes: bottom-left button returns to NoteBookTesting to read collected entries.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class NotebookReturnHomeButton : MonoBehaviour
    {
        public const string HomeSceneName = "NoteBookTesting";

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
                button.onClick.RemoveListener(ReturnHome);
                button.onClick.AddListener(ReturnHome);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(ReturnHome);
            }
        }

        public static void ReturnHome()
        {
            SceneManager.LoadScene(HomeSceneName);
        }
    }
}
