using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Peaceland.Notebook.EditableScenePack
{
    /// <summary>
    /// Queryable scene audit vs NOTEBOOK_IMPLEMENTATION_PLAN + NOTEBOOK_UI_CONTRACT + human playable spec.
    /// </summary>
    public static class NotebookSceneSpecAudit
    {
        private const string HomeSceneSuffix = "NoteBookTesting.unity";

        public static NotebookHarnessReport AuditScene(string scenePath, string sceneName, int round)
        {
            NotebookHarnessReport report = new NotebookHarnessReport
            {
                generatedUtc = DateTime.UtcNow.ToString("o"),
            };

            report.Add(NotebookHarnessSeverity.Info, "AUDIT_ROUND", "Self-check round " + round + ".", sceneName);
            report.Add(NotebookHarnessSeverity.Info, "AUDIT_SCENE", scenePath ?? sceneName, sceneName);

            bool isHome = IsHomeScene(scenePath, sceneName);
            bool isCollect = IsCollectScene(scenePath, sceneName);

            AuditInfrastructure(report, sceneName);
            AuditAuthoringProfile(report, sceneName);
            AuditInteractMarkers(report, sceneName, isHome, isCollect);
            AuditOverlay(report, sceneName);
            AuditNotebookUi(report, sceneName, isHome);
            AuditCollectWiring(report, sceneName, isCollect);

            if (report.Issues.Count(issue => issue.severity != NotebookHarnessSeverity.Info) == 0)
            {
                report.Add(NotebookHarnessSeverity.Info, "ROUND_PASS", "Round " + round + " passed.", sceneName);
            }

            return report;
        }

        public static string MergeReportsToMarkdown(IReadOnlyList<NotebookHarnessReport> rounds, string title)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# " + title);
            builder.AppendLine();
            builder.AppendLine("- Generated (UTC): `" + DateTime.UtcNow.ToString("o") + "`");
            builder.AppendLine("- Rounds: " + rounds.Count);
            builder.AppendLine();

            int totalErrors = 0;
            for (int i = 0; i < rounds.Count; i++)
            {
                NotebookHarnessReport round = rounds[i];
                totalErrors += round.ErrorCount;
                builder.AppendLine("## Round " + (i + 1));
                builder.AppendLine();
                builder.AppendLine("- Errors: " + round.ErrorCount + " | Warnings: " + round.WarningCount);
                builder.AppendLine("- Pass: **" + (round.Passed ? "YES" : "NO") + "**");
                builder.AppendLine();
                builder.Append(round.ToMarkdown());
                builder.AppendLine();
            }

            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("- **Overall:** " + (totalErrors == 0 ? "PASS (5/5 rounds clean)" : "FAIL (" + totalErrors + " total errors)"));
            builder.AppendLine("- **Human next:** Play scene, edit markers/layout in Hierarchy, re-run 5-round check.");
            return builder.ToString();
        }

        private static void AuditInfrastructure(NotebookHarnessReport report, string sceneName)
        {
            Camera camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "SCENE_NO_CAMERA", "Missing Main Camera.", sceneName);
            }
            else if (camera.GetComponent<AudioListener>() == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "SCENE_NO_AUDIO_LISTENER", "Main Camera missing AudioListener.", sceneName);
            }

            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "SCENE_NO_EVENTSYSTEM", "Missing EventSystem.", sceneName);
            }

            if (GameObject.Find("Playtest Background") == null)
            {
                report.Add(NotebookHarnessSeverity.Warning, "SCENE_NO_BACKGROUND", "Missing Playtest Background (human playable).", sceneName);
            }
        }

        private static void AuditAuthoringProfile(NotebookHarnessReport report, string sceneName)
        {
            NotebookSceneAuthoringProfile profile = UnityEngine.Object.FindFirstObjectByType<NotebookSceneAuthoringProfile>(FindObjectsInactive.Include);
            if (profile == null)
            {
                report.Add(
                    NotebookHarnessSeverity.Error,
                    "NO_AUTHORING_PROFILE",
                    "Missing NotebookSceneAuthoringProfile — Play may rebuild UI via bootstrap.",
                    sceneName);
                return;
            }

            if (!profile.DisableRuntimeBootstrapRebuild)
            {
                report.Add(
                    NotebookHarnessSeverity.Warning,
                    "BOOTSTRAP_REBUILD_ON",
                    "disableRuntimeBootstrapRebuild is false; Play may overwrite Scene edits.",
                    sceneName);
            }
        }

        private static void AuditInteractMarkers(NotebookHarnessReport report, string sceneName, bool isHome, bool isCollect)
        {
            NotebookSceneInteractMarker[] markers = UnityEngine.Object.FindObjectsByType<NotebookSceneInteractMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (markers.Length == 0)
            {
                report.Add(
                    NotebookHarnessSeverity.Error,
                    "NO_INTERACT_MARKERS",
                    "No NotebookSceneInteractMarker in scene — interactables not documented for edit.",
                    sceneName);
                return;
            }

            report.Add(NotebookHarnessSeverity.Info, "INTERACT_COUNT", "Found " + markers.Length + " interact marker(s).", sceneName);

            if (isHome)
            {
                RequireKind(report, sceneName, markers, NotebookSceneInteractKind.HudOpenNotebook, "Notebook HUD Button");
                RequireSectionLine(report, sceneName, markers, NotebookSection.Present);
                RequireSectionLine(report, sceneName, markers, NotebookSection.Memory1);
                RequireSectionLine(report, sceneName, markers, NotebookSection.Memory2);
                if (NotebookFeatureFlags.IncludeHiddenStatsSection)
                {
                    RequireSectionLine(report, sceneName, markers, NotebookSection.HiddenStats);
                }
            }

            if (isCollect)
            {
                bool hasCollect = markers.Any(marker => marker.Kind == NotebookSceneInteractKind.CollectTrigger);
                if (!hasCollect)
                {
                    report.Add(NotebookHarnessSeverity.Error, "COLLECT_MARKER_MISSING", "Collect scene needs CollectTrigger marker.", sceneName);
                }

                bool hasReturn = markers.Any(marker => marker.Kind == NotebookSceneInteractKind.ReturnHome);
                if (!hasReturn)
                {
                    report.Add(NotebookHarnessSeverity.Warning, "RETURN_HOME_MISSING", "Collect scene should have ReturnHome marker.", sceneName);
                }
            }
        }

        private static void AuditNotebookUi(NotebookHarnessReport report, string sceneName, bool isHome)
        {
            if (!isHome)
            {
                return;
            }

            if (UnityEngine.Object.FindFirstObjectByType<NotebookController>(FindObjectsInactive.Include) == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "NO_NOTEBOOK_CONTROLLER", "Home scene missing NotebookController.", sceneName);
            }

            string[] requiredObjects =
            {
                "Notebook Open Root",
                "Book Background",
                "Directory Page Root",
                "Directory Lines",
                "Notebook Canvas",
            };

            for (int i = 0; i < requiredObjects.Length; i++)
            {
                if (FindSceneObjectByName(requiredObjects[i]) == null)
                {
                    report.Add(NotebookHarnessSeverity.Error, "UI_MISSING", "Missing '" + requiredObjects[i] + "'.", sceneName);
                }
            }

            NotebookBookArtLayout artLayout = UnityEngine.Object.FindFirstObjectByType<NotebookBookArtLayout>(FindObjectsInactive.Include);
            if (artLayout == null)
            {
                report.Add(NotebookHarnessSeverity.Warning, "NO_ART_LAYOUT", "Missing NotebookBookArtLayout on Open Root.", sceneName);
            }
            else if (!artLayout.LockLayout)
            {
                report.Add(NotebookHarnessSeverity.Warning, "LAYOUT_NOT_LOCKED", "NotebookBookArtLayout.lockLayout is false.", sceneName);
            }

            NotebookDirectoryLineView[] lines = UnityEngine.Object.FindObjectsByType<NotebookDirectoryLineView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            HashSet<NotebookSection> sections = new HashSet<NotebookSection>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] != null)
                {
                    sections.Add(lines[i].Section);
                }
            }

            if (!sections.Contains(NotebookSection.Present))
            {
                report.Add(NotebookHarnessSeverity.Error, "DIRLINE_PRESENT", "Missing directory line for Present.", sceneName);
            }

            if (!sections.Contains(NotebookSection.Memory1))
            {
                report.Add(NotebookHarnessSeverity.Error, "DIRLINE_M1", "Missing directory line for Memory 1.", sceneName);
            }

            if (!sections.Contains(NotebookSection.Memory2))
            {
                report.Add(NotebookHarnessSeverity.Error, "DIRLINE_M2", "Missing directory line for Memory 2.", sceneName);
            }

            if (NotebookFeatureFlags.IncludeHiddenStatsSection && !sections.Contains(NotebookSection.HiddenStats))
            {
                report.Add(NotebookHarnessSeverity.Warning, "DIRLINE_STATS", "Missing directory line for Hidden Stats (dev).", sceneName);
            }

            RequireHandPlacedRect(report, sceneName, "Notebook HUD Button", NotebookUILayoutRole.HudOpenButton);
            RequireHandPlacedRect(report, sceneName, "Notebook Open Root", NotebookUILayoutRole.BookOpenRoot);
            RequireToastAnchor(report, sceneName);
        }

        private static void AuditOverlay(NotebookHarnessReport report, string sceneName)
        {
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "OVERLAY_NO_CANVAS", "Notebook overlay needs a Canvas.", sceneName);
                return;
            }

            NotebookOverlayView overlay = UnityEngine.Object.FindFirstObjectByType<NotebookOverlayView>(FindObjectsInactive.Include);
            if (overlay == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "OVERLAY_VIEW_MISSING", "Every notebook scene needs NotebookOverlayView.", sceneName);
                return;
            }

            Transform overlayRoot = overlay.transform;
            if (overlayRoot.name != "Notebook Overlay" || overlayRoot.parent != canvas.transform)
            {
                report.Add(
                    NotebookHarnessSeverity.Error,
                    "OVERLAY_ROOT_MISMATCH",
                    "NotebookOverlayView must be the direct child named 'Notebook Overlay' under the scene Canvas.",
                    sceneName);
            }

            RequireOverlayChild(report, sceneName, overlayRoot, "Collectible Hint");
            Transform toast = RequireOverlayChild(report, sceneName, overlayRoot, "Collected Toast");
            if (toast == null)
            {
                return;
            }

            NotebookNotificationAnchor anchor = toast.GetComponent<NotebookNotificationAnchor>();
            if (anchor == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "OVERLAY_ANCHOR_MISSING", "Collected Toast needs NotebookNotificationAnchor.", sceneName);
            }
            else if (!anchor.UseScreenSafePosition || anchor.UseHandPlacedRestPosition)
            {
                report.Add(
                    NotebookHarnessSeverity.Error,
                    "OVERLAY_NOT_SCREEN_SAFE",
                    "Collected Toast must use shared screen-safe placement; hand-placed mode is disabled for cross-scene consistency.",
                    sceneName);
            }
        }

        private static Transform RequireOverlayChild(
            NotebookHarnessReport report,
            string sceneName,
            Transform overlayRoot,
            string childName)
        {
            Transform child = overlayRoot.Find(childName);
            if (child == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "OVERLAY_CHILD_MISSING", "Missing overlay child '" + childName + "'.", sceneName);
            }

            return child;
        }

        private static void AuditCollectWiring(NotebookHarnessReport report, string sceneName, bool isCollect)
        {
            if (!isCollect)
            {
                return;
            }

            NotebookCollectTrigger[] triggers = UnityEngine.Object.FindObjectsByType<NotebookCollectTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (triggers.Length == 0)
            {
                report.Add(NotebookHarnessSeverity.Error, "NO_COLLECT_TRIGGER", "No NotebookCollectTrigger in collect scene.", sceneName);
                return;
            }

            bool hasWorldCollectTrigger = false;
            for (int i = 0; i < triggers.Length; i++)
            {
                NotebookCollectTrigger trigger = triggers[i];
                if (trigger == null)
                {
                    continue;
                }

                if (IsUiCollectTrigger(trigger))
                {
                    continue;
                }

                hasWorldCollectTrigger = true;
                if (trigger.GetComponent<Collider>() == null && trigger.GetComponent<Collider2D>() == null)
                {
                    report.Add(NotebookHarnessSeverity.Warning, "COLLECT_NO_COLLIDER", "Collect trigger lacks collider: " + trigger.gameObject.name, sceneName);
                }
            }

            Camera camera = Camera.main;
            if (hasWorldCollectTrigger && camera != null && camera.GetComponent<Physics2DRaycaster>() == null)
            {
                report.Add(NotebookHarnessSeverity.Warning, "COLLECT_NO_RAYCAST", "Main Camera missing Physics2DRaycaster for 2D collect.", sceneName);
            }
        }

        private static bool IsUiCollectTrigger(NotebookCollectTrigger trigger)
        {
            return trigger != null
                && trigger.GetComponent<RectTransform>() != null
                && trigger.GetComponent<Graphic>() != null;
        }

        private static void RequireKind(
            NotebookHarnessReport report,
            string sceneName,
            NotebookSceneInteractMarker[] markers,
            NotebookSceneInteractKind kind,
            string labelHint)
        {
            if (!markers.Any(marker => marker.Kind == kind))
            {
                report.Add(NotebookHarnessSeverity.Error, "INTERACT_" + kind, "Missing interact marker: " + labelHint, sceneName);
            }
        }

        private static void RequireSectionLine(NotebookHarnessReport report, string sceneName, NotebookSceneInteractMarker[] markers, NotebookSection section)
        {
            if (!markers.Any(marker => marker.Kind == NotebookSceneInteractKind.DirectorySection && marker.Section == section))
            {
                report.Add(NotebookHarnessSeverity.Error, "INTERACT_DIR_" + section, "Missing directory section marker: " + section, sceneName);
            }
        }

        private static void RequireHandPlacedRect(
            NotebookHarnessReport report,
            string sceneName,
            string objectName,
            NotebookUILayoutRole requiredRole)
        {
            GameObject target = FindSceneObjectByName(objectName);
            if (target == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "HANDPLACED_TARGET_MISSING", "Missing '" + objectName + "' for hand-placed audit.", sceneName);
                return;
            }

            NotebookHandPlacedRect handPlaced = target.GetComponent<NotebookHandPlacedRect>();
            if (handPlaced == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "HANDPLACED_MISSING_" + requiredRole, "Missing NotebookHandPlacedRect on '" + objectName + "'.", sceneName);
                return;
            }

            if (handPlaced.LayoutRole != requiredRole)
            {
                report.Add(
                    NotebookHarnessSeverity.Error,
                    "HANDPLACED_ROLE_" + requiredRole,
                    "'" + objectName + "' has hand-placed role '" + handPlaced.LayoutRole + "', expected '" + requiredRole + "'.",
                    sceneName);
            }
        }

        private static void RequireToastAnchor(NotebookHarnessReport report, string sceneName)
        {
            GameObject toast = FindSceneObjectByName("Collected Toast");
            if (toast == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "TOAST_MISSING", "Missing 'Collected Toast' for hand-placed audit.", sceneName);
                return;
            }

            bool hasHandPlaced = toast.TryGetComponent(out NotebookHandPlacedRect handPlaced)
                && handPlaced.LayoutRole == NotebookUILayoutRole.CollectedToast;
            bool hasNotificationAnchor = toast.TryGetComponent(out NotebookNotificationAnchor notificationAnchor)
                && notificationAnchor.UseHandPlacedRestPosition;

            if (!hasHandPlaced && !hasNotificationAnchor)
            {
                report.Add(
                    NotebookHarnessSeverity.Error,
                    "TOAST_HANDPLACED_MISSING",
                    "Collected Toast needs NotebookHandPlacedRect or NotebookNotificationAnchor hand placement.",
                    sceneName);
            }
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

        private static bool IsHomeScene(string scenePath, string sceneName)
        {
            if (!string.IsNullOrWhiteSpace(scenePath) && scenePath.Replace('\\', '/').EndsWith(HomeSceneSuffix))
            {
                return true;
            }

            return sceneName == "NoteBookTesting";
        }

        private static bool IsCollectScene(string scenePath, string sceneName)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return sceneName != null && sceneName.StartsWith("NotebookTest_") && sceneName != "NoteBookTesting";
            }

            string normalized = scenePath.Replace('\\', '/');
            return normalized.Contains("NotebookTest_") && !normalized.EndsWith(HomeSceneSuffix);
        }
    }
}
