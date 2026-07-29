using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Peaceland.Notebook
{
    public enum NotebookTestSceneKind
    {
        NotebookHome = 0,
        FloristMinigame = 1,
        FloristItemCollect = 2,
        RandJItemCollect = 3,
        IntroNewspaper = 4,
    }

    public class NotebookTestSceneNavigator : MonoBehaviour
    {
        private const string SaveKey = "peaceland.notebook.state";

        [Serializable]
        private sealed class SceneLink
        {
            public NotebookTestSceneKind kind;
            public string sceneName;
            public string scenePath;
            public string label;
        }

        [SerializeField] private NotebookTestSceneKind sceneKind = NotebookTestSceneKind.NotebookHome;
        [SerializeField] private Canvas rootCanvas;

        private bool isBuilding;

        private static readonly SceneLink[] SceneLinks =
        {
            new SceneLink
            {
                kind = NotebookTestSceneKind.NotebookHome,
                sceneName = "NoteBookTesting",
                scenePath = "Assets/Notebook/Scenes/NoteBookTesting.unity",
                label = "Notebook Home",
            },
            new SceneLink
            {
                kind = NotebookTestSceneKind.FloristMinigame,
                sceneName = "NotebookTest_FloristMinigame",
                scenePath = "Assets/Notebook/Scenes/NotebookTest_FloristMinigame.unity",
                label = "Florist Minigame",
            },
            new SceneLink
            {
                kind = NotebookTestSceneKind.FloristItemCollect,
                sceneName = "NotebookTest_FloristItemCollect",
                scenePath = "Assets/Notebook/Scenes/NotebookTest_FloristItemCollect.unity",
                label = "Florist Item Collect",
            },
            new SceneLink
            {
                kind = NotebookTestSceneKind.RandJItemCollect,
                sceneName = "NotebookTest_RandJItemCollect",
                scenePath = "Assets/Notebook/Scenes/NotebookTest_RandJItemCollect.unity",
                label = "R&J Item Collect",
            },
            new SceneLink
            {
                kind = NotebookTestSceneKind.IntroNewspaper,
                sceneName = "NotebookTest_IntroNewspaper",
                scenePath = "Assets/Notebook/Scenes/NotebookTest_IntroNewspaper.unity",
                label = "Intro Newspaper",
            },
        };

        public void Configure(NotebookTestSceneKind kind)
        {
            sceneKind = kind;
            EnsureSetup();
        }

        private void Reset()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            EnsureSetup();
        }

        private void OnEnable()
        {
            // Do not rebuild navigator UI in Edit mode — blocks Scene-view transforms.
            if (!Application.isPlaying)
            {
                return;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Intentionally empty.
        }
#endif

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureSetup();
        }

        /// <summary>Called from editor authoring menus only.</summary>
        public void EditorEnsureSetup()
        {
            EnsureSetup();
        }

        private void EnsureSetup()
        {
            if (isBuilding)
            {
                return;
            }

            isBuilding = true;

            try
            {
                EnsureEventSystem();
                EnsureCanvas();
                BuildNavigator();
            }
            finally
            {
                isBuilding = false;
            }
        }

        private void EnsureEventSystem()
        {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem != null)
            {
                EnsureInputModule(eventSystem.gameObject);
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(RectTransform), typeof(EventSystem));
            EnsureInputModule(eventSystemObject);
        }

        private void EnsureInputModule(GameObject eventSystemObject)
        {
            if (eventSystemObject == null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            RemoveAllComponentsIfPresent<StandaloneInputModule>(eventSystemObject);
            RemoveDuplicateComponents<InputSystemUIInputModule>(eventSystemObject);
            GetOrAddComponent<InputSystemUIInputModule>(eventSystemObject);
#else
            RemoveDuplicateComponents<StandaloneInputModule>(eventSystemObject);
            GetOrAddComponent<StandaloneInputModule>(eventSystemObject);
#endif
        }

        private void EnsureCanvas()
        {
            if (rootCanvas != null)
            {
                return;
            }

            rootCanvas = FindFirstObjectByType<Canvas>();
            if (rootCanvas != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("Notebook Test Navigator Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            rootCanvas = canvasObject.GetComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildNavigator()
        {
            Transform existingRoot = rootCanvas != null ? rootCanvas.transform.Find("Notebook Test Navigator") : null;
            if (existingRoot != null)
            {
                BindExistingNavigator(existingRoot);
                return;
            }

            Transform root = EnsureChild(rootCanvas.transform, "Notebook Test Navigator");
            SetStretch(root as RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Transform panel = EnsureChild(root, "Panel");
            Image panelImage = GetOrAddComponent<Image>(panel.gameObject);
            panelImage.color = new Color(0.12f, 0.1f, 0.07f, 0.9f);
            SetStretch(panel as RectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -188f), new Vector2(-16f, -16f));

            Transform titleRoot = EnsureChild(panel, "Title");
            TMP_Text titleText = GetOrAddComponent<TextMeshProUGUI>(titleRoot.gameObject);
            titleText.text = $"Notebook Test: {GetCurrentLabel()}";
            titleText.fontSize = 30f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = new Color(0.96f, 0.92f, 0.82f, 1f);
            SetStretch(titleRoot as RectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -54f), new Vector2(-24f, -12f));

            Transform subtitleRoot = EnsureChild(panel, "Subtitle");
            TMP_Text subtitleText = GetOrAddComponent<TextMeshProUGUI>(subtitleRoot.gameObject);
            subtitleText.text = "All scenes below are notebook test scenes and can jump to each other.";
            subtitleText.fontSize = 18f;
            subtitleText.alignment = TextAlignmentOptions.Left;
            subtitleText.color = new Color(0.9f, 0.86f, 0.75f, 1f);
            SetStretch(subtitleRoot as RectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -88f), new Vector2(-24f, -52f));

            Transform navRoot = EnsureChild(panel, "Nav Buttons");
            SetupHorizontalLayout(navRoot.gameObject, 12f, new RectOffset(0, 0, 0, 0), useContentSizeFitter: false);
            SetStretch(navRoot as RectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -154f), new Vector2(-24f, -100f));
            RebuildNavButtons(navRoot);
            ForceLayout(navRoot as RectTransform);

            Transform actionRoot = EnsureChild(panel, "Scenario Buttons");
            SetupHorizontalLayout(actionRoot.gameObject, 12f, new RectOffset(0, 0, 0, 0), useContentSizeFitter: false);
            SetStretch(actionRoot as RectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -214f), new Vector2(-24f, -160f));
            RebuildScenarioButtons(actionRoot);
            ForceLayout(actionRoot as RectTransform);
        }

        private void BindExistingNavigator(Transform root)
        {
            Transform panel = root.Find("Panel");
            if (panel == null)
            {
                return;
            }

            TMP_Text titleText = panel.Find("Title")?.GetComponent<TMP_Text>();
            if (titleText != null)
            {
                titleText.text = $"Notebook Test: {GetCurrentLabel()}";
            }

            TMP_Text subtitleText = panel.Find("Subtitle")?.GetComponent<TMP_Text>();
            if (subtitleText != null)
            {
                subtitleText.text = "All scenes below are notebook test scenes and can jump to each other.";
            }

            Transform navRoot = panel.Find("Nav Buttons");
            if (navRoot != null)
            {
                RebuildNavButtons(navRoot);
                ForceLayout(navRoot as RectTransform);
            }

            Transform actionRoot = panel.Find("Scenario Buttons");
            if (actionRoot != null)
            {
                RebuildScenarioButtons(actionRoot);
                ForceLayout(actionRoot as RectTransform);
            }
        }

        private void RebuildNavButtons(Transform navRoot)
        {
            for (int i = 0; i < SceneLinks.Length; i++)
            {
                SceneLink link = SceneLinks[i];
                Button button = CreateButton(navRoot, link.label, new Color(0.38f, 0.28f, 0.19f, 1f));
                bool isCurrent = link.kind == sceneKind;
                if (isCurrent)
                {
                    Image image = button.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = new Color(0.79f, 0.57f, 0.27f, 1f);
                    }
                }

                button.interactable = !isCurrent;
                NotebookTestSceneKind targetKind = link.kind;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => LoadScene(targetKind));
            }

            HideExtraChildren(navRoot, SceneLinks.Length);
        }

        private void RebuildScenarioButtons(Transform actionRoot)
        {
            int childIndex = 0;

            switch (sceneKind)
            {
                case NotebookTestSceneKind.NotebookHome:
                    CreateInfoLabel(actionRoot, childIndex++, "Home scene for checking the notebook UI and saved state.");
                    CreateButton(actionRoot, "Clear Notebook Save", new Color(0.49f, 0.22f, 0.18f, 1f)).onClick.AddListener(ClearNotebookSave);
                    childIndex++;
                    break;
                case NotebookTestSceneKind.FloristMinigame:
                    CreateButton(actionRoot, "Unlock Florist Flower Note", new Color(0.32f, 0.47f, 0.26f, 1f)).onClick.AddListener(() =>
                        NotebookGlobalBridge.CollectEntry("NotebookEntry_Memory1FloristFlower"));
                    CreateButton(actionRoot, "Clear Notebook Save", new Color(0.49f, 0.22f, 0.18f, 1f)).onClick.AddListener(ClearNotebookSave);
                    childIndex += 2;
                    break;
                case NotebookTestSceneKind.FloristItemCollect:
                    CreateButton(actionRoot, "Collect Florist Normal Note", new Color(0.32f, 0.47f, 0.26f, 1f)).onClick.AddListener(() =>
                        NotebookGlobalBridge.CollectEntry("NotebookEntry_Memory1FloristNormal"));
                    CreateButton(actionRoot, "Clear Notebook Save", new Color(0.49f, 0.22f, 0.18f, 1f)).onClick.AddListener(ClearNotebookSave);
                    childIndex += 2;
                    break;
                case NotebookTestSceneKind.RandJItemCollect:
                    CreateButton(actionRoot, "Collect R&J Balcony Note", new Color(0.32f, 0.47f, 0.26f, 1f)).onClick.AddListener(() =>
                        NotebookGlobalBridge.CollectEntry("NotebookEntry_Memory2RJBalcony"));
                    CreateButton(actionRoot, "Collect R&J Letter Note", new Color(0.26f, 0.4f, 0.52f, 1f)).onClick.AddListener(() =>
                        NotebookGlobalBridge.CollectEntry("NotebookEntry_Memory2RJLetter"));
                    CreateButton(actionRoot, "Clear Notebook Save", new Color(0.49f, 0.22f, 0.18f, 1f)).onClick.AddListener(ClearNotebookSave);
                    childIndex += 3;
                    break;
                case NotebookTestSceneKind.IntroNewspaper:
                    CreateButton(actionRoot, "Collect Newspaper Note", new Color(0.32f, 0.47f, 0.26f, 1f)).onClick.AddListener(() =>
                        NotebookGlobalBridge.CollectEntry("NotebookEntry_PresentNewspaper"));
                    CreateButton(actionRoot, "Clear Notebook Save", new Color(0.49f, 0.22f, 0.18f, 1f)).onClick.AddListener(ClearNotebookSave);
                    childIndex += 2;
                    break;
            }

            HideExtraChildren(actionRoot, childIndex);
        }

        private void LoadScene(NotebookTestSceneKind targetKind)
        {
            SceneLink link = GetSceneLink(targetKind);
            if (link == null)
            {
                Debug.LogWarning($"No notebook test scene link found for {targetKind}.", this);
                return;
            }

            SceneManager.LoadScene(link.sceneName);
        }

        private void ClearNotebookSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }

        private string GetCurrentLabel()
        {
            SceneLink link = GetSceneLink(sceneKind);
            return link != null ? link.label : sceneKind.ToString();
        }

        private static SceneLink GetSceneLink(NotebookTestSceneKind kind)
        {
            for (int i = 0; i < SceneLinks.Length; i++)
            {
                if (SceneLinks[i].kind == kind)
                {
                    return SceneLinks[i];
                }
            }

            return null;
        }

        private static void CreateInfoLabel(Transform parent, int index, string text)
        {
            Transform labelRoot = EnsureChild(parent, $"Info {index}");
            TMP_Text label = GetOrAddComponent<TextMeshProUGUI>(labelRoot.gameObject);
            labelRoot.gameObject.SetActive(true);
            label.text = text;
            label.fontSize = 20f;
            label.alignment = TextAlignmentOptions.Left;
            label.color = new Color(0.96f, 0.92f, 0.82f, 1f);

            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(labelRoot.gameObject);
            layoutElement.preferredWidth = 520f;
            layoutElement.preferredHeight = 52f;
        }

        private static Button CreateButton(Transform parent, string label, Color backgroundColor)
        {
            Transform buttonRoot = EnsureChild(parent, $"{label} Button");
            buttonRoot.gameObject.SetActive(true);
            Image background = GetOrAddComponent<Image>(buttonRoot.gameObject);
            background.color = backgroundColor;

            LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(buttonRoot.gameObject);
            layoutElement.preferredWidth = 260f;
            layoutElement.preferredHeight = 52f;

            Button button = GetOrAddComponent<Button>(buttonRoot.gameObject);
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
            colors.pressedColor = new Color(0.84f, 0.84f, 0.84f, 1f);
            button.colors = colors;

            Transform labelRoot = EnsureChild(buttonRoot, "Label");
            TMP_Text labelText = GetOrAddComponent<TextMeshProUGUI>(labelRoot.gameObject);
            labelText.text = label;
            labelText.fontSize = 19f;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = new Color(0.18f, 0.14f, 0.1f, 1f);
            SetStretch(labelRoot as RectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f));

            button.onClick.RemoveAllListeners();
            return button;
        }

        private static void HideExtraChildren(Transform parent, int activeChildCount)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                parent.GetChild(i).gameObject.SetActive(i < activeChildCount);
            }
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                component = target.AddComponent<T>();
            }

            return component;
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child;
            }

            GameObject childObject = new GameObject(childName, typeof(RectTransform));
            child = childObject.transform;
            child.SetParent(parent, false);
            return child;
        }

        private static void RemoveAllComponentsIfPresent<T>(GameObject target) where T : Component
        {
            T[] components = target.GetComponents<T>();
            for (int i = 0; i < components.Length; i++)
            {
                DestroyUnityObject(components[i]);
            }
        }

        private static void RemoveDuplicateComponents<T>(GameObject target) where T : Component
        {
            T[] components = target.GetComponents<T>();
            for (int i = 1; i < components.Length; i++)
            {
                DestroyUnityObject(components[i]);
            }
        }

        private static void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

#if UNITY_EDITOR
            DestroyImmediate(target);
#else
            Destroy(target);
#endif
        }

        private static void SetupHorizontalLayout(GameObject target, float spacing, RectOffset padding, bool useContentSizeFitter = true)
        {
            HorizontalLayoutGroup layout = GetOrAddComponent<HorizontalLayoutGroup>(target);
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            ContentSizeFitter fitter = target.GetComponent<ContentSizeFitter>();
            if (useContentSizeFitter)
            {
                fitter = GetOrAddComponent<ContentSizeFitter>(target);
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                return;
            }

            if (fitter != null)
            {
                DestroyUnityObject(fitter);
            }
        }

        private static void ForceLayout(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        private static void SetStretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}
