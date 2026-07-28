#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Peaceland.Editor
{
    public static class PeacelandSaveEditor
    {
        private const string ExportDir = "Assets/Peaceland/Saves";

        [MenuItem("Peaceland/Save/Reset Active Slot Progress")]
        public static void ResetActiveSlotMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "Reset Active Slot",
                    "Clear notebook, stats, and progress for the active save slot?",
                    "Reset",
                    "Cancel"))
            {
                return;
            }

            if (Application.isPlaying && PeacelandSaveService.HasInstance)
            {
                PeacelandSaveService.Instance.ResetAll();
            }
            else
            {
                Debug.LogWarning("Enter Play mode with an active slot selected to reset runtime save.");
            }
        }

        [MenuItem("Peaceland/Save/Ensure Game Bootstrap In Scene")]
        public static void EnsureBootstrapMenu()
        {
            if (Application.isPlaying)
            {
                PeacelandGameBootstrap.EnsureExists();
                EditorUtility.DisplayDialog(
                    "Peaceland Bootstrap",
                    "Peaceland Game Bootstrap active in Play mode (DontDestroyOnLoad).",
                    "OK");
                return;
            }

            PeacelandGameBootstrap existing = Object.FindFirstObjectByType<PeacelandGameBootstrap>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog(
                    "Peaceland Bootstrap",
                    "Peaceland Game Bootstrap is already in this scene.\n\n"
                    + "In Play mode it auto-creates if missing — you do not need this object for Edit-mode layout work.",
                    "OK");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            GameObject host = new GameObject("Peaceland Game Bootstrap");
            Undo.RegisterCreatedObjectUndo(host, "Add Peaceland Bootstrap");
            host.AddComponent<PeacelandGameBootstrap>();
            EditorSceneManager.MarkSceneDirty(host.scene);
            Selection.activeGameObject = host;
            EditorUtility.DisplayDialog(
                "Peaceland Bootstrap",
                "Added scene placeholder. Stats/save still auto-bootstrap on Play.\n\n"
                + "You can move or delete this object in Edit mode — it will not fight your layout.",
                "OK");
        }

        [MenuItem("Peaceland/Save/Export Default Save To Project")]
        public static void ExportDefaultSaveMenu()
        {
            EnsureExportDir();
            string exportPath = Path.GetFullPath(Path.Combine(ExportDir, "peaceland_save_export.json"));
            if (Application.isPlaying && PeacelandSaveService.HasInstance)
            {
                PeacelandSaveService.Instance.SaveToPath(exportPath);
            }
            else
            {
                string defaultPath = Path.Combine(Application.persistentDataPath, PeacelandSaveService.LegacyDefaultFileName);
                if (File.Exists(defaultPath))
                {
                    File.Copy(defaultPath, exportPath, true);
                }
                else
                {
                    File.WriteAllText(exportPath, JsonUtility.ToJson(new PeacelandGameSaveData(), true));
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Exported save to " + exportPath);
        }

        [MenuItem("Peaceland/Save/Import Save Into Game")]
        public static void ImportSaveMenu()
        {
            string path = EditorUtility.OpenFilePanel("Import Peaceland Save", ExportDir, "json");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (Application.isPlaying && PeacelandSaveService.HasInstance)
            {
                PeacelandSaveService.Instance.LoadFromPath(path);
                Debug.Log("Imported save into running game from " + path);
                return;
            }

            string defaultPath = Path.Combine(Application.persistentDataPath, PeacelandSaveService.LegacyDefaultFileName);
            File.Copy(path, defaultPath, true);
            Debug.Log("Copied save to default path: " + defaultPath);
        }

        [MenuItem("Peaceland/Save/Reset All Progress")]
        public static void ResetAllMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "Reset Peaceland Save",
                    "Clear notebook, stats, and progress from the default save file?",
                    "Reset",
                    "Cancel"))
            {
                return;
            }

            if (Application.isPlaying && PeacelandSaveService.HasInstance)
            {
                PeacelandSaveService.Instance.ResetAll();
            }
            else
            {
                string defaultPath = Path.Combine(Application.persistentDataPath, PeacelandSaveService.LegacyDefaultFileName);
                File.WriteAllText(defaultPath, JsonUtility.ToJson(new PeacelandGameSaveData(), true));
            }

            PlayerPrefs.DeleteKey("peaceland.notebook.state");
            PlayerPrefs.Save();
            Debug.Log("Peaceland save reset.");
        }

        private static void EnsureExportDir()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Peaceland"))
            {
                AssetDatabase.CreateFolder("Assets", "Peaceland");
            }

            if (!AssetDatabase.IsValidFolder(ExportDir))
            {
                AssetDatabase.CreateFolder("Assets/Peaceland", "Saves");
            }
        }
    }
}
#endif
