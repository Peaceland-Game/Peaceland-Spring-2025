# Notebook, Save, Maze and Rhythm: start here

Yu Ma, 10/7/2026. This is the one page to read first. The other .md files next to the code go deeper; where they disagree with this page, this page is right.

Everything here is Inspector work. You only need code if you want something the components don't do.

| System | Folder | What you'd use it for |
|---|---|---|
| Notebook | `Assets/Notebook` | Notes the player collects and rereads |
| Save + hidden stats | `Assets/Peaceland` | Save slots, checkpoints, Kindness/Insight stats |
| Maze | `Assets/Scripts/Minigames/Maze`, `Assets/Prefabs/Maze` | Top-down "walk there without being seen" errands |
| Rhythm | `Assets/Scripts/Minigames/Rhythm`, `Assets/Prefabs/Rhythm` | Tap/hold/drag beats during a tense conversation |

None of these are placed in the Fall26Demo scenes. Adding them there is up to whoever owns that scene.

---

## Notebook

### Make a note

1. Project window, `Assets/Notebook/Data`, right click, **Create > Peaceland > Notebook > Entry**.
2. Fill in **Entry Id** (unique, never change it after people have saves), **Section** (Present, Memory1, Memory2), **Title**, **Body Text**, and optionally **Image**.
3. Menu **Peaceland > Notebook > Sync Notebook Database From Assets**. A note that isn't in the database does nothing when collected.
4. Check with **Peaceland > Notebook > Harness > Validate Content**.

The Drive doc "How to Make a Notebook Note" walks through every field.

### Put the notebook in a scene

1. Drag `Assets/Notebook/Prefabs/NotebookProductionSceneUI.prefab` onto the scene root.
2. Make sure the scene has an EventSystem. If it doesn't: right click in the Hierarchy, **UI > Event System**.
3. Play. The notebook icon opens the book.

The save system starts itself in every scene, so nothing else is needed. Never put `NotebookTestSceneControls.prefab` in a real scene; it's for test scenes only.

### Let the player collect a note

Pick whichever matches how the player finds it:

- **Click an object in the world or a UI image.** Menu **Peaceland > Notebook > Add Collectible To Scene...**, pick the note, pick World Sprite, UI Button, or Attach To Selection, click Add. It adds the collider, EventSystem and camera raycaster that clicking needs.
- **Open a War Room description.** On the object with `ViewDescription`, add `NotebookCollectTrigger` (assign the note) and `NotebookCollectOnDescriptionViewed`. Both fill themselves in when added to the same object.
- **Finish a drag puzzle.** Add `NotebookCollectTrigger` and `NotebookCollectOnDragCompleted`. Drag the puzzle's manager into **Fall26 Source** if it is an `F26_DragManager` (the letter puzzle), or **Source** for the older `DragManager`.
- **From a Yarn line.** Add `NotebookYarnCommands` to the object that has the scene's `DialogueRunner` (the "Dialogue System" object). Then in Yarn:

  ```
  <<collect_note NotebookEntry_Memory1FloristFlower>>
  ```

  The argument is the note's Entry Id. Drag `NotebookDatabase` into its **Database** field and a misspelled id gives a warning instead of failing quietly.
- **From your own script.** `Peaceland.Notebook.NotebookGlobalBridge.CollectEntry("EntryId");` works with or without the book in the scene.

The Console prints `Notebook: '<object>' collected '<title>'.` whenever a click or adapter collects something new.

### Let the player interpret a note (optional)

On the note asset, turn on **Requires Record Choice**, write a prompt and at least two choices. Each choice moves one hidden stat by its **Stat Delta** (keep it to 1). The stat list only offers Kindness/Cruelty and Insight/Naivety.

---

## Save and hidden stats

- **Slots.** Ten save slots, one JSON file each. `Assets/Peaceland/Scenes/SaveLoad.unity` is the slot picker (menu **Peaceland > Save > Open Save Load Scene**).
- **Playing from the Editor.** If you press Play in a gameplay scene without picking a slot, the game uses slot 0 on your machine. Things you collected last time stay collected. **Peaceland > Save > Reset Active Slot Progress** clears it.
- **Scene checkpoints.** A scene with a `PeacelandSceneCheckpointPolicy` becomes the "Continue" point when it loads. Add one to a scene by hand (Add Component). Turn off **Record As Gameplay Checkpoint** on menus and test scenes. Avoid **Peaceland > Save > Configure Enabled Scene Checkpoints** unless the team agrees: it opens and saves every scene in Build Settings.
- **Save points inside a memory.** Put `PeacelandMinigameProgressBridge` on the memory manager's object and give it a **Progress Key** (any short name, unique per scene). Then add a `PeacelandMinigameCheckpointMilestone`, point it at the bridge, and drag in the minigame that should save when it starts. The first minigame in the list works too.
- **Hidden stats from Yarn.** Add `YarnStatCommands` to the Dialogue System object, then `<<add_stat KindnessCruelty 1>>` or `<<add_stat InsightNaivety -1>>`. The value stays between -5 and +5 and is saved.

Heads up: the demo ending reads `F26_GameManager.KnowledgeStat` (`<<changeStat 0 n>>`), not these two stats. They don't talk to each other yet.

