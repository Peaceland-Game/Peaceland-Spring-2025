#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Peaceland.Notebook.Editor
{
    [CustomEditor(typeof(NotebookNotificationAnchor))]
    public sealed class NotebookNotificationAnchorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Screen-safe placement keeps the notification in the full-screen left-top corner. " +
                "Disable it only when you explicitly need to debug a hand-placed scene layout.",
                MessageType.Info);

            DrawDefaultInspector();

            if (GUILayout.Button("Reset To Screen-Safe Top-Left Preset"))
            {
                foreach (Object t in targets)
                {
                    if (t is NotebookNotificationAnchor anchor)
                    {
                        Undo.RecordObject(anchor, "Reset notification preset");
                        Undo.RecordObject(anchor.transform, "Reset notification preset");
                        anchor.ApplyPresetScreenSafeDefaults();
                        EditorUtility.SetDirty(anchor);
                    }
                }
            }
        }
    }
}
#endif
