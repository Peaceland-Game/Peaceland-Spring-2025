using Peaceland.Notebook;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Peaceland.Notebook.Editor
{
    /// <summary>
    /// Prefab-based Notebook scene authoring entry points.
    /// Presentation is edited in prefabs, never rebuilt by this class.
    /// </summary>
    public static class NotebookOpenUIAuthoring
    {
        [MenuItem("Peaceland/Notebook/Author Open UI In Active Scene")]
        public static void AuthorInActiveSceneMenu()
        {
            AuthorInActiveScene();
        }

        [MenuItem("Peaceland/Notebook/Wire Controller To Existing UI")]
        public static void WireControllerMenu()
        {
            WireControllerToShell();
        }

        [MenuItem("Peaceland/Notebook/Layout/Lock UI For Hand Editing")]
        public static void LockUILayoutForHandEditing()
        {
            SetLayoutLockOnActiveScene(true);
        }

        [MenuItem("Peaceland/Notebook/Layout/Unlock UI For Auto Repair")]
        public static void UnlockUILayoutForAutoRepair()
        {
            SetLayoutLockOnActiveScene(false);
        }

        [MenuItem("Peaceland/Notebook/Cleanup Legacy Test UI")]
        public static void CleanupLegacyMenu()
        {
            CleanupLegacyTestUi();
        }

        public static void CleanupLegacyTestUi()
        {
            global::Peaceland.Notebook.Editor.NotebookPrefabSceneAuthoring
                .RemoveLegacyNotebookObjects();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        public static void AuthorInActiveScene()
        {
            AuthorInActiveScene(true);
        }

        public static void AuthorInActiveScene(bool ensurePlaytestInfrastructure)
        {
            if (ensurePlaytestInfrastructure)
            {
                global::Peaceland.Notebook.Editor.NotebookScenePlayabilityEditor
                    .EnsurePlayableActiveScene();
            }

            global::Peaceland.Notebook.Editor.NotebookPrefabSceneAuthoring
                .EnsurePrefabsInActiveScene(ensurePlaytestInfrastructure);
            WireControllerToShell();
        }

        public static void WireControllerToShell()
        {
            global::Peaceland.Notebook.NotebookController controller =
                Object.FindFirstObjectByType<global::Peaceland.Notebook.NotebookController>(
                    FindObjectsInactive.Include);
            global::Peaceland.Notebook.NotebookUIShellReferences shell =
                global::Peaceland.Notebook.NotebookSceneLookup.FindShell();
            if (controller == null || shell == null)
            {
                Debug.LogWarning(
                    "Notebook wire skipped: add NotebookProductionSceneUI.prefab to the scene.");
                return;
            }

            shell.ApplyToController(controller);
            global::Peaceland.Notebook.NotebookSceneLookup.FindOpenButton()?.Configure(controller);
            Object.FindFirstObjectByType<global::Peaceland.Notebook.NotebookTestHarness>(
                    FindObjectsInactive.Include)
                ?.Configure(controller, shell);

            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }

        private static void SetLayoutLockOnActiveScene(bool locked)
        {
            global::Peaceland.Notebook.NotebookBookArtLayout layout =
                Object.FindFirstObjectByType<global::Peaceland.Notebook.NotebookBookArtLayout>(
                    FindObjectsInactive.Include);
            if (layout == null)
            {
                Debug.LogWarning(
                    "Notebook layout lock skipped: no NotebookProductionSceneUI prefab instance found.");
                return;
            }

            layout.SetLockLayout(locked);
            EditorUtility.SetDirty(layout);
            PrefabUtility.RecordPrefabInstancePropertyModifications(layout);
            EditorSceneManager.MarkSceneDirty(layout.gameObject.scene);
        }
    }
}