---

## Maze

### Try it

**Peaceland > Maze > Build Sample Level** makes `Assets/Scenes/Maze/MazeSample.unity` with a route, two objectives and a guard. Press Play and click where the player should go; they walk there around the walls.

### Build your own level

1. Start from `PF_Maze_MinigameRoot.prefab` or the sample scene. Pieces: `PF_Maze_Player`, `PF_Maze_Wall`, `PF_Maze_PatrolNpc`, `PF_Maze_EventTrigger`, `PF_Maze_PenaltyCell`, `PF_Maze_VisionOccluder`.
2. Put everything on whole-number positions. The grid is 1 unit.
3. The scene camera needs a `MazeCameraController2D` component, or it won't follow the player. Keep the camera outside the maze root.
4. Add an empty object with `MazeObjectiveSequence` (inside the maze root, if you will use step "Run it as a step" below). List the `MazeEventTrigger2D` objectives in the order the player should reach them. The same trigger can appear twice.
5. On each trigger, type the Yarn node to play into **Start Node**.
6. On each guard, add patrol point objects to **Patrol Points**, and hook **On Player Caught** to `MazeObjectiveSequence.ResetCurrentObjective`.

Getting caught sends the player back to the start and re-arms the objective they were walking to. Guards don't catch anyone while dialogue is playing.

### Run it as a step in a memory

1. Put the whole maze under one object and **switch that object off**.
2. On another object add `MazeMinigame`, drag the maze object into **Maze Root**.
3. Add that `MazeMinigame` to the memory manager's **Minigames** list where it should happen.

When the last objective is done, the maze hides and the manager moves to the next minigame.

Careful in scenes whose manager calls `NextMinigame` every time any dialogue ends (the R&J Present and R&J Memory managers do). There, every maze objective's dialogue would also push the memory forward. Give the maze its own scene, or leave **Start Node** empty on the triggers in those scenes.

---

## Rhythm

### Try it

**Peaceland > Rhythm > Open Prefab Playtest**, press Play. Four story lines, one gesture each: tap, hold, multi tap, drag.

### Two ways to use it

- **Steps list.** Drop `Assets/Prefabs/Rhythm/RhythmNarrativeSequence.prefab` into the scene, edit its **Steps** (speaker, line, gesture), and add it to the memory manager's **Minigames** list. Hook **On Sequence Complete** to whatever comes next.
- **Inside a Yarn script.** Keep `RhythmNarrativeSequence` in the scene, add `RhythmYarnCommands` to the Dialogue System object, then write beats between lines:

  ```
  Damir: Are you looking for something?
  <<rhythm_beat Hold>>
  Jovan: Not really. Just a pen.
  <<rhythm_beat MultiTap 1.2>>
  ```

  Kinds are `StoryTap`, `Hold`, `MultiTap`, `Move`. The number is optional: seconds of warning before the hit. The line waits until the beat lands; a miss just asks again.

  Or type the node into the sequence's **Start Node**: then the minigame plays that node and counts as finished when the node ends. The same "any dialogue ending moves the memory on" warning as the maze applies.

Timing windows, hold length and tap count are on each beat prefab (`RhythmBeatInteraction`). Pausing the game pauses the beat too.

The four beat prefabs on their own are invisible until a sequence starts them, so always use them through `RhythmNarrativeSequence`.

---

## Menus you don't need

**Create Prefab Bank** (Maze and Rhythm) rebuilds the prefabs from scratch and overwrites changes made to them; it asks first. Most other items under **Peaceland > Notebook** are authoring tools for the notebook's own test scenes; you can ignore them.

## Checking nothing broke

| Menu | What it checks |
|---|---|
| Peaceland > Notebook > Harness > Run Full Harness | Notes, database, collect wiring |
| Peaceland > Notebook > Harness > Verify Collectible Spawner | Add Collectible To Scene output |
| Peaceland > Save > Run Expanded Closed Loop (Play Mode) | Save, load, checkpoints |
| Peaceland > Save > Run R&J / Flower Internal Save Point Smoke (Play Mode) | Save points inside memories |
| Peaceland > Maze > Run Sample Level Check (Play Mode) | Vision cone, walls, objectives |
| Peaceland > Rhythm > Run Beat Check / Run Yarn Command Check (Play Mode) | All four gestures, retries, `<<rhythm_beat>>` |
| Peaceland > Save > Run Scene Play Smoke (Headless) | Every scene starts without errors |

These checks write to your local save slot 0. Copy `%USERPROFILE%\AppData\LocalLow\Peaceland\Peaceland` somewhere first if you care about it.

Command line, from the project folder (the Play Mode ones quit by themselves, so no `-quit`):

```
Unity.exe -batchmode -nographics -projectPath . -executeMethod Peaceland.Editor.PeacelandScenePlaySmoke.RunHeadless -logFile smoke.log
Unity.exe -batchmode -nographics -projectPath . -executeMethod MazePlayModeCheck.Run -logFile maze.log
Unity.exe -batchmode -nographics -projectPath . -executeMethod RhythmPlayModeCheck.Run -logFile rhythm.log
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod Peaceland.Notebook.Editor.NotebookHarnessEditor.RunHarnessBatch -logFile harness.log
```
