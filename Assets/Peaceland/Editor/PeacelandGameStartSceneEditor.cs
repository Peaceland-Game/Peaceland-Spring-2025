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
    public static class PeacelandGameStartSceneEditor
    {
        public const string ScenePath = "Assets/Peaceland/Scenes/GameStart.unity";

        [MenuItem("Peaceland/Save/Author Game Start Scene")]
        public static void AuthorGameStartSceneMenu()
        {
            AuthorGameStartScene();
            EditorUtility.DisplayDialog(
                "Game Start Scene",
                "Created/updated " + ScenePath + ".\n\n"
                + "1. Open the scene and polish UI by hand.\n"
                + "2. Move GameStart to index 0 in Build Settings if it should boot first.\n"
                + "3. Press Play → 开始游戏 → pick a slot.",
                "OK");
        }

        [MenuItem("Peaceland/Save/Open Game Start Scene")]
        public static void OpenGameStartSceneMenu()
        {
            if (!File.Exists(ScenePath))
            {
                AuthorGameStartScene();
            }

            EditorSceneManager.OpenScene(ScenePath);
        }

        public static void AuthorGameStartScene()
        {
            EnsureSceneFolder();
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            EnsureMainCamera();
            EnsureEventSystem();

            Canvas canvas = EnsureCanvas();
            RectTransform canvasRect = canvas.transform as RectTransform;

            TMP_Text title = EnsureText(
                canvasRect,
                "Title Text",
                "Peaceland",
                72f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -120f),
                new Vector2(800f, 96f));

            Button startButton = EnsureButton(
                canvasRect,
                "Start Game Button",
                "开始游戏",
                new Color(0.22f, 0.36f, 0.58f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(320f, 72f));

            GameObject flowRoot = new GameObject("Game Start Flow", typeof(PeacelandGameStartController));
            PeacelandGameStartController controller = flowRoot.GetComponent<PeacelandGameStartController>();

            Transform panelRoot = EnsureChild(canvasRect, "Save Select Panel");
            Stretch(panelRoot as RectTransform);
            Image panelBackdrop = panelRoot.gameObject.AddComponent<Image>();
            panelBackdrop.color = new Color(0f, 0f, 0f, 0.72f);
            panelRoot.gameObject.SetActive(false);

            PeacelandSaveSlotPanel slotPanel = panelRoot.gameObject.AddComponent<PeacelandSaveSlotPanel>();

            Transform dialog = EnsureChild(panelRoot, "Dialog");
            RectTransform dialogRect = dialog as RectTransform;
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(760f, 620f);
            dialogRect.anchoredPosition = Vector2.zero;
            Image dialogImage = dialog.gameObject.AddComponent<Image>();
            dialogImage.color = new Color(0.96f, 0.93f, 0.86f, 0.98f);

            EnsureText(
                dialog,
                "Panel Title",
                "选择存档",
                40f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(24f, -72f),
                new Vector2(-24f, -16f));

            Button closeButton = EnsureButton(
                dialog,
                "Close Panel Button",
                "返回",
                new Color(0.45f, 0.42f, 0.38f, 1f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 36f),
                new Vector2(180f, 52f));

            Transform slotsRoot = EnsureChild(dialog, "Slot List");
            RectTransform slotsRect = slotsRoot as RectTransform;
            slotsRect.anchorMin = new Vector2(0f, 0f);
            slotsRect.anchorMax = new Vector2(1f, 1f);
            slotsRect.offsetMin = new Vector2(32f, 96f);
            slotsRect.offsetMax = new Vector2(-32f, -96f);
            VerticalLayoutGroup layout = slotsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            PeacelandSaveSlotEntryView[] entries = new PeacelandSaveSlotEntryView[PeacelandSaveSlots.SlotCount];
            for (int i = 0; i < PeacelandSaveSlots.SlotCount; i++)
            {
                entries[i] = CreateSlotEntry(slotsRoot, i);
            }

            SerializedObject controllerObject = new SerializedObject(controller);
            controllerObject.FindProperty("startGameButton").objectReferenceValue = startButton;
            controllerObject.FindProperty("closeSavePanelButton").objectReferenceValue = closeButton;
            controllerObject.FindProperty("saveSlotPanel").objectReferenceValue = slotPanel;
            controllerObject.FindProperty("firstGameplaySceneName").stringValue = "NoteBookTesting";
            controllerObject.FindProperty("resumeLastSceneWhenAvailable").boolValue = true;
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject panelObject = new SerializedObject(slotPanel);
            panelObject.FindProperty("root").objectReferenceValue = panelRoot.gameObject;
            panelObject.FindProperty("slotEntries").arraySize = PeacelandSaveSlots.SlotCount;
            for (int i = 0; i < PeacelandSaveSlots.SlotCount; i++)
            {
                panelObject.FindProperty("slotEntries").GetArrayElementAtIndex(i).objectReferenceValue = entries[i];
            }

            panelObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("Peaceland game start scene authored at " + ScenePath);
        }

        private static PeacelandSaveSlotEntryView CreateSlotEntry(Transform parent, int slotIndex)
        {
            GameObject row = new GameObject(
                "Save Slot " + (slotIndex + 1),
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(PeacelandSaveSlotEntryView),
                typeof(LayoutElement));

            row.transform.SetParent(parent, false);
            LayoutElement layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 128f;

            Image rowImage = row.GetComponent<Image>();
            rowImage.color = new Color(1f, 1f, 1f, 0.92f);

            Button selectButton = row.GetComponent<Button>();
            selectButton.targetGraphic = rowImage;

            RectTransform rowRect = row.transform as RectTransform;
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.sizeDelta = new Vector2(0f, 128f);

            TMP_Text title = EnsureText(
                row.transform,
                "Title",
                "存档 " + (slotIndex + 1),
                28f,
                FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(20f, -16f),
                new Vector2(-160f, -52f));

            TMP_Text status = EnsureText(
                row.transform,
                "Status",
                "空槽位",
                22f,
                FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(20f, -12f),
                new Vector2(-160f, 12f));

            TMP_Text detail = EnsureText(
                row.transform,
                "Detail",
                "—",
                18f,
                FontStyles.Italic,
                TextAlignmentOptions.BottomLeft,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(20f, 12f),
                new Vector2(-160f, 44f));

            Button deleteButton = EnsureButton(
                row.transform,
                "Delete Button",
                "删除",
                new Color(0.55f, 0.24f, 0.2f, 1f),
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-20f, 0f),
                new Vector2(120f, 44f));

            PeacelandSaveSlotEntryView entryView = row.GetComponent<PeacelandSaveSlotEntryView>();
            SerializedObject entryObject = new SerializedObject(entryView);
            entryObject.FindProperty("slotIndex").intValue = slotIndex;
            entryObject.FindProperty("selectButton").objectReferenceValue = selectButton;
            entryObject.FindProperty("deleteButton").objectReferenceValue = deleteButton;
            entryObject.FindProperty("titleText").objectReferenceValue = title;
            entryObject.FindProperty("statusText").objectReferenceValue = status;
            entryObject.FindProperty("detailText").objectReferenceValue = detail;
            entryObject.ApplyModifiedPropertiesWithoutUndo();
            return entryView;
        }

        private static void EnsureSceneFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Peaceland/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/Peaceland", "Scenes");
            }
        }

        private static void EnsureMainCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Canvas EnsureCanvas()
        {
            Canvas existing = Object.FindFirstObjectByType<Canvas>();
            if (existing != null)
            {
                return existing;
            }

            GameObject canvasObject = new GameObject(
                "Game Start Canvas",
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

        private static Transform EnsureChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing;
            }

            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TMP_Text EnsureText(
            Transform parent,
            string name,
            string text,
            float size,
            FontStyles style,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            Transform existing = parent.Find(name);
            GameObject textObject = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));

            if (existing == null)
            {
                textObject.transform.SetParent(parent, false);
            }

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            TMP_Text tmp = textObject.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = new Color(0.16f, 0.13f, 0.1f, 1f);
            return tmp;
        }

        private static Button EnsureButton(
            Transform parent,
            string name,
            string label,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            Transform existing = parent.Find(name);
            GameObject buttonObject = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));

            if (existing == null)
            {
                buttonObject.transform.SetParent(parent, false);
            }

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            TMP_Text labelText = EnsureText(
                buttonObject.transform,
                "Label",
                label,
                26f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            labelText.color = Color.white;
            return button;
        }
    }
}
#endif
