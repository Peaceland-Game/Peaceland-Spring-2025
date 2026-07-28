#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Peaceland.Notebook.Editor
{
    /// <summary>
    /// Optional: re-run Author when NoteBookTesting opens. Off by default so manual layout tuning is not wiped.
    /// Enable via Peaceland → Notebook → Preferences → Auto-Author NoteBookTesting On Open.
    /// </summary>
    [InitializeOnLoad]
    internal static class NotebookTestSceneAutoAuthor
    {
        private const string PrefKey = "Peaceland.Notebook.AutoAuthorOnSceneOpen";
        private const string TestingSceneSuffix = "NoteBookTesting.unity";

        static NotebookTestSceneAutoAuthor()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        public static bool AutoAuthorEnabled
        {
            get => EditorPrefs.GetBool(PrefKey, false);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        [MenuItem("Peaceland/Notebook/Preferences/Auto-Author NoteBookTesting On Open", false, 300)]
        private static void ToggleAutoAuthor()
        {
            AutoAuthorEnabled = !AutoAuthorEnabled;
        }

        [MenuItem("Peaceland/Notebook/Preferences/Auto-Author NoteBookTesting On Open", true)]
        private static bool ToggleAutoAuthorValidate()
        {
            Menu.SetChecked("Peaceland/Notebook/Preferences/Auto-Author NoteBookTesting On Open", AutoAuthorEnabled);
            return true;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!AutoAuthorEnabled)
            {
                return;
            }

            if (!scene.path.Replace('\\', '/').EndsWith(TestingSceneSuffix))
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (!scene.isLoaded)
                {
                    return;
                }

                NotebookOpenUIAuthoring.AuthorInActiveScene();
                Debug.Log("Notebook: auto-authored NoteBookTesting (preference enabled).");
            };
        }
    }
}
#endif
