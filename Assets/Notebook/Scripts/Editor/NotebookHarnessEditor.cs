#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Peaceland.Notebook.Editor
{
    public static class NotebookHarnessEditor
    {
        private const string CatalogPath = "Assets/Notebook/Resources/NotebookContentCatalog.asset";
        private const string CatalogLegacyPath = "Assets/Notebook/Harness/NotebookContentCatalog.asset";
        private const string DatabasePath = "Assets/Notebook/Data/NotebookDatabase.asset";
        private const string ReportDir = "Library/NotebookReports";
        private const string ReportMdPath = ReportDir + "/last_harness_report.md";
        private const string ReportJsonPath = ReportDir + "/last_harness_report.json";

        private static readonly string[] TestScenePaths =
        {
            "Assets/Notebook/Scenes/NoteBookTesting.unity",
            "Assets/Notebook/Scenes/NotebookTest_FloristMinigame.unity",
            "Assets/Notebook/Scenes/NotebookTest_FloristItemCollect.unity",
            "Assets/Notebook/Scenes/NotebookTest_RandJItemCollect.unity",
            "Assets/Notebook/Scenes/NotebookTest_IntroNewspaper.unity",
        };

        public static string[] GetTestScenePaths() => TestScenePaths;

        [MenuItem("Peaceland/Notebook/Harness/Create Or Refresh Content Catalog")]
        public static void CreateOrRefreshCatalogMenu()
        {
            NotebookContentCatalog catalog = EnsureCatalogAsset();
            PopulateDefaultSpecs(catalog);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Notebook content catalog ready at " + CatalogPath);
        }

        [MenuItem("Peaceland/Notebook/Harness/Validate Content")]
        public static void ValidateContentMenu()
        {
            NotebookHarnessReport report = RunValidation(scanScenes: false);
            LogReportSummary(report);
        }

        [MenuItem("Peaceland/Notebook/Harness/Validate Content + Collect Wiring")]
        public static void ValidateWithWiringMenu()
        {
            NotebookHarnessReport report = RunValidation(scanScenes: true);
            LogReportSummary(report);
        }

        [MenuItem("Peaceland/Notebook/Harness/Export Validation Report")]
        public static void ExportValidationReportMenu()
        {
            NotebookHarnessReport report = RunValidation(scanScenes: true);
            ExportReport(report);
            LogReportSummary(report);
            Debug.Log("Harness report exported to " + ReportMdPath);
        }

        [MenuItem("Peaceland/Notebook/Harness/Run Full Harness")]
        public static void RunFullHarnessMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            CreateOrRefreshCatalogMenu();
            NotebookContentAuthoring.SyncNotebookDatabaseMenu();
            NotebookContentAuthoring.AuthorAllNotebookTestScenesMenu();
            NotebookHarnessReport report = RunValidation(scanScenes: true);
            ExportReport(report);
            LogReportSummary(report);
        }

        /// <summary>Unity batchmode: -executeMethod Peaceland.Notebook.Editor.NotebookHarnessEditor.RunHarnessBatch</summary>
        public static void RunHarnessBatch()
        {
            CreateOrRefreshCatalogMenu();
            NotebookContentAuthoring.SyncNotebookDatabaseMenu();
            NotebookHarnessReport report = RunValidation(scanScenes: true);
            ExportReport(report);
            Debug.Log(report.ToMarkdown());
            if (report.HasErrors)
            {
                EditorApplication.Exit(1);
            }
        }

        public static NotebookContentCatalog EnsureCatalogAsset()
        {
            EnsureFolder("Assets/Notebook/Resources");
            NotebookContentCatalog catalog = AssetDatabase.LoadAssetAtPath<NotebookContentCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = AssetDatabase.LoadAssetAtPath<NotebookContentCatalog>(CatalogLegacyPath);
            }

            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<NotebookContentCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            return catalog;
        }

        public static NotebookHarnessReport RunValidation(bool scanScenes)
        {
            HashSet<string> wired = scanScenes ? ScanCollectTriggersInTestScenes() : null;

            // Scene authoring / domain reload can destroy cached ScriptableObject references.
            AssetDatabase.Refresh();
            NotebookContentCatalog catalog = EnsureCatalogAsset();
            if (catalog.Specs.Count == 0)
            {
                PopulateDefaultSpecs(catalog);
                EditorUtility.SetDirty(catalog);
            }

            NotebookDatabase database = AssetDatabase.LoadAssetAtPath<NotebookDatabase>(DatabasePath);
            NotebookHarnessReport report = NotebookHarnessValidator.Validate(database, catalog, wired);
            if (scanScenes)
            {
                NotebookScenePlayabilityEditor.ValidateAllTestScenes(report);
            }

            return report;
        }

        private static HashSet<string> ScanCollectTriggersInTestScenes()
        {
            HashSet<string> wired = new HashSet<string>();
            string originalScene = EditorSceneManager.GetActiveScene().path;

            for (int i = 0; i < TestScenePaths.Length; i++)
            {
                string scenePath = TestScenePaths[i];
                if (!File.Exists(scenePath))
                {
                    continue;
                }

                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                NotebookCollectTrigger[] triggers = Object.FindObjectsByType<NotebookCollectTrigger>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

                for (int t = 0; t < triggers.Length; t++)
                {
                    CollectEntryIdsFromTrigger(triggers[t], wired);
                }
            }

            if (!string.IsNullOrWhiteSpace(originalScene) && File.Exists(originalScene))
            {
                EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
            }

            return wired;
        }

        private static void CollectEntryIdsFromTrigger(NotebookCollectTrigger trigger, HashSet<string> wired)
        {
            if (trigger == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(trigger);
            SerializedProperty entriesProperty = serialized.FindProperty("entries");
            if (entriesProperty == null)
            {
                return;
            }

            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                NotebookEntryDefinition entry = entriesProperty.GetArrayElementAtIndex(i).objectReferenceValue
                    as NotebookEntryDefinition;
                if (entry != null && !string.IsNullOrWhiteSpace(entry.EntryId))
                {
                    wired.Add(entry.EntryId);
                }
            }
        }

        private static void ExportReport(NotebookHarnessReport report)
        {
            Directory.CreateDirectory(GetAbsolutePath(ReportDir));
            File.WriteAllText(GetAbsolutePath(ReportMdPath), report.ToMarkdown());
            File.WriteAllText(GetAbsolutePath(ReportJsonPath), ReportToJson(report));
        }

        private static string ReportToJson(NotebookHarnessReport report)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"passed\": " + (report.Passed ? "true" : "false") + ",");
            builder.AppendLine("  \"errorCount\": " + report.ErrorCount + ",");
            builder.AppendLine("  \"warningCount\": " + report.WarningCount + ",");
            builder.AppendLine("  \"generatedUtc\": \"" + report.generatedUtc + "\",");
            builder.AppendLine("  \"issues\": [");
            for (int i = 0; i < report.Issues.Count; i++)
            {
                NotebookHarnessIssue issue = report.Issues[i];
                builder.Append("    { \"severity\": \"").Append(issue.severity).Append("\", ");
                builder.Append("\"code\": \"").Append(EscapeJson(issue.code)).Append("\", ");
                builder.Append("\"entryId\": \"").Append(EscapeJson(issue.entryId ?? string.Empty)).Append("\", ");
                builder.Append("\"message\": \"").Append(EscapeJson(issue.message)).Append("\" }");
                builder.AppendLine(i < report.Issues.Count - 1 ? "," : string.Empty);
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string EscapeJson(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static void LogReportSummary(NotebookHarnessReport report)
        {
            if (report.Passed)
            {
                Debug.Log("Notebook harness PASSED (" + report.WarningCount + " warnings).");
                return;
            }

            Debug.LogError("Notebook harness FAILED: " + report.ErrorCount + " errors, " + report.WarningCount + " warnings.");
            for (int i = 0; i < report.Issues.Count; i++)
            {
                NotebookHarnessIssue issue = report.Issues[i];
                if (issue.severity == NotebookHarnessSeverity.Error)
                {
                    Debug.LogError("[" + issue.code + "] " + issue.message + (string.IsNullOrWhiteSpace(issue.entryId) ? string.Empty : " (" + issue.entryId + ")"));
                }
            }
        }

        private static void PopulateDefaultSpecs(NotebookContentCatalog catalog)
        {
            SerializedObject serialized = new SerializedObject(catalog);
            SerializedProperty specsProperty = serialized.FindProperty("specs");
            specsProperty.ClearArray();

            AddSpec(specsProperty, Spec(
                "NotebookEntry_PresentNewspaper",
                NotebookContentKind.Gameplay,
                NotebookSection.Present,
                "newspaper",
                requireCollect: true,
                scene: "NotebookTest_IntroNewspaper",
                notes: "Present intro newspaper click"));

            AddSpec(specsProperty, Spec(
                "NotebookEntry_Memory1FloristNormal",
                NotebookContentKind.Gameplay,
                NotebookSection.Memory1,
                "scene-collected",
                requireCollect: false,
                scene: null,
                notes: "Florist greeting — optional test scene hook"));

            AddSpec(specsProperty, Spec(
                "NotebookEntry_Memory1FloristFlower",
                NotebookContentKind.Gameplay,
                NotebookSection.Memory1,
                "minigame",
                requireCollect: true,
                scene: "NotebookTest_FloristMinigame",
                notes: "Florist minigame complete button"));

            for (int i = 1; i <= 5; i++)
            {
                AddSpec(specsProperty, Spec(
                    "NotebookEntry_FloristFlower_" + i.ToString("00"),
                    NotebookContentKind.Gameplay,
                    NotebookSection.Memory1,
                    "scene-collected",
                    requireCollect: true,
                    scene: "NotebookTest_FloristItemCollect",
                    notes: "Florist world flower " + i));
            }

            AddSpec(specsProperty, Spec(
                "NotebookEntry_Memory2RJBalcony",
                NotebookContentKind.Gameplay,
                NotebookSection.Memory2,
                "scene-collected",
                requireCollect: true,
                scene: "NotebookTest_RandJItemCollect",
                notes: "R&J balcony fragment"));

            AddSpec(specsProperty, Spec(
                "NotebookEntry_Memory2RJLetter",
                NotebookContentKind.Gameplay,
                NotebookSection.Memory2,
                "scene-collected",
                requireCollect: true,
                scene: "NotebookTest_RandJItemCollect",
                notes: "R&J letter draft"));

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static NotebookContentSpec Spec(
            string entryId,
            NotebookContentKind kind,
            NotebookSection section,
            string categoryId,
            bool requireCollect,
            string scene,
            string notes)
        {
            return new NotebookContentSpec
            {
                entryId = entryId,
                kind = kind,
                section = section,
                categoryId = categoryId,
                requireBodyText = true,
                requireCollectTrigger = requireCollect,
                collectSceneName = scene,
                notes = notes,
            };
        }

        private static void AddSpec(SerializedProperty specsProperty, NotebookContentSpec spec)
        {
            int index = specsProperty.arraySize;
            specsProperty.InsertArrayElementAtIndex(index);
            SerializedProperty element = specsProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("entryId").stringValue = spec.entryId;
            element.FindPropertyRelative("kind").enumValueIndex = (int)spec.kind;
            element.FindPropertyRelative("section").enumValueIndex = (int)spec.section;
            element.FindPropertyRelative("categoryId").stringValue = spec.categoryId;
            element.FindPropertyRelative("requireBodyText").boolValue = spec.requireBodyText;
            element.FindPropertyRelative("requireCollectTrigger").boolValue = spec.requireCollectTrigger;
            element.FindPropertyRelative("collectSceneName").stringValue = spec.collectSceneName ?? string.Empty;
            element.FindPropertyRelative("notes").stringValue = spec.notes ?? string.Empty;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (!string.IsNullOrWhiteSpace(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static string GetAbsolutePath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }
    }
}
#endif
