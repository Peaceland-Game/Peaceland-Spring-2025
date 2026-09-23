using System;
using System.Linq;
using Peaceland;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Behaviour for the prefab-authored Notebook test controls.
    /// It binds actions only; it never creates UI at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotebookTestHarness : MonoBehaviour
    {
        [Serializable]
        public struct SceneButtonLink
        {
            public Button button;
            public string sceneName;
        }

        [Header("Notebook")]
        [SerializeField] private NotebookController notebookController;
        [SerializeField] private NotebookUIShellReferences shell;
        [SerializeField] private NotebookDatabase database;
        [SerializeField] private NotebookContentCatalog contentCatalog;

        [Header("Prefab UI")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private TMP_Text toggleLabel;
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button runContentCheckButton;
        [SerializeField] private Button collectAllButton;
        [SerializeField] private Button kindnessPlusButton;
        [SerializeField] private Button kindnessMinusButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button toggleFlagButton;
        [SerializeField] private Button logStatsButton;
        [SerializeField] private Button clearNotebookButton;
        [SerializeField] private SceneButtonLink[] sceneButtons = Array.Empty<SceneButtonLink>();

        private bool listenersBound;

        private void Awake()
        {
            ResolveNotebookReferences();
            RebindListeners();
            RefreshHarnessStatus();
        }

        private void OnEnable()
        {
            ResolveNotebookReferences();
            RebindListeners();
        }

        private void OnDestroy()
        {
            UnbindListeners();
        }

        public void Configure(NotebookController controller, NotebookUIShellReferences shellReferences)
        {
            notebookController = controller;
            shell = shellReferences;
            database = notebookController != null ? notebookController.Database : database;
            RebindListeners();
            RefreshHarnessStatus();
        }

        private void RebindListeners()
        {
            UnbindListeners();
            BindListeners();
        }

        private void ResolveNotebookReferences()
        {
            shell ??= NotebookSceneLookup.FindShell();
            notebookController ??= FindFirstObjectByType<NotebookController>(FindObjectsInactive.Include);
            database ??= notebookController != null ? notebookController.Database : null;
            contentCatalog ??= Resources.Load<NotebookContentCatalog>("NotebookContentCatalog");
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            Bind(toggleButton, TogglePanel);
            Bind(runContentCheckButton, RunContentCheck);
            Bind(collectAllButton, CollectAllGameplay);
            Bind(kindnessPlusButton, AddKindness);
            Bind(kindnessMinusButton, RemoveKindness);
            Bind(saveButton, SaveGame);
            Bind(loadButton, LoadGame);
            Bind(toggleFlagButton, ToggleTestProgressFlag);
            Bind(logStatsButton, LogAllStats);
            Bind(clearNotebookButton, ClearNotebookSave);

            for (int i = 0; i < sceneButtons.Length; i++)
            {
                SceneButtonLink link = sceneButtons[i];
                if (link.button == null || string.IsNullOrWhiteSpace(link.sceneName))
                {
                    continue;
                }

                string targetScene = link.sceneName;
                link.button.onClick.AddListener(() => SceneManager.LoadScene(targetScene));
            }

            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            Unbind(toggleButton, TogglePanel);
            Unbind(runContentCheckButton, RunContentCheck);
            Unbind(collectAllButton, CollectAllGameplay);
            Unbind(kindnessPlusButton, AddKindness);
            Unbind(kindnessMinusButton, RemoveKindness);
            Unbind(saveButton, SaveGame);
            Unbind(loadButton, LoadGame);
            Unbind(toggleFlagButton, ToggleTestProgressFlag);
            Unbind(logStatsButton, LogAllStats);
            Unbind(clearNotebookButton, ClearNotebookSave);

            for (int i = 0; i < sceneButtons.Length; i++)
            {
                sceneButtons[i].button?.onClick.RemoveAllListeners();
            }

            listenersBound = false;
        }

        private void TogglePanel()
        {
            if (panel == null)
            {
                return;
            }

            bool showPanel = !panel.activeSelf;
            if (showPanel && notebookController != null)
            {
                notebookController.CloseNotebook();
            }

            panel.SetActive(showPanel);
            if (toggleLabel != null)
            {
                toggleLabel.text = panel.activeSelf ? "Hide notebook test tools" : "Show notebook test tools";
            }
        }

        private void RunContentCheck()
        {
            if (database == null || contentCatalog == null)
            {
                SetStatus("Missing NotebookDatabase or NotebookContentCatalog.");
                return;
            }

            NotebookHarnessReport report = NotebookHarnessValidator.Validate(database, contentCatalog);
            SetStatus(report.Passed
                ? $"Content check PASS ({report.WarningCount} warnings)."
                : $"Content check FAIL: {report.ErrorCount} errors. See Console.");

            foreach (NotebookHarnessIssue issue in report.Issues.Where(item =>
                         item.severity == NotebookHarnessSeverity.Error))
            {
                Debug.LogError($"[NotebookHarness] {issue.code}: {issue.message} {issue.entryId}");
            }
        }

        private void CollectAllGameplay()
        {
            if (contentCatalog == null)
            {
                SetStatus("NotebookContentCatalog is missing.");
                return;
            }

            NotebookHarnessValidator.CollectAllGameplayEntries(contentCatalog);
            RefreshHarnessStatus();
        }

        private void AddKindness()
        {
            AdjustKindness(1);
        }

        private void RemoveKindness()
        {
            AdjustKindness(-1);
        }

        private void AdjustKindness(int delta)
        {
            PeacelandStatManager.Instance.AddDelta(PeacelandStatId.KindnessCruelty, delta);
            RefreshHarnessStatus();
        }

        private void SaveGame()
        {
            PeacelandGameBootstrap.EnsureExists();
            PeacelandSaveService.Instance.EnsureActiveSlotBound(preferExistingSlotZero: true);
            PeacelandSaveService.Instance.Save();
            RefreshHarnessStatus();
        }

        private void LoadGame()
        {
            PeacelandGameBootstrap.EnsureExists();
            PeacelandSaveService.Instance.EnsureActiveSlotBound(preferExistingSlotZero: true);
            PeacelandSaveService.Instance.Load();
            RefreshHarnessStatus();
        }

        private void ToggleTestProgressFlag()
        {
            bool next = !PeacelandProgress.Instance.HasFlag("test_flag");
            PeacelandProgress.Instance.SetFlag("test_flag", next);
            RefreshHarnessStatus();
        }

        private static void LogAllStats()
        {
            Debug.Log("[PeacelandHarness] " + PeacelandStatManager.Instance.FormatAllStats()
                + " | test_flag=" + PeacelandProgress.Instance.HasFlag("test_flag"));
        }

        private void ClearNotebookSave()
        {
            notebookController?.ClearSavedStateForDebug();
            RefreshHarnessStatus();
        }

        private void RefreshHarnessStatus()
        {
            if (statusText == null)
            {
                return;
            }

            int saved = NotebookSaveUtility.CountCollected();
            int gameplay = contentCatalog != null ? contentCatalog.GameplaySpecs().Count() : 0;
            string stats = PeacelandStatManager.Instance != null
                ? PeacelandStatManager.Instance.FormatAllStats()
                : "stats n/a";
            statusText.text = $"Save: {saved} notebook | gameplay specs: {gameplay} | {stats}";
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            button?.onClick.AddListener(action);
        }

        private static void Unbind(Button button, UnityEngine.Events.UnityAction action)
        {
            button?.onClick.RemoveListener(action);
        }
    }
}
