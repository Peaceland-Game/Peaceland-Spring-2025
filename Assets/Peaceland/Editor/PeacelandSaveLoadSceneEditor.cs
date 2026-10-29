#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Peaceland.Editor
{
    public static class PeacelandSaveLoadSceneEditor
    {
        public const string ScenePath = "Assets/Peaceland/Scenes/SaveLoad.unity";
        private const string BackgroundPath = "Assets/Art/Present/BG_MemoryTree_Contrast.jpg";

        [MenuItem("Peaceland/Save/Author Save Load Scene")]
        public static void AuthorSaveLoadSceneMenu()
        {
            AuthorSaveLoadScene();
        }

        [MenuItem("Peaceland/Save/Open Save Load Scene")]
        public static void OpenSaveLoadSceneMenu()
        {
            if (!File.Exists(ScenePath))
            {
                AuthorSaveLoadScene();
                return;
            }

            EditorSceneManager.OpenScene(ScenePath);
        }

        public static void AuthorSaveLoadScene()
        {
            EnsureSceneFolder();
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            CreateCameraAndLight();
            CreateEventSystem();

            Canvas canvas = CreateCanvas();
            RectTransform canvasRect = canvas.transform as RectTransform;
            CreateBackground(canvasRect);

            Image overlay = CreateImage(canvasRect, "Dark Overlay", new Color(0f, 0f, 0f, 0.58f));
            Stretch(overlay.rectTransform);

            RectTransform safeArea = CreateRect(canvasRect, "Safe Area");
            safeArea.anchorMin = new Vector2(0.045f, 0.055f);
            safeArea.anchorMax = new Vector2(0.955f, 0.945f);
            safeArea.offsetMin = Vector2.zero;
            safeArea.offsetMax = Vector2.zero;

            TMP_Text heading = CreateText(safeArea, "Heading", "SAVE FILES", 34f, FontStyles.Bold, TextAlignmentOptions.Left);
            SetAnchoredRect(heading.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -12f), new Vector2(320f, 54f), new Vector2(0f, 1f));
            heading.color = new Color(0.94f, 0.9f, 0.82f, 1f);

            Button backButton = CreateButton(safeArea, "Back Button", "<", new Color32(245, 194, 202, 255));
            SetAnchoredRect(backButton.transform as RectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-8f, -4f), new Vector2(74f, 74f), Vector2.one);

            RectTransform slotRoot = CreateRect(safeArea, "Slot Scroll");
            slotRoot.anchorMin = new Vector2(0f, 0.12f);
            slotRoot.anchorMax = new Vector2(1f, 0.88f);
            slotRoot.offsetMin = new Vector2(8f, 0f);
            slotRoot.offsetMax = new Vector2(-8f, 0f);
            Image slotFrame = slotRoot.gameObject.AddComponent<Image>();
            slotFrame.color = new Color(0.08f, 0.06f, 0.04f, 0.42f);

            ScrollRect slotScroll = slotRoot.gameObject.AddComponent<ScrollRect>();
            slotScroll.horizontal = false;
            slotScroll.vertical = true;
            slotScroll.movementType = ScrollRect.MovementType.Clamped;
            slotScroll.scrollSensitivity = 32f;

            RectTransform slotViewport = CreateRect(slotRoot, "Viewport");
            Stretch(slotViewport);
            slotViewport.offsetMin = new Vector2(16f, 16f);
            slotViewport.offsetMax = new Vector2(-16f, -16f);
            Image viewportImage = slotViewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            slotViewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            RectTransform slotContent = CreateRect(slotViewport, "Content");
            slotContent.anchorMin = new Vector2(0f, 1f);
            slotContent.anchorMax = new Vector2(1f, 1f);
            slotContent.pivot = new Vector2(0.5f, 1f);
            slotContent.anchoredPosition = Vector2.zero;
            slotContent.sizeDelta = Vector2.zero;
            slotContent.gameObject.AddComponent<GridLayoutGroup>();
            slotContent.gameObject.AddComponent<PeacelandResponsiveSaveGrid>();
            ContentSizeFitter slotFitter = slotContent.gameObject.AddComponent<ContentSizeFitter>();
            slotFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            slotScroll.viewport = slotViewport;
            slotScroll.content = slotContent;

            PeacelandSaveSlotEntryView template = CreateSlotEntry(slotContent, 0);
            PeacelandSaveSlotPanel slotPanel = slotRoot.gameObject.AddComponent<PeacelandSaveSlotPanel>();
            SerializedObject panelObject = new SerializedObject(slotPanel);
            panelObject.FindProperty("root").objectReferenceValue = slotRoot.gameObject;
            panelObject.FindProperty("slotEntryTemplate").objectReferenceValue = template;
            panelObject.FindProperty("slotEntryContainer").objectReferenceValue = slotContent;
            panelObject.FindProperty("initialVisibleSlotCount").intValue =
                PeacelandSaveSlots.InitialVisibleSlotCount;
            panelObject.FindProperty("emptySlotBuffer").intValue = 2;
            SerializedProperty entriesProperty = panelObject.FindProperty("slotEntries");
            entriesProperty.arraySize = 1;
            entriesProperty.GetArrayElementAtIndex(0).objectReferenceValue = template;
            panelObject.ApplyModifiedPropertiesWithoutUndo();

            PeacelandCheckpointListPanel checkpointPanel =
                CreateCheckpointPanel(safeArea);

            TMP_Text feedback = CreateText(
                safeArea,
                "Feedback",
                "Select a save. Each slot owns separate notebook data.",
                18f,
                FontStyles.Normal,
                TextAlignmentOptions.Center);
            feedback.color = new Color(0.94f, 0.9f, 0.82f, 0.9f);
            feedback.rectTransform.anchorMin = new Vector2(0f, 0f);
            feedback.rectTransform.anchorMax = new Vector2(1f, 0.09f);
            feedback.rectTransform.offsetMin = Vector2.zero;
            feedback.rectTransform.offsetMax = Vector2.zero;

            GameObject flow = new GameObject(
                "Save Load Flow",
                typeof(PeacelandSaveLoadSceneController),
                typeof(PeacelandSceneCheckpointPolicy));
            PeacelandSaveLoadSceneController controller = flow.GetComponent<PeacelandSaveLoadSceneController>();
            SerializedObject controllerObject = new SerializedObject(controller);
            controllerObject.FindProperty("slotPanel").objectReferenceValue = slotPanel;
            controllerObject.FindProperty("checkpointPanel").objectReferenceValue = checkpointPanel;
            controllerObject.FindProperty("backButton").objectReferenceValue = backButton;
            controllerObject.FindProperty("feedbackText").objectReferenceValue = feedback;
            controllerObject.FindProperty("returnSceneName").stringValue = "DemoStart";
            controllerObject.FindProperty("firstGameplaySceneName").stringValue = "DemoDisclaimer";
            controllerObject.FindProperty("loadGameplaySceneAfterSelection").boolValue = true;
            controllerObject.FindProperty("clearActiveSlotOnOpen").boolValue = true;
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject policyObject = new SerializedObject(flow.GetComponent<PeacelandSceneCheckpointPolicy>());
            policyObject.FindProperty("recordAsGameplayCheckpoint").boolValue = false;
            policyObject.FindProperty("exclusionReason").stringValue =
                "Navigation-only save selection scene.";
            policyObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureInBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Save/load scene authored at " + ScenePath);
        }

        private static PeacelandCheckpointListPanel CreateCheckpointPanel(Transform parent)
        {
            Image panelImage = CreateImage(
                parent,
                "Checkpoint Panel",
                new Color(0.10f, 0.075f, 0.045f, 0.94f));
            panelImage.raycastTarget = true;
            RectTransform panelRect = panelImage.rectTransform;
            panelRect.anchorMin = new Vector2(0f, 0.12f);
            panelRect.anchorMax = new Vector2(1f, 0.88f);
            panelRect.offsetMin = new Vector2(8f, 0f);
            panelRect.offsetMax = new Vector2(-8f, 0f);

            TMP_Text heading = CreateText(
                panelRect,
                "Checkpoint Heading",
                "CHECKPOINTS",
                24f,
                FontStyles.Bold,
                TextAlignmentOptions.Left);
            heading.color = new Color(0.94f, 0.9f, 0.82f, 1f);
            heading.rectTransform.anchorMin = new Vector2(0f, 1f);
            heading.rectTransform.anchorMax = new Vector2(1f, 1f);
            heading.rectTransform.pivot = new Vector2(0.5f, 1f);
            heading.rectTransform.anchoredPosition = new Vector2(-12f, -14f);
            heading.rectTransform.sizeDelta = new Vector2(-92f, 44f);

            Button close = CreateButton(
                panelRect,
                "Close Checkpoints",
                "X",
                new Color(0.30f, 0.12f, 0.10f, 0.9f));
            SetAnchoredRect(
                close.transform as RectTransform,
                Vector2.one,
                Vector2.one,
                new Vector2(-12f, -12f),
                new Vector2(48f, 42f),
                Vector2.one);
            TMP_Text closeLabel = close.GetComponentInChildren<TMP_Text>();
            closeLabel.fontSize = 22f;
            closeLabel.fontSizeMax = 22f;

            RectTransform scrollRoot = CreateRect(panelRect, "Checkpoint Scroll");
            scrollRoot.anchorMin = new Vector2(0f, 0f);
            scrollRoot.anchorMax = new Vector2(1f, 1f);
            scrollRoot.offsetMin = new Vector2(16f, 16f);
            scrollRoot.offsetMax = new Vector2(-16f, -66f);
            ScrollRect scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            RectTransform viewport = CreateRect(scrollRoot, "Viewport");
            Stretch(viewport);
            Image viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            RectTransform content = CreateRect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            content.gameObject.AddComponent<GridLayoutGroup>();
            PeacelandResponsiveSaveGrid responsive =
                content.gameObject.AddComponent<PeacelandResponsiveSaveGrid>();
            SerializedObject gridObject = new SerializedObject(responsive);
            gridObject.FindProperty("columns").intValue = 2;
            gridObject.FindProperty("cardHeight").floatValue = 96f;
            gridObject.FindProperty("horizontalSpacing").floatValue = 18f;
            gridObject.FindProperty("verticalSpacing").floatValue = 10f;
            gridObject.FindProperty("horizontalPadding").floatValue = 2f;
            gridObject.ApplyModifiedPropertiesWithoutUndo();
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;

            PeacelandCheckpointEntryView template =
                CreateCheckpointEntry(content);
            PeacelandCheckpointListPanel panel =
                panelRect.gameObject.AddComponent<PeacelandCheckpointListPanel>();
            SerializedObject panelObject = new SerializedObject(panel);
            panelObject.FindProperty("root").objectReferenceValue = panelRect.gameObject;
            panelObject.FindProperty("headingText").objectReferenceValue = heading;
            panelObject.FindProperty("closeButton").objectReferenceValue = close;
            panelObject.FindProperty("entryContainer").objectReferenceValue = content;
            panelObject.FindProperty("entryTemplate").objectReferenceValue = template;
            panelObject.ApplyModifiedPropertiesWithoutUndo();
            panelRect.gameObject.SetActive(false);
            return panel;
        }

        private static PeacelandCheckpointEntryView CreateCheckpointEntry(Transform parent)
        {
            GameObject card = new GameObject(
                "Checkpoint 1",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement),
                typeof(PeacelandCheckpointEntryView));
            card.transform.SetParent(parent, false);
            LayoutElement layout = card.GetComponent<LayoutElement>();
            layout.minHeight = 86f;
            layout.preferredHeight = 96f;

            Image background = card.GetComponent<Image>();
            background.color = new Color(0.90f, 0.84f, 0.72f, 1f);
            Button button = card.GetComponent<Button>();
            button.targetGraphic = background;

            TMP_Text title = CreateText(
                card.transform,
                "Title",
                "Checkpoint",
                20f,
                FontStyles.Bold,
                TextAlignmentOptions.TopLeft);
            title.rectTransform.anchorMin = new Vector2(0f, 0.48f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.offsetMin = new Vector2(14f, 0f);
            title.rectTransform.offsetMax = new Vector2(-74f, -8f);

            TMP_Text detail = CreateText(
                card.transform,
                "Detail",
                "Scene\nTime",
                15f,
                FontStyles.Normal,
                TextAlignmentOptions.BottomLeft);
            detail.rectTransform.anchorMin = Vector2.zero;
            detail.rectTransform.anchorMax = new Vector2(1f, 0.58f);
            detail.rectTransform.offsetMin = new Vector2(14f, 8f);
            detail.rectTransform.offsetMax = new Vector2(-14f, 0f);

            TMP_Text active = CreateText(
                card.transform,
                "Active Marker",
                "ACTIVE",
                13f,
                FontStyles.Bold,
                TextAlignmentOptions.TopRight);
            active.color = new Color(0.35f, 0.15f, 0.08f, 1f);
            active.rectTransform.anchorMin = new Vector2(0.72f, 0.62f);
            active.rectTransform.anchorMax = Vector2.one;
            active.rectTransform.offsetMin = Vector2.zero;
            active.rectTransform.offsetMax = new Vector2(-12f, -10f);

            PeacelandCheckpointEntryView view =
                card.GetComponent<PeacelandCheckpointEntryView>();
            SerializedObject viewObject = new SerializedObject(view);
            viewObject.FindProperty("selectButton").objectReferenceValue = button;
            viewObject.FindProperty("titleText").objectReferenceValue = title;
            viewObject.FindProperty("detailText").objectReferenceValue = detail;
            viewObject.FindProperty("activeMarker").objectReferenceValue = active.gameObject;
            viewObject.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static PeacelandSaveSlotEntryView CreateSlotEntry(Transform parent, int slotIndex)
        {
            GameObject card = new GameObject(
                "Save Slot " + (slotIndex + 1),
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(CanvasGroup),
                typeof(LayoutElement),
                typeof(PeacelandSaveSlotEntryView));
            card.transform.SetParent(parent, false);

            LayoutElement layout = card.GetComponent<LayoutElement>();
            layout.flexibleHeight = 1f;
            layout.minHeight = 108f;
            layout.preferredHeight = 128f;

            Image background = card.GetComponent<Image>();
            background.color = new Color32(217, 217, 217, 255);
            background.raycastTarget = true;

            Button button = card.GetComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            TMP_Text title = CreateText(card.transform, "Save Number", "Save " + (slotIndex + 1), 18f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            title.rectTransform.anchorMin = Vector2.zero;
            title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(14f, 8f);
            title.rectTransform.offsetMax = new Vector2(-14f, -8f);

            TMP_Text location = CreateText(card.transform, "Location", "No data", 27f, FontStyles.Bold, TextAlignmentOptions.Center);
            location.rectTransform.anchorMin = new Vector2(0.08f, 0.2f);
            location.rectTransform.anchorMax = new Vector2(0.92f, 0.82f);
            location.rectTransform.offsetMin = Vector2.zero;
            location.rectTransform.offsetMax = Vector2.zero;

            TMP_Text detail = CreateText(
                card.transform,
                "Detail",
                "Scene | Stats | Notebook | Checkpoints",
                14f,
                FontStyles.Normal,
                TextAlignmentOptions.BottomLeft);
            detail.rectTransform.anchorMin = Vector2.zero;
            detail.rectTransform.anchorMax = new Vector2(0.82f, 0.32f);
            detail.rectTransform.offsetMin = new Vector2(14f, 8f);
            detail.rectTransform.offsetMax = Vector2.zero;

            TMP_Text day = CreateText(card.transform, "Day", string.Empty, 18f, FontStyles.Bold, TextAlignmentOptions.BottomRight);
            day.rectTransform.anchorMin = Vector2.zero;
            day.rectTransform.anchorMax = Vector2.one;
            day.rectTransform.offsetMin = new Vector2(14f, 8f);
            day.rectTransform.offsetMax = new Vector2(-14f, -8f);

            Button delete = CreateButton(card.transform, "Delete Button", "X", new Color(0.12f, 0.08f, 0.08f, 0.72f));
            SetAnchoredRect(
                delete.transform as RectTransform,
                Vector2.one,
                Vector2.one,
                new Vector2(-8f, -8f),
                new Vector2(34f, 34f),
                Vector2.one);
            TMP_Text deleteLabel = delete.GetComponentInChildren<TMP_Text>();
            deleteLabel.color = new Color(1f, 0.86f, 0.82f, 1f);
            deleteLabel.fontSize = 20f;
            deleteLabel.fontSizeMin = 10f;
            deleteLabel.fontSizeMax = 20f;

            PeacelandSaveSlotEntryView view = card.GetComponent<PeacelandSaveSlotEntryView>();
            SerializedObject viewObject = new SerializedObject(view);
            viewObject.FindProperty("slotIndex").intValue = slotIndex;
            viewObject.FindProperty("selectButton").objectReferenceValue = button;
            viewObject.FindProperty("deleteButton").objectReferenceValue = delete;
            viewObject.FindProperty("titleText").objectReferenceValue = title;
            viewObject.FindProperty("detailText").objectReferenceValue = detail;
            viewObject.FindProperty("locationText").objectReferenceValue = location;
            viewObject.FindProperty("dayText").objectReferenceValue = day;
            viewObject.FindProperty("slotBackground").objectReferenceValue = background;
            viewObject.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static void CreateBackground(RectTransform parent)
        {
            GameObject backgroundObject = new GameObject("Memory Tree Background", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            backgroundObject.transform.SetParent(parent, false);
            RectTransform rect = backgroundObject.GetComponent<RectTransform>();
            Stretch(rect);

            RawImage image = backgroundObject.GetComponent<RawImage>();
            image.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
            image.uvRect = new Rect(0f, 0f, 1f, 1f);
            image.raycastTarget = false;

            AspectRatioFitter fitter = backgroundObject.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            Texture texture = image.texture;
            fitter.aspectRatio = texture != null && texture.height > 0
                ? (float)texture.width / texture.height
                : 16f / 9f;
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject(
                "Save Load Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateCameraAndLight()
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);

            GameObject lightObject = new GameObject("Directional Light", typeof(Light));
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0f;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Button CreateButton(Transform parent, string name, string label, Color color)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            TMP_Text text = CreateText(buttonObject.transform, "Label", label, 48f, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
            return button;
        }

        private static TMP_Text CreateText(Transform parent, string name, string value, float size, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.black;
            text.raycastTarget = false;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(12f, size * 0.6f);
            text.fontSizeMax = size;
            return text;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.transform as RectTransform;
        }

        private static void SetAnchoredRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureSceneFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Peaceland/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/Peaceland", "Scenes");
            }
        }

        private static void EnsureInBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene existing in scenes)
            {
                if (existing.path == ScenePath)
                {
                    return;
                }
            }

            EditorBuildSettingsScene[] expanded = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(expanded, 0);
            expanded[expanded.Length - 1] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = expanded;
        }
    }
}
#endif
