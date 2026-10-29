using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string ScreenshotPathKey = "PlayModeTest.ScreenshotPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static List<string> _capturedLogs = new List<string>();

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");

            switch (state)
            {
                case "Idle":
                    break;

                case "WaitingForCompile":
                    Debug.Log("[PlayModeTest] Scheduling Play Mode entry.");
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;

                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += Tick;
                    }
                    break;

                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.update += Tick;
                    }
                    break;

                case "Done":
                    Debug.Log(SentinelLog);
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += Tick;
            }
        }

        private static int _frameCount = 0;
        private static bool _screenshotTaken = false;
        private static string _screenshotPath = "";

        private static void Tick()
        {
            _frameCount++;
            if (_frameCount < 12) return;

            if (!_screenshotTaken)
            {
                _screenshotPath = "Assets/PlayModeTest_SingleChoice_Verified.png";
                ScreenCapture.CaptureScreenshot(_screenshotPath);
                SessionState.SetString(ScreenshotPathKey, _screenshotPath);
                _screenshotTaken = true;
                return;
            }

            EditorApplication.update -= Tick;
            Application.logMessageReceived += OnLogMessage;
            string resultJson;
            try
            {
                resultJson = RunTestLogic();
            }
            catch (System.Exception e)
            {
                Debug.LogError("[PlayModeTest] Exception: " + e);
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = e.Message,
                    logs = _capturedLogs.ToArray()
                });
            }
            finally
            {
                Application.logMessageReceived -= OnLogMessage;
            }

            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception ||
                message.Contains("[Test]") || message.Contains("TEST_RESULT"))
            {
                _capturedLogs.Add("[" + type + "] " + message);
            }
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public string screenshotPath;
            public bool piece0Snapped;
            public bool piece1ReplacedPiece0;
            public bool piece0DisplacedBackToBank;
            public bool summarySpreadOpened;
            public string[] logs;
        }

        private static string RunTestLogic()
        {
            var controller = Object.FindFirstObjectByType<JournalMinigameController>();
            if (controller == null)
            {
                return JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = "JournalMinigameController not found",
                    logs = _capturedLogs.ToArray()
                });
            }

            var dm = Object.FindFirstObjectByType<DragManager>();
            var allTargets = Object.FindObjectsByType<DragTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log("[Test] Found allTargets length: " + allTargets.Length);

            DragTarget target0 = null;
            foreach (var t in allTargets)
            {
                if (t.gameObject.name == "TargetSlot_Page_0")
                {
                    target0 = t;
                    break;
                }
            }

            var allDraggables = Object.FindObjectsByType<Draggable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log("[Test] Found allDraggables length: " + allDraggables.Length);

            Draggable piece0 = null;
            Draggable piece1 = null;
            foreach (var d in allDraggables)
            {
                if (d.name == "Piece_Page_0_Choice_0") piece0 = d;
                if (d.name == "Piece_Page_0_Choice_1") piece1 = d;
            }

            if (target0 == null || piece0 == null || piece1 == null)
            {
                return JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = "Components not found: target=" + (target0 != null) + ", piece0=" + (piece0 != null) + ", piece1=" + (piece1 != null),
                    logs = _capturedLogs.ToArray()
                });
            }

            Vector3 bankPos0 = piece0.transform.position;
            Vector3 bankPos1 = piece1.transform.position;

            var soPiece = new SerializedObject(piece0);
            var dtListProp = soPiece.FindProperty("dragTargets");
            int dragTargetsCount = dtListProp != null ? dtListProp.arraySize : -1;

            Debug.Log(string.Format("[Test] target0.targetType={0} ({1}), piece0.dragTargetsCount={2}",
                target0.targetType != null ? target0.targetType : "null",
                target0.targetType != null ? target0.targetType.GetType().Name : "null",
                dragTargetsCount));

            // 1. Drag piece 0 into slot
            piece0.StartDrag(piece0.transform.position, 0);
            piece0.transform.position = target0.transform.position;
            piece0.EndDrag();

            bool p0Snapped = (target0.currentSnappedPiece == piece0 && target0.isSnapped);
            Debug.Log(string.Format("[Test] Piece 0 snapped into target slot: {0} (currentSnappedPiece={1}, isSnapped={2})",
                p0Snapped, target0.currentSnappedPiece != null ? target0.currentSnappedPiece.name : "null", target0.isSnapped));

            // 2. Drag piece 1 into the SAME slot (should take one choice at a time: replace piece 0 and displace piece 0 back to bank)
            piece1.StartDrag(piece1.transform.position, 0);
            piece1.transform.position = target0.transform.position;
            piece1.EndDrag();

            bool p1ReplacedP0 = (target0.currentSnappedPiece == piece1 && target0.isSnapped);
            bool p0Returned = (piece0.transform.position == bankPos0);
            Debug.Log("[Test] Piece 1 replaced Piece 0: " + p1ReplacedP0);
            Debug.Log("[Test] Piece 0 returned back to word bank: " + p0Returned);

            // 3. Fill page 0, page 1 and page 2 to verify review completion flow
            controller.SnapChoiceForMemory(0, 0);
            controller.SnapChoiceForMemory(1, 0);
            controller.SnapChoiceForMemory(2, 0);
            controller.OnCompleteJournalClicked();

            var summaryObj = GameObject.Find("SummarySpread_Root");
            bool summaryOpen = summaryObj != null && summaryObj.activeSelf;
            Debug.Log("[Test] Summary spread opened on all 3 complete: " + summaryOpen);

            if (summaryOpen)
            {
                // Capture the completed summary spread visually
                ScreenCapture.CaptureScreenshot("Assets/PlayModeTest_SummarySpread.png");
                SessionState.SetString(ScreenshotPathKey, "Assets/PlayModeTest_SummarySpread.png");
            }

            bool allPassed = p0Snapped && p1ReplacedP0 && p0Returned && summaryOpen;

            return JsonUtility.ToJson(new TestResult
            {
                success = allPassed,
                screenshotPath = _screenshotPath,
                piece0Snapped = p0Snapped,
                piece1ReplacedPiece0 = p1ReplacedP0,
                piece0DisplacedBackToBank = p0Returned,
                summarySpreadOpened = summaryOpen,
                logs = _capturedLogs.ToArray()
            });
        }
    }
}
