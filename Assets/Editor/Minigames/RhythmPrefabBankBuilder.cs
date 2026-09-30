#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public static class RhythmPrefabBankBuilder
{
    // Every reusable rhythm asset lives in this one folder, so the Project window can treat it as a prefab bank.
    private const string Folder = "Assets/Prefabs/Rhythm";
    private const string PlaytestScene = "Assets/Scenes/Rhythm/RhythmPrefabPlaytest.unity";
    private const string YarnFolder = "Assets/Scenes/Rhythm/Yarn";
    private const string YarnPlaytestScene = "Assets/Scenes/Rhythm/RhythmYarnPlaytest.unity";
    public const string YarnPlaytestNode = "RhythmYarnPlaytest";

    [MenuItem("Peaceland/Rhythm/Create Prefab Bank")]
    public static void CreatePrefabBank()
    {
        // This menu command is idempotent: running it again overwrites the current template version of a prefab of the same name.
        EnsureFolder("Assets/Prefabs");
        EnsureFolder(Folder);

        RhythmBeatInteraction storyTap = CreateBeatPrefab(
            "RhythmStoryTapBeat",
            RhythmBeatKind.StoryTap,
            new Color(0.12f, 0.28f, 0.48f, 1f),
            0.05f,
            0.15f,
            0.75f,
            3,
            120f);
        RhythmBeatInteraction hold = CreateBeatPrefab(
            "RhythmHoldBeat",
            RhythmBeatKind.Hold,
            new Color(0.26f, 0.18f, 0.46f, 1f),
            0.05f,
            0.15f,
            0.8f,
            3,
            120f);
        RhythmBeatInteraction multiTap = CreateBeatPrefab(
            "RhythmMultiTapBeat",
            RhythmBeatKind.MultiTap,
            new Color(0.48f, 0.24f, 0.16f, 1f),
            0.05f,
            0.15f,
            0.75f,
            3,
            120f);
        RhythmBeatInteraction move = CreateBeatPrefab(
            "RhythmMoveBeat",
            RhythmBeatKind.Move,
            new Color(0.16f, 0.42f, 0.32f, 1f),
            0.05f,
            0.15f,
            0.75f,
            3,
            120f);

        GameObject sequenceObject = new GameObject(
            "RhythmNarrativeSequence",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(RhythmNarrativeMinigame));
        RhythmNarrativeMinigame sequence = sequenceObject.GetComponent<RhythmNarrativeMinigame>();
        Canvas canvas = sequenceObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = sequenceObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform beatContainer = CreateRect(
            "BeatContainer", sequenceObject.transform, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(260f, 260f));
        Text speakerLabel = CreateText(
            "Speaker", sequenceObject.transform, 30, new Vector2(0.5f, 1f),
            new Vector2(0f, -90f), new Vector2(900f, 50f));
        Text lineLabel = CreateText(
            "StoryLine", sequenceObject.transform, 24, new Vector2(0.5f, 1f),
            new Vector2(0f, -145f), new Vector2(1100f, 70f));
        Text resultLabel = CreateText(
            "Result", sequenceObject.transform, 30, new Vector2(0.5f, 0f),
            new Vector2(0f, 100f), new Vector2(800f, 60f));

        // Write the private Inspector fields through SerializedObject, rather than make runtime config public for the sake of an editor tool.
        SerializedObject serializedSequence = new SerializedObject(sequence);
        serializedSequence.FindProperty("storyTapPrefab").objectReferenceValue = storyTap;
        serializedSequence.FindProperty("holdPrefab").objectReferenceValue = hold;
        serializedSequence.FindProperty("multiTapPrefab").objectReferenceValue = multiTap;
        serializedSequence.FindProperty("movePrefab").objectReferenceValue = move;
        serializedSequence.FindProperty("beatContainer").objectReferenceValue = beatContainer;
        serializedSequence.FindProperty("speakerLabel").objectReferenceValue = speakerLabel;
        serializedSequence.FindProperty("lineLabel").objectReferenceValue = lineLabel;
        serializedSequence.FindProperty("resultLabel").objectReferenceValue = resultLabel;

        SerializedProperty steps = serializedSequence.FindProperty("steps");
        steps.arraySize = 4;
        SetStep(steps.GetArrayElementAtIndex(0), "THE ORGANIZER", "Listen first. Then answer.", RhythmBeatKind.StoryTap);
        SetStep(steps.GetArrayElementAtIndex(1), "THE ORGANIZER", "Stay with the pressure.", RhythmBeatKind.Hold);
        SetStep(steps.GetArrayElementAtIndex(2), "THE ORGANIZER", "Again. Again. Again.", RhythmBeatKind.MultiTap);
        SetStep(steps.GetArrayElementAtIndex(3), "THE ORGANIZER", "Move when the crowd moves.", RhythmBeatKind.Move);
        serializedSequence.ApplyModifiedPropertiesWithoutUndo();

        string sequencePath = Folder + "/RhythmNarrativeSequence.prefab";
        PrefabUtility.SaveAsPrefabAsset(sequenceObject, sequencePath);
        Object.DestroyImmediate(sequenceObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(sequencePath);
        Debug.Log("Rhythm prefab bank created at " + Folder);
    }

    [MenuItem("Peaceland/Rhythm/Create Prefab Playtest Scene")]
    public static void CreatePlaytestScene()
    {
        CreatePrefabBank();
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Scenes/Rhythm");

        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            // The beats tick; without a listener Unity complains every frame one plays.
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            SceneManager.MoveGameObjectToScene(cameraObject, scene);

            GameObject lightObject = new GameObject("Directional Light", typeof(Light));
            lightObject.GetComponent<Light>().type = LightType.Directional;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            SceneManager.MoveGameObjectToScene(lightObject, scene);

            GameObject eventSystem = new GameObject(
                "EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(eventSystem, scene);

            GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(
                Folder + "/RhythmNarrativeSequence.prefab");
            GameObject sequence = (GameObject)PrefabUtility.InstantiatePrefab(template, scene);
            SerializedObject serializedSequence = new SerializedObject(
                sequence.GetComponent<RhythmNarrativeMinigame>());
            serializedSequence.FindProperty("startOnAwake").boolValue = true;
            serializedSequence.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, PlaytestScene);
            Debug.Log("Rhythm prefab playtest scene created at " + PlaytestScene);
        }
        finally
        {
            if (scene.isLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    [MenuItem("Peaceland/Rhythm/Create Yarn Playtest Scene")]
    public static void CreateYarnPlaytestScene()
    {
        // The smallest scene in which <<rhythm_beat>> can be seen doing its job: a script
        // that asks for two gestures, a Yarn line view, and the sequence the beats spawn in.
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Scenes/Rhythm");
        EnsureFolder(YarnFolder);

        string yarnPath = YarnFolder + "/" + YarnPlaytestNode + ".yarn";
        string projectPath = YarnFolder + "/" + YarnPlaytestNode + ".yarnproject";
        System.IO.File.WriteAllText(yarnPath, string.Join("\n", new[]
        {
            "title: " + YarnPlaytestNode,
            "---",
            "Organizer: Listen first. Then answer.",
            "<<rhythm_beat StoryTap 0.4>>",
            "Organizer: Stay with the pressure.",
            "<<rhythm_beat Hold 0.4>>",
            "Organizer: That is the rhythm.",
            "===",
            ""
        }));
        System.IO.File.WriteAllText(projectPath, string.Join("\n", new[]
        {
            "{",
            "  \"projectFileVersion\": 2,",
            "  \"sourceFiles\": [ \"**/*.yarn\" ],",
            "  \"excludeFiles\": [ \"**/*~/*\" ],",
            "  \"localisation\": {},",
            "  \"baseLanguage\": \"en\",",
            "  \"compilerOptions\": {}",
            "}",
            ""
        }));
        AssetDatabase.ImportAsset(yarnPath, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(projectPath, ImportAssetOptions.ForceSynchronousImport);
        YarnProject project = AssetDatabase.LoadAssetAtPath<YarnProject>(projectPath);
        if (project == null)
        {
            throw new System.InvalidOperationException("Yarn did not import " + projectPath);
        }

        GameObject lineViewTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Packages/dev.yarnspinner.unity/Prefabs/Line View.prefab");
        GameObject sequenceTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(
            Folder + "/RhythmNarrativeSequence.prefab");
        if (lineViewTemplate == null || sequenceTemplate == null)
        {
            throw new System.InvalidOperationException(
                "Need the Yarn Spinner Line View prefab and the rhythm prefab bank first.");
        }

        // Single, not additive: batchmode starts on an untitled scene that additive refuses to sit beside.
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        SceneManager.MoveGameObjectToScene(cameraObject, scene);

        GameObject eventSystem = new GameObject(
            "EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(eventSystem, scene);

        GameObject canvasObject = new GameObject(
            "Dialogue Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        SceneManager.MoveGameObjectToScene(canvasObject, scene);
        GameObject lineViewObject = (GameObject)PrefabUtility.InstantiatePrefab(lineViewTemplate, scene);
        lineViewObject.transform.SetParent(canvasObject.transform, false);
        LineView lineView = lineViewObject.GetComponent<LineView>();
        // A line that waits for a click would stall a headless run; the beat itself is the wait.
        SerializedObject serializedLineView = new SerializedObject(lineView);
        serializedLineView.FindProperty("autoAdvance").boolValue = true;
        serializedLineView.FindProperty("holdTime").floatValue = 0.3f;
        serializedLineView.ApplyModifiedPropertiesWithoutUndo();

        GameObject sequence = (GameObject)PrefabUtility.InstantiatePrefab(sequenceTemplate, scene);
        RhythmNarrativeMinigame rhythm = sequence.GetComponent<RhythmNarrativeMinigame>();

        GameObject dialogueObject = new GameObject("Dialogue", typeof(DialogueRunner), typeof(InMemoryVariableStorage));
        SceneManager.MoveGameObjectToScene(dialogueObject, scene);
        DialogueRunner runner = dialogueObject.GetComponent<DialogueRunner>();
        runner.yarnProject = project;
        runner.startAutomatically = false;
        runner.startNode = YarnPlaytestNode;
        runner.dialogueViews = new DialogueViewBase[] { lineView };
        runner.SetProject(project);
        SerializedObject serializedRunner = new SerializedObject(runner);
        serializedRunner.FindProperty("_variableStorage").objectReferenceValue =
            dialogueObject.GetComponent<InMemoryVariableStorage>();
        serializedRunner.ApplyModifiedPropertiesWithoutUndo();

        RhythmYarnCommands commands = dialogueObject.AddComponent<RhythmYarnCommands>();
        SerializedObject serializedCommands = new SerializedObject(commands);
        serializedCommands.FindProperty("dialogueRunner").objectReferenceValue = runner;
        serializedCommands.FindProperty("rhythm").objectReferenceValue = rhythm;
        serializedCommands.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, YarnPlaytestScene);
        Debug.Log("Rhythm Yarn playtest scene created at " + YarnPlaytestScene);
    }

    [MenuItem("Peaceland/Rhythm/Validate Prefab Bank")]
    public static void ValidatePrefabBank()
    {
        string[] paths =
        {
            Folder + "/RhythmStoryTapBeat.prefab",
            Folder + "/RhythmHoldBeat.prefab",
            Folder + "/RhythmMultiTapBeat.prefab",
            Folder + "/RhythmMoveBeat.prefab",
            Folder + "/RhythmNarrativeSequence.prefab"
        };

        foreach (string path in paths)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                throw new System.InvalidOperationException("Missing " + path);
            }
        }

        GameObject sequenceObject = AssetDatabase.LoadAssetAtPath<GameObject>(paths[4]);
        RhythmNarrativeMinigame sequence =
            sequenceObject.GetComponent<RhythmNarrativeMinigame>();
        SerializedObject serializedSequence = new SerializedObject(sequence);
        string[] references =
        {
            "storyTapPrefab", "holdPrefab", "multiTapPrefab", "movePrefab",
            "beatContainer", "speakerLabel", "lineLabel", "resultLabel"
        };
        foreach (string reference in references)
        {
            if (serializedSequence.FindProperty(reference).objectReferenceValue == null)
            {
                throw new System.InvalidOperationException(
                    "Rhythm sequence is missing reference " + reference);
            }
        }

        Debug.Log("Rhythm prefab bank validation PASS: 4 beat elements + sequence root.");
    }

    [MenuItem("Peaceland/Rhythm/Open Prefab Playtest")]
    public static void OpenPlaytest()
    {
        EditorSceneManager.OpenScene(PlaytestScene, OpenSceneMode.Single);
    }

    private static RhythmBeatInteraction CreateBeatPrefab(
        string prefabName,
        RhythmBeatKind kind,
        Color bodyColor,
        float perfectWindow,
        float goodWindow,
        float holdDuration,
        int requiredTaps,
        float moveDistance)
    {
        // Beat prefabs stay light, holding the component and its base colours only. The ring and the label are made at runtime.
        GameObject beatObject = new GameObject(prefabName, typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(RhythmBeatInteraction));
        RectTransform rect = beatObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220f, 220f);
        Image image = beatObject.GetComponent<Image>();
        image.color = bodyColor;
        image.raycastTarget = true;

        RhythmBeatInteraction beat = beatObject.GetComponent<RhythmBeatInteraction>();
        Image ring = CreateImage("TimingRing", beatObject.transform);
        ring.color = new Color(0.31f, 0.77f, 1f, 0.75f);
        ring.raycastTarget = false;
        ring.transform.SetAsFirstSibling();
        Text label = CreateText(
            "Label", beatObject.transform, 28, new Vector2(0.5f, 0.5f),
            Vector2.zero, rect.sizeDelta);

        SerializedObject serializedBeat = new SerializedObject(beat);
        serializedBeat.FindProperty("beatKind").enumValueIndex = (int)kind;
        serializedBeat.FindProperty("perfectWindow").floatValue = perfectWindow;
        serializedBeat.FindProperty("goodWindow").floatValue = goodWindow;
        serializedBeat.FindProperty("holdDuration").floatValue = holdDuration;
        serializedBeat.FindProperty("requiredTaps").intValue = requiredTaps;
        serializedBeat.FindProperty("requiredMoveDistance").floatValue = moveDistance;
        serializedBeat.FindProperty("ringImage").objectReferenceValue = ring;
        serializedBeat.FindProperty("label").objectReferenceValue = label;
        serializedBeat.ApplyModifiedPropertiesWithoutUndo();

        string path = Folder + "/" + prefabName + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(beatObject, path);
        Object.DestroyImmediate(beatObject);
        return prefab.GetComponent<RhythmBeatInteraction>();
    }

    private static void SetStep(SerializedProperty step, string speaker, string line, RhythmBeatKind kind)
    {
        step.FindPropertyRelative("speaker").stringValue = speaker;
        step.FindPropertyRelative("line").stringValue = line;
        step.FindPropertyRelative("beatKind").enumValueIndex = (int)kind;
        step.FindPropertyRelative("leadTime").floatValue = 0.85f;
    }

    private static RectTransform CreateRect(
        string objectName,
        Transform parent,
        Vector2 anchor,
        Vector2 position,
        Vector2 size)
    {
        GameObject child = new GameObject(objectName, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image CreateImage(string objectName, Transform parent)
    {
        RectTransform rect = CreateRect(
            objectName, parent, new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(220f, 220f));
        return rect.gameObject.AddComponent<Image>();
    }

    private static Text CreateText(
        string objectName,
        Transform parent,
        int fontSize,
        Vector2 anchor,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rect = CreateRect(objectName, parent, anchor, position, size);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static void EnsureFolder(string path)
    {
        // AssetDatabase.CreateFolder makes one level at a time, so this method calls itself down the hierarchy.
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = path.Substring(0, path.LastIndexOf('/'));
        string folderName = path.Substring(path.LastIndexOf('/') + 1);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
#endif
