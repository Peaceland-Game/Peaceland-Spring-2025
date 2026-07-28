#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Peaceland.Notebook.Editor
{
    public static class NotebookTestSceneCleanup
    {
        private const string TestingScenePath = "Assets/Notebook/Scenes/NoteBookTesting.unity";

        [MenuItem("Peaceland/Notebook/Cleanup NoteBookTesting Scene", false, 1)]
        public static void CleanupMenu()
        {
            if (!EnsureTestingSceneActive())
            {
                return;
            }

            CleanupActiveTestingScene();
        }

        public static void CleanupActiveTestingScene()
        {
            int removedSprites = RemoveStrayWorldNotebookSprites();
            int removedLegacyButtons = RemoveLegacySpreadButtons();
            NotebookOpenUIAuthoring.CleanupLegacyTestUi();
            NotebookOpenUIAuthoring.AuthorInActiveScene();
            ApplyBaselineArtLayoutDefaults();
            EnsurePageClipMasks();
            NotebookOpenUIAuthoring.WireControllerToShell();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log(
                "Notebook test scene cleanup complete. "
                + "Removed " + removedSprites + " world sprite(s), "
                + removedLegacyButtons + " legacy spread button(s). "
                + "Tune NotebookBookArtLayout, then enable Lock Layout. "
                + "See NOTEBOOK_WORKFLOW.md");
        }

        private static bool EnsureTestingSceneActive()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path.Replace('\\', '/').EndsWith("NoteBookTesting.unity"))
            {
                return true;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            EditorSceneManager.OpenScene(TestingScenePath);
            return true;
        }

        private static int RemoveStrayWorldNotebookSprites()
        {
            int removed = 0;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null)
                {
                    continue;
                }

                if (!IsStrayWorldNotebookSprite(root))
                {
                    continue;
                }

                Object.DestroyImmediate(root);
                removed++;
            }

            return removed;
        }

        private static bool IsStrayWorldNotebookSprite(GameObject root)
        {
            if (root.GetComponentInChildren<Canvas>(true) != null)
            {
                return false;
            }

            SpriteRenderer spriteRenderer = root.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                return false;
            }

            string name = root.name.ToLowerInvariant();
            return name.Contains("notebookopened") || name.Contains("notebook_opened");
        }

        private static int RemoveLegacySpreadButtons()
        {
            Transform openRoot = GameObject.Find("Notebook Open Root")?.transform;
            if (openRoot == null)
            {
                return 0;
            }

            int removed = 0;
            removed += DestroyIfPresent(openRoot, "Previous Spread Button");
            removed += DestroyIfPresent(openRoot, "Next Spread Button");
            return removed;
        }

        private static int DestroyIfPresent(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                return 0;
            }

            Object.DestroyImmediate(child.gameObject);
            return 1;
        }

        private static void ApplyBaselineArtLayoutDefaults()
        {
            GameObject openRootObject = GameObject.Find("Notebook Open Root");
            if (openRootObject == null)
            {
                return;
            }

            NotebookBookArtLayout artLayout = openRootObject.GetComponent<NotebookBookArtLayout>();
            if (artLayout == null)
            {
                artLayout = openRootObject.AddComponent<NotebookBookArtLayout>();
            }

            SerializedObject artObject = new SerializedObject(artLayout);
            SerializedProperty pageHeight = artObject.FindProperty("pageUsableHeight");
            if (pageHeight != null && pageHeight.floatValue <= 0f)
            {
                pageHeight.floatValue = 320f;
            }

            SerializedProperty topReserved = artObject.FindProperty("pageTopReservedHeight");
            if (topReserved != null && topReserved.floatValue <= 0f)
            {
                topReserved.floatValue = 52f;
            }

            SerializedProperty bookmarkTop = artObject.FindProperty("bookmarkTopInset");
            if (bookmarkTop != null && bookmarkTop.floatValue <= 0f)
            {
                bookmarkTop.floatValue = NotebookBookShellLayout.BookmarkTopInset;
            }

            SerializedProperty bookmarkOffset = artObject.FindProperty("bookmarkHorizontalOffset");
            if (bookmarkOffset != null && bookmarkOffset.floatValue <= 0f)
            {
                bookmarkOffset.floatValue = NotebookBookShellLayout.BookmarkHorizontalOffset;
            }

            SerializedProperty lockLayout = artObject.FindProperty("lockLayout");
            if (lockLayout != null)
            {
                lockLayout.boolValue = false;
            }

            artObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(artLayout);
        }

        private static void EnsurePageClipMasks()
        {
            string[] pagePaths =
            {
                "Notebook Open Root/Content Root/Section Page Root/Entries Area/Index Root/Left Page",
                "Notebook Open Root/Content Root/Section Page Root/Entries Area/Index Root/Right Page",
                "Notebook Open Root/Content Root/Section Page Root/Entries Area/Content Root/Left Page",
                "Notebook Open Root/Content Root/Section Page Root/Entries Area/Content Root/Right Page",
            };

            for (int i = 0; i < pagePaths.Length; i++)
            {
                Transform page = GameObject.Find(pagePaths[i])?.transform;
                if (page == null)
                {
                    continue;
                }

                if (page.GetComponent<RectMask2D>() == null)
                {
                    page.gameObject.AddComponent<RectMask2D>();
                }
            }
        }
    }
}
#endif
