#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Peaceland.Notebook.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    // Editor-only check for the Add Collectible To Scene window. The window's button cannot be
    // pressed from a script, so this runs the same Spawn() call the button runs and asserts the
    // object it leaves behind. Everything happens in a throwaway additive scene.
    public static class NotebookCollectibleSpawnerHarness
    {
        [MenuItem("Peaceland/Notebook/Harness/Verify Collectible Spawner")]
        public static void Run()
        {
            NotebookEntryDefinition entry = FindAnyEntry();
            if (entry == null)
            {
                Debug.LogError("[SpawnerHarness] FAIL: no NotebookEntryDefinition asset in the project.");
                return;
            }

            Scene previousActive = SceneManager.GetActiveScene();
            GameObject previousSelection = Selection.activeGameObject;
            // Additive leaves the scene the user has open alone, but Unity refuses to add a scene
            // next to an untitled one, which is all a batchmode run ever starts with. Nothing is at
            // stake in that case, so replace it instead.
            bool additive = !string.IsNullOrEmpty(previousActive.path);
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                additive ? NewSceneMode.Additive : NewSceneMode.Single);
            if (additive)
            {
                SceneManager.SetActiveScene(scene);
            }

            Selection.activeGameObject = null;

            // The spawner reuses any Canvas already loaded, which may live in the scene the user
            // has open. Undo is what puts that scene back the way it was; closing the throwaway
            // scene only cleans up what landed inside it.
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();

            List<string> failures = new List<string>();
            try
            {
                CheckWorldSprite(entry, failures);
                CheckUIButton(entry, failures);
                CheckAttachToSelection(entry, failures);
            }
            finally
            {
                Undo.RevertAllDownToGroup(undoGroup);
                if (additive)
                {
                    SceneManager.SetActiveScene(previousActive);
                    EditorSceneManager.CloseScene(scene, true);
                }

                Selection.activeGameObject = previousSelection;
            }

            if (failures.Count == 0)
            {
                Debug.Log("[SpawnerHarness] PASS: all three spawn kinds produced a wired collectible.");
                return;
            }

            Debug.LogError("[SpawnerHarness] FAIL:\n - " + string.Join("\n - ", failures));
        }

        private static void CheckWorldSprite(NotebookEntryDefinition entry, List<string> failures)
        {
            GameObject spawned = NotebookCollectibleSpawner.Spawn(
                entry, NotebookCollectibleSpawner.SpawnKind.WorldSprite, null, true, true);

            if (spawned == null)
            {
                failures.Add("WorldSprite: Spawn returned null.");
                return;
            }

            Require(spawned.GetComponent<SpriteRenderer>() != null, "WorldSprite: no SpriteRenderer.", failures);
            Require(spawned.GetComponent<BoxCollider2D>() != null, "WorldSprite: no collider, clicks cannot land.", failures);
            Require(spawned.GetComponent<NotebookCollectableGlowView>() != null, "WorldSprite: glow was requested but not added.", failures);
            CheckTrigger(spawned, entry, expectPointerClick: false, expectMouseDown: true, label: "WorldSprite", failures: failures);
        }

        private static void CheckUIButton(NotebookEntryDefinition entry, List<string> failures)
        {
            GameObject spawned = NotebookCollectibleSpawner.Spawn(
                entry, NotebookCollectibleSpawner.SpawnKind.UIButton, null, true, true);

            if (spawned == null)
            {
                failures.Add("UIButton: Spawn returned null.");
                return;
            }

            Canvas canvas = spawned.GetComponentInParent<Canvas>();
            Require(canvas != null, "UIButton: not parented to a Canvas, so it never renders.", failures);
            if (canvas != null)
            {
                Require(canvas.GetComponent<GraphicRaycaster>() != null,
                    "UIButton: Canvas has no GraphicRaycaster, so clicks are never raycast.", failures);
            }

            Require(Object.FindFirstObjectByType<EventSystem>() != null,
                "UIButton: no EventSystem in the scene, so OnPointerClick never fires.", failures);
            Require(spawned.GetComponent<Image>() != null, "UIButton: no Image.", failures);
            CheckTrigger(spawned, entry, expectPointerClick: true, expectMouseDown: false, label: "UIButton", failures: failures);
        }

        private static void CheckAttachToSelection(NotebookEntryDefinition entry, List<string> failures)
        {
            GameObject host = new GameObject("ExistingArt", typeof(SpriteRenderer));
            Undo.RegisterCreatedObjectUndo(host, "Verify Collectible Spawner");
            GameObject spawned = NotebookCollectibleSpawner.Spawn(
                entry, NotebookCollectibleSpawner.SpawnKind.AttachToSelection, host, false, false);

            if (spawned != host)
            {
                failures.Add("AttachToSelection: did not return the object it was given.");
                return;
            }

            Require(host.GetComponents<NotebookCollectTrigger>().Length == 1,
                "AttachToSelection: expected exactly one trigger on the host.", failures);
            Require(host.GetComponent<NotebookCollectableGlowView>() == null,
                "AttachToSelection: glow was switched off but added anyway.", failures);

            // Running it twice must not stack duplicate entries on the same trigger.
            NotebookCollectibleSpawner.Spawn(
                entry, NotebookCollectibleSpawner.SpawnKind.AttachToSelection, host, false, false);
            SerializedProperty entries = new SerializedObject(host.GetComponent<NotebookCollectTrigger>())
                .FindProperty("entries");
            Require(entries.arraySize == 1,
                "AttachToSelection: spawning twice duplicated the entry (" + entries.arraySize + " items).", failures);
        }

        private static void CheckTrigger(
            GameObject spawned,
            NotebookEntryDefinition entry,
            bool expectPointerClick,
            bool expectMouseDown,
            string label,
            List<string> failures)
        {
            NotebookCollectTrigger trigger = spawned.GetComponent<NotebookCollectTrigger>();
            if (trigger == null)
            {
                failures.Add(label + ": no NotebookCollectTrigger.");
                return;
            }

            SerializedObject serialized = new SerializedObject(trigger);
            SerializedProperty entries = serialized.FindProperty("entries");
            bool wired = entries != null
                && Enumerable.Range(0, entries.arraySize)
                    .Any(i => entries.GetArrayElementAtIndex(i).objectReferenceValue == entry);

            Require(wired, label + ": the chosen entry was not written into the trigger.", failures);
            Require(serialized.FindProperty("collectOnPointerClick").boolValue == expectPointerClick,
                label + ": collectOnPointerClick should be " + expectPointerClick + ".", failures);
            Require(serialized.FindProperty("collectOnMouseDown").boolValue == expectMouseDown,
                label + ": collectOnMouseDown should be " + expectMouseDown + ".", failures);
        }

        private static void Require(bool condition, string message, List<string> failures)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        private static NotebookEntryDefinition FindAnyEntry()
        {
            return AssetDatabase.FindAssets("t:NotebookEntryDefinition")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<NotebookEntryDefinition>)
                .FirstOrDefault(item => item != null);
        }
    }
}
#endif
