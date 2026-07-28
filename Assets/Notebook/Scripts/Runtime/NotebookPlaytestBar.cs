using Peaceland;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Runtime playtest navigation bar for all notebook test scenes. Added automatically in Play mode.
    /// </summary>
    public class NotebookPlaytestBar : MonoBehaviour
    {
        private static readonly SceneLink[] SceneLinks =
        {
            new SceneLink("NoteBookTesting", "Home"),
            new SceneLink("NotebookTest_FloristMinigame", "Florist MG"),
            new SceneLink("NotebookTest_FloristItemCollect", "Florist Collect"),
            new SceneLink("NotebookTest_RandJItemCollect", "R&J Collect"),
            new SceneLink("NotebookTest_IntroNewspaper", "Newspaper"),
        };

        private Canvas canvas;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapIfNotebookTestScene()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            string sceneName = SceneManager.GetActiveScene().name;
            if (!IsNotebookTestScene(sceneName))
            {
                return;
            }

            if (FindFirstObjectByType<NotebookPlaytestBar>() != null)
            {
                return;
            }

            GameObject host = new GameObject("Notebook Playtest Bar");
            host.AddComponent<NotebookPlaytestBar>();
        }

        public static bool IsNotebookTestScene(string sceneName)
        {
            for (int i = 0; i < SceneLinks.Length; i++)
            {
                if (SceneLinks[i].SceneName == sceneName)
                {
                    return true;
                }
            }

            return false;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            BuildForActiveScene();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsNotebookTestScene(scene.name))
            {
                Destroy(gameObject);
                return;
            }

            BuildForActiveScene();
        }

        private void BuildForActiveScene()
        {
            if (canvas != null)
            {
                Destroy(canvas.gameObject);
                canvas = null;
            }

            canvas = CreateCanvas();
            Transform panel = CreatePanel(canvas.transform);
            CreateTitle(panel, SceneManager.GetActiveScene().name);
            CreateNavRow(panel);
            CreateStatusRow(panel);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Notebook Playtest Bar Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas newCanvas = canvasObject.GetComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            newCanvas.sortingOrder = 5000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return newCanvas;
        }

        private static Transform CreatePanel(Transform parent)
        {
            GameObject panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);
            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.1f, 0.08f, 0.06f, 0.92f);

            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 118f);

            return panelObject.transform;
        }

        private static void CreateTitle(Transform panel, string sceneName)
        {
            GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObject.transform.SetParent(panel, false);
            TMP_Text title = titleObject.GetComponent<TextMeshProUGUI>();
            title.text = "Notebook Playtest — " + sceneName;
            title.fontSize = 22f;
            title.fontStyle = FontStyles.Bold;
            title.color = new Color(0.96f, 0.92f, 0.82f, 1f);
            title.alignment = TextAlignmentOptions.MidlineLeft;

            RectTransform rect = titleObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -8f);
            rect.sizeDelta = new Vector2(-32f, 28f);
        }

        private void CreateNavRow(Transform panel)
        {
            GameObject rowObject = new GameObject("Nav Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowObject.transform.SetParent(panel, false);
            HorizontalLayoutGroup layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(16, 16, 0, 0);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;

            RectTransform rect = rowObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -40f);
            rect.sizeDelta = new Vector2(0f, 40f);

            string activeScene = SceneManager.GetActiveScene().name;
            for (int i = 0; i < SceneLinks.Length; i++)
            {
                SceneLink link = SceneLinks[i];
                bool isCurrent = link.SceneName == activeScene;
                Button button = CreateBarButton(rowObject.transform, link.Label, isCurrent);
                if (!isCurrent)
                {
                    string target = link.SceneName;
                    button.onClick.AddListener(() => SceneManager.LoadScene(target));
                }
            }

            CreateBarButton(rowObject.transform, "Clear Save", false).onClick.AddListener(ClearSave);
        }

        private void CreateStatusRow(Transform panel)
        {
            GameObject statusObject = new GameObject("Status", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusObject.transform.SetParent(panel, false);
            TMP_Text status = statusObject.GetComponent<TextMeshProUGUI>();
            status.text = BuildStatusText();
            status.fontSize = 16f;
            status.color = new Color(0.86f, 0.82f, 0.72f, 1f);
            status.alignment = TextAlignmentOptions.MidlineLeft;

            RectTransform rect = statusObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -84f);
            rect.sizeDelta = new Vector2(-32f, 24f);
        }

        private static string BuildStatusText()
        {
            int collected = NotebookSaveUtility.CountCollected();
            string statsLine = "K/C=" + PeacelandStatManager.Instance.Get(PeacelandStatId.KindnessCruelty)
                + " | S/A=" + PeacelandStatManager.Instance.Get(PeacelandStatId.SelfishAltruistic)
                + " | checkpoint=" + PeacelandProgress.Instance.GetCurrentSceneCheckpoint();
            return collected == 0
                ? statsLine + " | notebook: none collected yet"
                : statsLine + " | notebook collected: " + collected;
        }

        private static void ClearSave()
        {
            PlayerPrefs.DeleteKey(NotebookSaveUtility.DefaultSaveKey);
            PlayerPrefs.Save();
            NotebookPlaytestBar existing = FindFirstObjectByType<NotebookPlaytestBar>();
            if (existing != null)
            {
                existing.BuildForActiveScene();
            }
        }

        private static Button CreateBarButton(Transform parent, string label, bool highlighted)
        {
            GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = highlighted
                ? new Color(0.79f, 0.57f, 0.27f, 1f)
                : new Color(0.38f, 0.28f, 0.19f, 1f);

            LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 36f;
            layoutElement.preferredWidth = Mathf.Max(120f, label.Length * 11f);

            Button button = buttonObject.GetComponent<Button>();
            button.interactable = !highlighted;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            TMP_Text labelText = labelObject.GetComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 16f;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = new Color(0.16f, 0.12f, 0.08f, 1f);
            RectTransform labelRect = labelText.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 4f);
            labelRect.offsetMax = new Vector2(-8f, -4f);

            return button;
        }

        private readonly struct SceneLink
        {
            public SceneLink(string sceneName, string label)
            {
                SceneName = sceneName;
                Label = label;
            }

            public string SceneName { get; }
            public string Label { get; }
        }
    }
}
