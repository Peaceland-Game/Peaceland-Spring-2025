#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Peaceland.Notebook.Editor
{
    /// <summary>
    /// Ensures and validates human-playable scene infrastructure (camera, input, collect wiring).
    /// </summary>
    public static class NotebookScenePlayabilityEditor
    {
        private const string HomeScenePath = "Assets/Notebook/Scenes/NoteBookTesting.unity";
        private const string BackgroundName = "Playtest Background";

        private static readonly string[] CollectScenePaths =
        {
            "Assets/Notebook/Scenes/NotebookTest_FloristItemCollect.unity",
            "Assets/Notebook/Scenes/NotebookTest_RandJItemCollect.unity",
        };

        [MenuItem("Peaceland/Notebook/Ensure Human Playable (Active Scene)")]
        public static void EnsureActiveSceneMenu()
        {
            EnsurePlayableActiveScene();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Human-playable basics ensured for active scene.");
        }

        [MenuItem("Peaceland/Notebook/Ensure Human Playable (All Test Scenes)")]
        public static void EnsureAllTestScenesMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string[] scenes =
            {
                HomeScenePath,
                "Assets/Notebook/Scenes/NotebookTest_FloristMinigame.unity",
                "Assets/Notebook/Scenes/NotebookTest_FloristItemCollect.unity",
                "Assets/Notebook/Scenes/NotebookTest_RandJItemCollect.unity",
                "Assets/Notebook/Scenes/NotebookTest_IntroNewspaper.unity",
            };

            string original = EditorSceneManager.GetActiveScene().path;
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorSceneManager.OpenScene(scenes[i], OpenSceneMode.Single);
                EnsurePlayableActiveScene();
                EditorSceneManager.SaveOpenScenes();
            }

            if (!string.IsNullOrWhiteSpace(original) && File.Exists(original))
            {
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            }

            Debug.Log("Human-playable basics ensured for all notebook test scenes.");
        }

        public static void EnsurePlayableActiveScene()
        {
            string path = EditorSceneManager.GetActiveScene().path.Replace('\\', '/');
            bool isHome = path.EndsWith(HomeScenePath);
            bool isCollect = IsCollectScene(path);

            EnsureMainCamera();
            EnsureEventSystem();
            EnsurePlaytestBackground();

            if (isHome)
            {
                EnsureHomeNotebookHud();
            }

            if (isCollect)
            {
                EnsureWorldCollectCamera();
            }
        }

        public static void ValidateActiveScene(NotebookHarnessReport report, string scenePath)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            bool isHome = scenePath.Replace('\\', '/').EndsWith(HomeScenePath);
            bool isCollect = IsCollectScene(scenePath);

            Camera camera = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "SCENE_NO_CAMERA", "Missing Main Camera.", sceneName);
            }
            else if (camera.GetComponent<AudioListener>() == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "SCENE_NO_AUDIO_LISTENER", "Main Camera missing AudioListener.", sceneName);
            }

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                report.Add(NotebookHarnessSeverity.Error, "SCENE_NO_EVENTSYSTEM", "Missing EventSystem.", sceneName);
            }

            if (isHome)
            {
                if (Object.FindFirstObjectByType<NotebookController>() == null)
                {
                    report.Add(NotebookHarnessSeverity.Error, "SCENE_NO_NOTEBOOK_CONTROLLER", "Home scene missing NotebookController.", sceneName);
                }

                if (GameObject.Find("Notebook HUD Button") == null)
                {
                    report.Add(NotebookHarnessSeverity.Warning, "SCENE_NO_HUD_BUTTON", "Home scene missing Notebook HUD Button (open book).", sceneName);
                }
            }

            if (isCollect)
            {
                NotebookCollectTrigger[] triggers = Object.FindObjectsByType<NotebookCollectTrigger>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                if (triggers == null || triggers.Length == 0)
                {
                    report.Add(NotebookHarnessSeverity.Error, "SCENE_NO_COLLECT_TRIGGER", "Collect scene has no NotebookCollectTrigger.", sceneName);
                }
                else if (RequiresPhysics2DRaycaster(triggers) && camera != null && camera.GetComponent<Physics2DRaycaster>() == null)
                {
                    report.Add(NotebookHarnessSeverity.Error, "SCENE_NO_RAYCASTER", "Collect scene camera missing Physics2DRaycaster.", sceneName);
                }
            }

            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
                {
                    report.Add(NotebookHarnessSeverity.Error, "SCENE_CANVAS_NO_CAMERA", "Screen Space Camera canvas has no worldCamera.", sceneName);
                }
            }
        }

        private static bool RequiresPhysics2DRaycaster(NotebookCollectTrigger[] triggers)
        {
            for (int i = 0; i < triggers.Length; i++)
            {
                NotebookCollectTrigger trigger = triggers[i];
                if (trigger != null && !IsUiCollectTrigger(trigger))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsUiCollectTrigger(NotebookCollectTrigger trigger)
        {
            return trigger != null
                && trigger.GetComponent<RectTransform>() != null
                && trigger.GetComponent<Graphic>() != null;
        }

        public static void ValidateAllTestScenes(NotebookHarnessReport report)
        {
            string original = EditorSceneManager.GetActiveScene().path;
            string[] scenes = NotebookHarnessEditor.GetTestScenePaths();

            for (int i = 0; i < scenes.Length; i++)
            {
                string scenePath = scenes[i];
                if (!File.Exists(scenePath))
                {
                    report.Add(NotebookHarnessSeverity.Error, "SCENE_MISSING", "Test scene file not found.", scenePath);
                    continue;
                }

                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                ValidateActiveScene(report, scenePath);
            }

            if (!string.IsNullOrWhiteSpace(original) && File.Exists(original))
            {
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            }
        }

        private static bool IsCollectScene(string path)
        {
            string normalized = path.Replace('\\', '/');
            for (int i = 0; i < CollectScenePaths.Length; i++)
            {
                if (normalized.EndsWith(CollectScenePaths[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static void EnsureMainCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = Object.FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
                camera.transform.position = new Vector3(0f, 0f, -10f);
            }

            if (camera.gameObject.tag != "MainCamera")
            {
                camera.gameObject.tag = "MainCamera";
            }

            if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static void EnsurePlaytestBackground()
        {
            if (GameObject.Find(BackgroundName) != null)
            {
                return;
            }

            GameObject background = new GameObject(BackgroundName);
            SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
            renderer.color = new Color(0.18f, 0.22f, 0.28f, 1f);
            renderer.sortingOrder = -100;

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            background.transform.localScale = new Vector3(40f, 40f, 1f);
        }

        private static void EnsureHomeNotebookHud()
        {
            if (GameObject.Find("Notebook HUD Button") != null)
            {
                return;
            }

            // Home authoring normally creates HUD; flag only if Author Open UI not run.
        }

        private static void EnsureWorldCollectCamera()
        {
            Camera camera = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            if (camera != null && camera.GetComponent<Physics2DRaycaster>() == null)
            {
                camera.gameObject.AddComponent<Physics2DRaycaster>();
            }
        }
    }
}
#endif
