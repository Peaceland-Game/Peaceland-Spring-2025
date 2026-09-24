# Notebook Prefab Usage Guide

The notebook UI has exactly one source:

- `Assets/Notebook/Prefabs/NotebookProductionSceneUI.prefab`
- Test tooling: `Assets/Notebook/Prefabs/NotebookTestSceneControls.prefab`

Do not copy the notebook canvas into a scene by hand, and do not let a runtime script build the notebook UI.

## 1. Adding the notebook to a production scene

1. Open the scene.
2. Drag `NotebookProductionSceneUI.prefab` onto the scene root.
3. Check the scene has exactly one of each:
   - `NotebookController`
   - `NotebookUIShellReferences`
   - `NotebookOpenButton`
   - notebook canvas
4. If the scene has no `EventSystem`, add one, with the UI input module that matches the project's current input system.
5. Save, enter Play Mode, and check:
   - the notebook icon in the top left responds to a click
   - open, switch section, turn pages, close
   - leave the scene and come back: collected state is unchanged

There is also a menu item:

`Peaceland > Notebook > Author Open UI In Active Scene`

It only places and binds the prefab. It no longer generates UI.

## 2. Adding the notebook to a test scene

A test scene holds all three:

- `NotebookProductionSceneUI.prefab`
- `NotebookTestSceneControls.prefab`
- the scene's own real collectible, minigame or interaction adapter

Never put `NotebookTestSceneControls.prefab` in a production scene.

To prepare all five notebook test scenes at once:

`Peaceland > Notebook > Prefabs > Migrate All Notebook Test Scenes`

That command:

1. Rebuilds `NotebookTestSceneControls.prefab`.
2. Clears out the old bootstrap's generated UI.
3. Places the production UI prefab in each test scene.
4. Places the test controls prefab.
5. Binds the controller, the shell, the open button and the harness.
6. Saves the scene.

## 3. Changing how the notebook looks

Open `NotebookProductionSceneUI.prefab` in Prefab Mode and edit it there:

- notebook icon
- book background
- directory
- bookmark tabs
- entry template
- record choice panel
- page turn controls
- toast / notification

Do not repeat the same edit scene by scene. Keep scene overrides for:

- a genuine canvas sorting order clash
- a visibility difference that scene specifically needs
- small positional give and take with scene specific UI

If several scenes need the same override, apply it back to the prefab instead.

## 4. Adding a collectible note

1. Create or duplicate a `NotebookEntryDefinition` in `Assets/Notebook/Data`.
2. Give it a unique `entryId`.
3. Set its section, title, body, image and sort order.
4. Add the entry to `NotebookDatabase.asset`.
5. Add the matching notebook collectible or adapter to the real interactable object.
6. Reference the entry asset in the Inspector. Never hard code entry text in a script.
7. Check in Play Mode that:
   - it reads as uncollected before the interaction
   - a detected / updated toast appears after it
   - the notebook shows the right image, title and a body that reflows
   - interacting again does not add it twice

## 5. Collecting on minigame completion

A minigame scene keeps its own completion condition. On completion, the scene's adapter calls into the notebook:

1. The adapter references a `NotebookEntryDefinition`.
2. It collects only when the minigame is genuinely complete.
3. Minigame rules never live inside the notebook UI.
4. If the entry needs an interpretation, turn on its record choice configuration.
5. Confirm the choice panel appears the next time the notebook is opened.

Who owns what:

- **Minigame** decides it is complete.
- **Adapter** turns that into an entry collect.
- **Notebook** shows the awaiting interpretation state and stores the choice.
- **Stat system** applies the stat delta that choice carries.

## 6. Interpretations and stats

On the `NotebookEntryDefinition`:

1. Turn on the record choice option.
2. Configure two to four interpretation choices.
3. Give each a stable choice id.
4. Set the `PeacelandStatId` it moves, and by how much.
5. Never write a stat name or number into a scene button's code.

To test the whole loop:

1. Finish the minigame and collect.
2. Close the notebook and open it again.
3. Pick an interpretation.
4. Check the stat moved exactly once.
5. Save.
6. Change scene.
7. Load, or restart Play Mode.
8. Check the choice, the entry and the stat all came back.
9. Open it again: the delta must not apply a second time.

## 7. What `NotebookTestSceneBootstrap` does now

The type exists only for old scenes and serialized references. All it still does:

- find the existing prefab instance
- apply `NotebookUIShellReferences` to the `NotebookController`
- configure the `NotebookOpenButton`
- configure an optional `NotebookTestHarness`

It no longer:

- creates a canvas or an EventSystem
- creates buttons, TMP objects or the entry template
- creates entry assets or a database
- collects dummy entries
- repairs or rewrites RectTransforms

A new scene normally does not need this component at all.

## 8. Inspector debug checklist

- [ ] exactly one production notebook prefab instance in the scene
- [ ] no `NotebookTestHarness` in a production scene
- [ ] exactly one test controls prefab instance in a test scene
- [ ] `NotebookController.Database` is not empty
- [ ] the key references on `NotebookUIShellReferences` are not empty
- [ ] `NotebookOpenButton` points at the current controller
- [ ] no `Test Tools (Runtime)` object and no generated canvas
- [ ] console is clear of missing scripts, duplicate EventSystems and duplicate controllers
- [ ] at 16:9, at 16:10 and while resizing the window, the icon and toasts stay in the top left safe area
- [ ] entry body height follows the text, and the image does not squash it

## 9. Migration checklist

- [ ] run the migration menu on a duplicate first
- [ ] check all five notebook test scenes
- [ ] check the production prefab carries no test only component
- [ ] check the batch migration command left production scenes alone
- [ ] zero compile errors in the console
- [ ] run the content harness
- [ ] run the save / load closed loop
- [ ] work through the manual Play Mode checklist
- [ ] read the git diff before staging anything

## 10. Rolling back

When rolling back a file or a scene, restore only what you meant to and do not reset a whole dirty worktree. Duplicate the scene before migrating it, or make a local checkpoint commit first.
