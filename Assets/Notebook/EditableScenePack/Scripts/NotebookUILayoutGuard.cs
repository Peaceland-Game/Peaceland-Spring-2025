using UnityEngine;

namespace Peaceland.Notebook.EditableScenePack
{
    public static class NotebookUILayoutGuard
    {
        public static bool ShouldSkipLayoutApply(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return false;
            }

            if (rectTransform.TryGetComponent<NotebookHandPlacedRect>(out NotebookHandPlacedRect handPlaced)
                && handPlaced.RespectSceneLayout)
            {
                return true;
            }

            // Bookmark tabs reparent between rails at runtime; they must always stack by index.
            if (rectTransform.GetComponent<NotebookBookmarkTabView>() != null)
            {
                return false;
            }

            Transform openRoot = FindNotebookOpenRoot(rectTransform);
            if (openRoot != null && NotebookBookArtLayout.IsLocked(openRoot))
            {
                return IsUnderOpenShell(rectTransform, openRoot);
            }

            return false;
        }

        private static Transform FindNotebookOpenRoot(Transform start)
        {
            Transform current = start;
            while (current != null)
            {
                if (current.name == "Notebook Open Root" || current.GetComponent<NotebookBookArtLayout>() != null)
                {
                    return current;
                }

                current = current.parent;
            }

            return null;
        }

        private static bool IsUnderOpenShell(Transform target, Transform openRoot)
        {
            return target == openRoot || target.IsChildOf(openRoot);
        }
    }
}
