#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Peaceland.Notebook.Editor;

namespace Peaceland.Notebook.EditableScenePack.Editor
{
    public static class NotebookEditableScenePackEditor
    {
        private const string PackRoot = "Assets/Notebook/EditableScenePack";
        private const string ReportsFolder = PackRoot + "/Reports";
        private const string LatestReportPath = ReportsFolder + "/scene_self_check_latest.md";

        private static readonly string[] AllTestScenes =
        {
            "Assets/Notebook/Scenes/NoteBookTesting.unity",
            "Assets/Notebook/Scenes/NotebookTest_FloristMinigame.unity",
            "Assets/Notebook/Scenes/NotebookTest_FloristItemCollect.unity",
            "Assets/Notebook/Scenes/NotebookTest_RandJItemCollect.unity",
            "Assets/Notebook/Scenes/NotebookTest_IntroNewspaper.unity",
        };

        [MenuItem("Peaceland/Notebook/Editable Pack/Install On Active Scene")]
        public static void InstallActiveScene()
        {
            InstallOnActiveScene();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[EditableScenePack] Installed on active scene.");
        }

        [MenuItem("Peaceland/Notebook/Editable Pack/Tag Hand-Placed UI On Active Scene")]
        public static void TagHandPlacedUiOnActiveScene()
        {
            int tagged = TagHandPlacedUiOnActiveSceneInternal();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[EditableScenePack] Tagged " + tagged + " hand-placed notebook UI rect(s) on active scene.");
        }

        [MenuItem("Peaceland/Notebook/Editable Pack/Relayout Bookmark Tabs On Active Scene")]
        public static void RelayoutBookmarkTabsOnActiveScene()
        {
            NotebookBookmarkTabBar tabBar = Object.FindFirstObjectByType<NotebookBookmarkTabBar>(FindObjectsInactive.Include);
            if (tabBar == null)
            {
                Debug.LogWarning("[EditableScenePack] NotebookBookmarkTabBar not found on active scene.");
                return;
            }

            tabBar.RefreshAndWire(NotebookSection.Directory, true);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[EditableScenePack] Relayout bookmark tabs for main directory view.");
        }

        [MenuItem("Peaceland/Notebook/Editable Pack/Tag Hand-Placed UI On All Test Scenes")]
        public static void TagHandPlacedUiOnAllTestScenes()
        {
            EditorSceneManager.SaveOpenScenes();

            string original = EditorSceneManager.GetActiveScene().path;
            int tagged = 0;
            for (int i = 0; i < AllTestScenes.Length; i++)
            {
                EditorSceneManager.OpenScene(AllTestScenes[i], OpenSceneMode.Single);
                tagged += TagHandPlacedUiOnActiveSceneInternal();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }

            if (!string.IsNullOrWhiteSpace(original) && File.Exists(original))
            {
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            }

            Debug.Log("[EditableScenePack] Tagged " + tagged + " hand-placed notebook UI rect(s) across all notebook test scenes.");
        }

        [MenuItem("Peaceland/Notebook/Editable Pack/Apply To All Notebook Test Scenes")]
        public static void ApplyAllScenes()
        {
            EditorSceneManager.SaveOpenScenes();

            string original = EditorSceneManager.GetActiveScene().path;
            for (int i = 0; i < AllTestScenes.Length; i++)
            {
                EditorSceneManager.OpenScene(AllTestScenes[i], OpenSceneMode.Single);
                InstallOnActiveScene();
                NotebookScenePlayabilityEditor.EnsurePlayableActiveScene();
                EditorSceneManager.SaveOpenScenes();
            }

            if (!string.IsNullOrWhiteSpace(original) && File.Exists(original))
            {
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            }

            Debug.Log("[EditableScenePack] Applied to all notebook test scenes.");
        }

        [MenuItem("Peaceland/Notebook/Editable Pack/Run 5-Round Self Check (Active Scene)")]
        public static void RunFiveRoundActive()
        {
            InstallOnActiveScene();
            EditorSceneManager.SaveOpenScenes();
            string path = EditorSceneManager.GetActiveScene().path;
            string name = EditorSceneManager.GetActiveScene().name;
            List<NotebookHarnessReport> rounds = RunFiveRounds(path, name);
            WriteReport(rounds, "Scene Self Check — " + name);
            LogSummary(rounds);
        }

        [MenuItem("Peaceland/Notebook/Editable Pack/Run 5-Round Self Check (All Test Scenes)")]
        public static void RunFiveRoundAll()
        {
            EditorSceneManager.SaveOpenScenes();

            string original = EditorSceneManager.GetActiveScene().path;
            List<NotebookHarnessReport> allRounds = new List<NotebookHarnessReport>();

            for (int s = 0; s < AllTestScenes.Length; s++)
            {
                EditorSceneManager.OpenScene(AllTestScenes[s], OpenSceneMode.Single);
                InstallOnActiveScene();
                EditorSceneManager.SaveOpenScenes();
                string path = AllTestScenes[s];
                string name = Path.GetFileNameWithoutExtension(path);
                allRounds.AddRange(RunFiveRounds(path, name));
            }

            if (!string.IsNullOrWhiteSpace(original) && File.Exists(original))
            {
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            }

            WriteReport(allRounds, "Scene Self Check — All Notebook Test Scenes (5 rounds each)");
            LogSummary(allRounds);
        }

