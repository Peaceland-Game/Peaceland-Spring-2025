using UnityEngine;

namespace Peaceland.Notebook
{
    public static class NotebookSceneLookup
    {
        public static NotebookUIShellReferences FindShell()
        {
            return Object.FindFirstObjectByType<NotebookUIShellReferences>(FindObjectsInactive.Include);
        }

        public static NotebookController FindController()
        {
            return Object.FindFirstObjectByType<NotebookController>(FindObjectsInactive.Include);
        }

        public static NotebookOpenButton FindOpenButton()
        {
            return Object.FindFirstObjectByType<NotebookOpenButton>(FindObjectsInactive.Include);
        }
    }
}
