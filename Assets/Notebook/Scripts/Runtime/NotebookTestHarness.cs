using System.Linq;
using Peaceland;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Runtime-only test tools panel. Attach to test bootstrap — not part of the shipped NotebookOpen prefab.
    /// </summary>
    public class NotebookTestHarness : MonoBehaviour
    {
        [SerializeField] private NotebookController notebookController;
        [SerializeField] private NotebookUIShellReferences shell;
        [SerializeField] private NotebookDatabase database;
        [SerializeField] private NotebookContentCatalog contentCatalog;
        [SerializeField] private bool includeClearSave = true;

        private GameObject harnessRoot;
        private TMP_Text statusText;

        private void Awake()
        {
            if (shell == null)
            {
                shell = NotebookSceneLookup.FindShell();
            }

            if (notebookController == null)
            {
                notebookController = FindFirstObjectByType<NotebookController>();
            }

            if (database == null && notebookController != null)
            {
                database = notebookController.Database;
            }

            if (contentCatalog == null)
            {
                contentCatalog = Resources.Load<NotebookContentCatalog>("NotebookContentCatalog");
            }

            BuildHarness();
        }

        private void OnDestroy()
        {
            if (harnessRoot != null)
            {
                Destroy(harnessRoot);
            }
        }

        private void BuildHarness()
        {
            Transform anchor = shell != null ? shell.TestToolsAnchor : null;
            if (anchor == null)
            {
                return;
            }

            harnessRoot = new GameObject("Test Tools (Runtime)", typeof(RectTransform));
            harnessRoot.transform.SetParent(anchor, false);
            RectTransform rootRect = harnessRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(1f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(0f, 36f);

            GameObject toggleObject = new GameObject("Toggle", typeof(RectTransform), typeof(Image), typeof(Button));
            toggleObject.transform.SetParent(harnessRoot.transform, false);
            RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0f, 0f);
            toggleRect.anchorMax = new Vector2(1f, 1f);
            toggleRect.offsetMin = Vector2.zero;
            toggleRect.offsetMax = Vector2.zero;
            toggleObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.04f);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(toggleObject.transform, false);
            TMP_Text label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = "▸ Content harness";
            label.fontSize = 14f;
            label.alignment = TextAlignmentOptions.Left;
            label.color = new Color(0.19f, 0.15f, 0.11f, 0.55f);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(4f, 0f);
            labelRect.offsetMax = new Vector2(-4f, 0f);

            GameObject panelObject = new GameObject("Panel", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panelObject.transform.SetParent(harnessRoot.transform, false);
            panelObject.SetActive(false);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 4f);

            VerticalLayoutGroup layout = panelObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Button toggle = toggleObject.GetComponent<Button>();
            toggle.onClick.AddListener(() =>
            {
                bool next = !panelObject.activeSelf;
                panelObject.SetActive(next);
                label.text = next ? "▾ Content harness" : "▸ Content harness";
            });

            GameObject statusObject = new GameObject("Status", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusObject.transform.SetParent(panelObject.transform, false);
            statusText = statusObject.GetComponent<TextMeshProUGUI>();
            statusText.fontSize = 13f;
            statusText.alignment = TextAlignmentOptions.Left;
            statusText.color = new Color(0.25f, 0.2f, 0.14f, 0.85f);
            statusText.textWrappingMode = TextWrappingModes.Normal;
            LayoutElement statusLayout = statusObject.AddComponent<LayoutElement>();
            statusLayout.preferredHeight = 48f;
            RefreshHarnessStatus();

            CreateHarnessButton(panelObject.transform, "Run content check", RunContentCheck);
            CreateHarnessButton(panelObject.transform, "Collect all gameplay (catalog)", CollectAllGameplay);

            CreateHarnessButton(panelObject.transform, "Kindness +1 (K/C)", () => AdjustKindness(1));
            CreateHarnessButton(panelObject.transform, "Kindness -1", () => AdjustKindness(-1));
            CreateHarnessButton(panelObject.transform, "Save game", () =>
            {
                PeacelandSaveService.Instance.Save();
                RefreshHarnessStatus();
            });
            CreateHarnessButton(panelObject.transform, "Load game", () =>
            {
                PeacelandSaveService.Instance.Load();
                RefreshHarnessStatus();
            });
            CreateHarnessButton(panelObject.transform, "Toggle test_flag progress", ToggleTestProgressFlag);
            CreateHarnessButton(panelObject.transform, "Log all stats", LogAllStats);
            if (includeClearSave && notebookController != null)
            {
                CreateHarnessButton(panelObject.transform, "Clear notebook save", () =>
                {
                    notebookController.ClearSavedStateForDebug();
                    RefreshHarnessStatus();
                });
            }

            CreateSceneJumpButtons(panelObject.transform);
        }

        private void RunContentCheck()
        {
            if (database == null || contentCatalog == null)
            {
                SetStatus("Missing database or content catalog. Run Peaceland → Notebook → Harness → Create Or Refresh Content Catalog.");
                return;
            }

            NotebookHarnessReport report = NotebookHarnessValidator.Validate(database, contentCatalog);
            SetStatus(report.Passed
                ? "Content check PASS (" + report.WarningCount + " warnings)."
                : "Content check FAIL: " + report.ErrorCount + " errors. See Console.");
            for (int i = 0; i < report.Issues.Count; i++)
            {
                NotebookHarnessIssue issue = report.Issues[i];
                if (issue.severity == NotebookHarnessSeverity.Error)
                {
                    Debug.LogError("[NotebookHarness] " + issue.code + ": " + issue.message + " " + issue.entryId);
                }
            }
        }

        private void CollectAllGameplay()
        {
            if (contentCatalog == null)
            {
                SetStatus("Content catalog missing.");
                return;
            }

            NotebookHarnessValidator.CollectAllGameplayEntries(contentCatalog);
            RefreshHarnessStatus();
            SetStatus("Collected all catalog gameplay entries.");
        }

        private void ToggleTestProgressFlag()
        {
            bool next = !PeacelandProgress.Instance.HasFlag("test_flag");
            PeacelandProgress.Instance.SetFlag("test_flag", next);
            RefreshHarnessStatus();
            SetStatus("test_flag = " + next);
        }

        private static void LogAllStats()
        {
            Debug.Log("[PeacelandHarness] " + PeacelandStatManager.Instance.FormatAllStats()
                + " | test_flag=" + PeacelandProgress.Instance.HasFlag("test_flag"));
        }

        private void AdjustKindness(int delta)
        {
            PeacelandStatManager.Instance.AddDelta(PeacelandStatId.KindnessCruelty, delta);
            RefreshHarnessStatus();
            SetStatus("Kindness/Cruelty = " + PeacelandStatManager.Instance.Get(PeacelandStatId.KindnessCruelty));
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
            statusText.text = "Save: " + saved + " notebook | gameplay specs: " + gameplay + " | " + stats;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private void CreateSceneJumpButtons(Transform parent)
        {
            string[] labels =
            {
                "Notebook Home",
                "Florist Minigame",
                "Florist Collect",
                "R&J Collect",
                "Intro Newspaper",
            };

            string[] sceneNames =
            {
                "NoteBookTesting",
                "NotebookTest_FloristMinigame",
                "NotebookTest_FloristItemCollect",
                "NotebookTest_RandJItemCollect",
                "NotebookTest_IntroNewspaper",
            };

            for (int i = 0; i < labels.Length; i++)
            {
                string sceneName = sceneNames[i];
                CreateHarnessButton(parent, labels[i], () => UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName));
            }
        }

        private static void CreateHarnessButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = new Color(0.42f, 0.31f, 0.21f, 0.9f);
            buttonObject.GetComponent<LayoutElement>().preferredHeight = 32f;
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            TMP_Text label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 14f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 2f);
            labelRect.offsetMax = new Vector2(-8f, -2f);
        }
    }
}