        private static List<NotebookHarnessReport> RunFiveRounds(string scenePath, string sceneName)
        {
            List<NotebookHarnessReport> rounds = new List<NotebookHarnessReport>(5);
            for (int round = 1; round <= 5; round++)
            {
                rounds.Add(NotebookSceneSpecAudit.AuditScene(scenePath, sceneName, round));
            }

            return rounds;
        }

        private static void InstallOnActiveScene()
        {
            string path = EditorSceneManager.GetActiveScene().path.Replace('\\', '/');
            bool isHome = path.EndsWith("NoteBookTesting.unity");

            if (isHome)
            {
                NotebookOpenUIAuthoring.AuthorInActiveScene();
                TagHandPlacedUiOnActiveSceneInternal();
            }

            // Overlay is shared infrastructure: author it in every scene so the
            // hierarchy remains inspectable and collect feedback cannot drift.
            NotebookContentAuthoring.EnsureCollectOverlayMenu();

            EnsureAuthoringProfile();
            EnsureInteractMarkers(path);
            RefreshProfileInteractList();
        }

        private static void EnsureAuthoringProfile()
        {
            NotebookController controller = Object.FindFirstObjectByType<NotebookController>();
            GameObject host = controller != null ? controller.gameObject : FindSceneObjectByName("Notebook Test Bootstrap");
            if (host == null)
            {
                host = new GameObject("Notebook Test Bootstrap");
            }

            NotebookSceneAuthoringProfile profile = host.GetComponent<NotebookSceneAuthoringProfile>();
            if (profile == null)
            {
                profile = host.AddComponent<NotebookSceneAuthoringProfile>();
            }

            SerializedObject so = new SerializedObject(profile);
            so.FindProperty("disableRuntimeBootstrapRebuild").boolValue = true;
            so.FindProperty("collectDummyEntriesOnPlay").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureInteractMarkers(string scenePath)
        {
            string normalized = scenePath.Replace('\\', '/');
            bool isHome = normalized.EndsWith("NoteBookTesting.unity");
            bool isCollect = normalized.Contains("NotebookTest_") && !isHome;

            if (isHome)
            {
                TagByName("Notebook HUD Button", NotebookSceneInteractKind.HudOpenNotebook, "Open notebook");
                TagDirectoryLine("Present Line", NotebookSection.Present);
                TagDirectoryLine("Memory 1 Line", NotebookSection.Memory1);
                TagDirectoryLine("Memory 2 Line", NotebookSection.Memory2);
                if (NotebookFeatureFlags.IncludeHiddenStatsSection)
                {
                    TagDirectoryLine("Hidden Stats Line", NotebookSection.HiddenStats);
                }

                TagByName("Previous Page Hit", NotebookSceneInteractKind.PageTurnPrevious, "Previous spread");
                TagByName("Next Page Hit", NotebookSceneInteractKind.PageTurnNext, "Next spread");
                TagByName("Directory Tab", NotebookSceneInteractKind.BookmarkTab, "Directory tab", NotebookSection.Directory);
                TagByName("Present Tab", NotebookSceneInteractKind.BookmarkTab, "Present tab", NotebookSection.Present);
                TagByName("Memory 1 Tab", NotebookSceneInteractKind.BookmarkTab, "Memory 1 tab", NotebookSection.Memory1);
                TagByName("Memory 2 Tab", NotebookSceneInteractKind.BookmarkTab, "Memory 2 tab", NotebookSection.Memory2);
                if (NotebookFeatureFlags.IncludeHiddenStatsSection)
                {
                    TagByName("Hidden Stats Tab", NotebookSceneInteractKind.BookmarkTab, "Stats tab", NotebookSection.HiddenStats);
                }
            }

            if (isCollect)
            {
                NotebookCollectTrigger[] triggers = Object.FindObjectsByType<NotebookCollectTrigger>(FindObjectsSortMode.None);
                for (int i = 0; i < triggers.Length; i++)
                {
                    NotebookCollectTrigger trigger = triggers[i];
                    if (trigger == null)
                    {
                        continue;
                    }

                    NotebookSceneInteractMarker marker = GetOrAddMarker(trigger.gameObject);
                    marker.Configure(NotebookSceneInteractKind.CollectTrigger, trigger.gameObject.name);
                }

                TagByName("Return Home", NotebookSceneInteractKind.ReturnHome, "Return to home scene");
                Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
                for (int i = 0; i < buttons.Length; i++)
                {
                    Button button = buttons[i];
                    if (button == null)
                    {
                        continue;
                    }

                    string lower = button.gameObject.name.ToLowerInvariant();
                    if (lower.Contains("return") || lower.Contains("home"))
                    {
                        Tag(button.gameObject, NotebookSceneInteractKind.ReturnHome, button.gameObject.name);
                    }
                }
            }

            if (normalized.Contains("FloristMinigame"))
            {
                Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
                for (int i = 0; i < buttons.Length; i++)
                {
                    Button button = buttons[i];
                    if (button == null)
                    {
                        continue;
                    }

                if (button.gameObject.name.IndexOf("Complete", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Tag(button.gameObject, NotebookSceneInteractKind.MinigameComplete, "Complete minigame");
                    Tag(button.gameObject, NotebookSceneInteractKind.CollectTrigger, "Collect reward from minigame");
                }
            }
        }
        }

        private static void TagDirectoryLine(string objectName, NotebookSection section)
        {
            GameObject target = FindSceneObjectByName(objectName);
            if (target == null)
            {
                return;
            }

            Tag(target, NotebookSceneInteractKind.DirectorySection, section.ToString(), section);
        }

        private static void TagByName(string objectName, NotebookSceneInteractKind kind, string label, NotebookSection section = NotebookSection.Directory)
        {
            GameObject target = FindSceneObjectByName(objectName);
            if (target == null)
            {
                return;
            }

            Tag(target, kind, label, section);
        }

        private static void Tag(GameObject target, NotebookSceneInteractKind kind, string label, NotebookSection section = NotebookSection.Directory)
        {
            if (target == null)
            {
                return;
            }

            NotebookSceneInteractMarker marker = GetOrAddMarker(target);
            marker.Configure(kind, label, section);
            EditorUtility.SetDirty(marker);
        }

        private static NotebookSceneInteractMarker GetOrAddMarker(GameObject target)
        {
            NotebookSceneInteractMarker marker = target.GetComponent<NotebookSceneInteractMarker>();
            if (marker == null)
            {
                marker = target.AddComponent<NotebookSceneInteractMarker>();
            }

            return marker;
        }

        private static int TagHandPlacedUiOnActiveSceneInternal()
        {
            int tagged = 0;
            tagged += TagHandPlacedUiByName("Notebook HUD Button", NotebookUILayoutRole.HudOpenButton);
            tagged += TagHandPlacedUiByName("Collectible Hint", NotebookUILayoutRole.CollectibleHint);
            tagged += TagHandPlacedUiByName("Collected Toast", NotebookUILayoutRole.CollectedToast);
            tagged += TagHandPlacedUiByName("Notebook Open Root", NotebookUILayoutRole.BookOpenRoot);
            tagged += TagHandPlacedUiByName("Book Background", NotebookUILayoutRole.BookBackground);
            return tagged;
        }

        private static int TagHandPlacedUiByName(string objectName, NotebookUILayoutRole role)
        {
            GameObject target = FindSceneObjectByName(objectName);
            if (target == null)
            {
                Debug.LogWarning("[EditableScenePack] Hand-placed UI target not found: " + objectName);
                return 0;
            }

            NotebookHandPlacedRect handPlaced = target.GetComponent<NotebookHandPlacedRect>();
            if (handPlaced == null)
            {
                handPlaced = target.AddComponent<NotebookHandPlacedRect>();
            }

            handPlaced.SetRole(role, "Tagged by Editable Pack menu.");
            EditorUtility.SetDirty(handPlaced);
            return 1;
        }

        private static void RefreshProfileInteractList()
        {
            NotebookSceneAuthoringProfile profile = NotebookSceneAuthoringProfile.FindInScene();
            if (profile == null)
            {
                return;
            }

            NotebookSceneInteractMarker[] markers = Object.FindObjectsByType<NotebookSceneInteractMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            profile.SetInteracts(new List<NotebookSceneInteractMarker>(markers));
            EditorUtility.SetDirty(profile);
        }

        private static GameObject FindSceneObjectByName(string objectName)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject candidate = allObjects[i];
                if (candidate == null
                    || candidate.hideFlags != HideFlags.None
                    || !candidate.scene.IsValid()
                    || candidate.name != objectName)
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private static void WriteReport(List<NotebookHarnessReport> rounds, string title)
        {
            if (!AssetDatabase.IsValidFolder(PackRoot))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(ReportsFolder))
            {
                AssetDatabase.CreateFolder(PackRoot, "Reports");
            }

            string markdown = NotebookSceneSpecAudit.MergeReportsToMarkdown(rounds, title);
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), LatestReportPath), markdown);
            AssetDatabase.Refresh();
            Debug.Log("[EditableScenePack] Wrote " + LatestReportPath);
        }

        private static void LogSummary(List<NotebookHarnessReport> rounds)
        {
            int errors = 0;
            for (int i = 0; i < rounds.Count; i++)
            {
                errors += rounds[i].ErrorCount;
            }

            if (errors == 0)
            {
                Debug.Log("[EditableScenePack] 5-round self-check PASS. See " + LatestReportPath);
            }
            else
            {
                Debug.LogError("[EditableScenePack] Self-check FAIL with " + errors + " error(s). See " + LatestReportPath);
            }
        }
    }
}
#endif
