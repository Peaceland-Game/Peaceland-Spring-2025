# Minigame prefab usage

> Start with `Assets/HANDOFF_NOTEBOOK_SAVE_MINIGAMES.md`. It is the short version and is kept up to date; this page goes into more detail.

## Rhythm

Use `Assets/Prefabs/Rhythm/RhythmNarrativeSequence.prefab` for the complete
four-step example. Its Canvas, labels, beat container, sequence data, and four
beat references are visible and editable in Prefab Mode.

The four element prefabs are what the sequence spawns, one per gesture:

- `RhythmStoryTapBeat.prefab`
- `RhythmHoldBeat.prefab`
- `RhythmMultiTapBeat.prefab`
- `RhythmMoveBeat.prefab`

They start hidden and only appear when a sequence calls `BeginBeat`, so dropping
one into a scene on its own shows nothing. Use them through
`RhythmNarrativeSequence` (or your own script that calls `BeginBeat`).

Edit timing and gesture values on `RhythmBeatInteraction`. Edit narrative text,
step order, Yarn entry, and completion events on `RhythmNarrativeMinigame`.
Call `StartMinigame()` from the scene manager, dialogue hook, or UnityEvent.
The beat's own children (ring, label) are left as the Editor set them; only
sprites a prefab cannot store are filled in at runtime.

### Beats from Yarn

Put `RhythmYarnCommands` on the object that has the `DialogueRunner`, and keep
a `RhythmNarrativeSequence` in the scene. A script can then ask for a gesture
between any two lines:

```
Organizer: Stay with the pressure.
<<rhythm_beat Hold>>
Organizer: Again. Again. Again.
<<rhythm_beat MultiTap 1.2>>
```

Alternatively, type the node into the sequence's `Start Node`. `StartMinigame()`
then plays that node instead of the Steps list, and `onSequenceComplete` fires
when the node ends.

The dialogue waits until the beat lands; a miss only asks again. Pausing the
game (timeScale 0) pauses the beat as well. The second
argument is the lead time in seconds and is optional. Kinds are `StoryTap`,
`Hold`, `MultiTap`, `Move`.

### Sound

Each beat ticks a count-in across its lead and once more on the target, so the
player has a tempo to lock to. The ticks are generated; drop clips into
`Count In Clip` / `Target Clip` on the beat prefabs to replace them. Perfect,
Good and Miss sounds go on the beat's `onPerfect` / `onGood` / `onMiss` events.

### Feedback

The result line says early or late and by how many milliseconds, and names
what went wrong on a miss (`TOO EARLY`, `TOO LATE`, `LET GO EARLY`, `NO INPUT`).

### Commands

- `Peaceland > Rhythm > Create Prefab Bank` - regenerate the bank. This
  overwrites the prefabs and anything you changed on them, so it asks first.
- `Peaceland > Rhythm > Create Prefab Playtest Scene` / `Open Prefab Playtest` -
  the four-step example scene.
- `Peaceland > Rhythm > Validate Prefab Bank` - run before migration.
- `Peaceland > Rhythm > Create Yarn Playtest Scene` - writes a three-line
  script with two `<<rhythm_beat>>` commands and a scene that runs it
  (`Assets/Scenes/Rhythm/RhythmYarnPlaytest.unity`).
- `Peaceland > Rhythm > Run Beat Check (Play Mode)` - drives the prefab
  playtest scene with simulated input, locked to 60 fps, and fails if any of
  the four gestures, the early/late feedback, the retry or the hold timeout
  regress.
- `Peaceland > Rhythm > Run Yarn Command Check (Play Mode)` - runs the Yarn
  playtest script and fails if a miss lets the line move on or a landed beat
  does not.

Headless:

```
Unity.exe -batchmode -nographics -projectPath . -executeMethod RhythmPlayModeCheck.Run -logFile rhythm.log
Unity.exe -batchmode -nographics -projectPath . -executeMethod RhythmPlayModeCheck.RunYarn -logFile rhythm-yarn.log
```

Both call `Exit` themselves, so they take no `-quit`.

## Maze

Use `Assets/Prefabs/Maze/PF_Maze_MinigameRoot.prefab` for the complete sample
maze. It contains the level layout, start point, player, patrol NPC, events,
penalty cells, walls, and occluders. Keep the scene Camera outside this prefab and
add a `MazeCameraController2D` to it, or the camera will not follow the player.
`MazeGameBootstrap` finds both when the maze starts.

The root prefab does not contain a `MazeObjectiveSequence`; add one yourself
(see "Objectives" below). `Assets/Scenes/Maze/MazeSample.unity` has a complete,
wired example.

Use the element prefabs when building a different level:

- `PF_Maze_Player.prefab`
- `PF_Maze_PatrolNpc.prefab`
- `PF_Maze_EventTrigger.prefab`
- `PF_Maze_PenaltyCell.prefab`
- `PF_Maze_VisionOccluder.prefab`
- `PF_Maze_Wall.prefab`

Patrol points belong to the level instance. Assign them to `MazePatrolNpc2D`
in route order.

### Layers

The maze adds no Layers of its own. Anything solid goes on the project's
`Wall` Layer, which is what stops a step, a path and a sight line alike;
triggers stay on `Default` so they block none of the three. `MazeLayers` is
the only place that decision is written down, and the player, the NPC and the
vision cone all read it at Awake rather than trusting a serialized mask.

If the prefabs ever come back showing the wrong Layer, run
`Peaceland > Maze > Rebake Element Layers`.

### Objectives and being caught

`MazeObjectiveSequence` turns the maze into an errand list: it keeps exactly
one `MazeEventTrigger2D` armed at a time and moves to the next one after that
trigger's Yarn node finishes. List the same trigger twice with another in
between and the player has to walk back to it - that is the whole back and
forth.

Point a trigger at the `DialogueRunner` and node it should start; the sequence
waits for the node rather than for `onDialogueComplete`, which Yarn also raises
from `Stop()`.

Hook `MazePatrolNpc2D.onPlayerCaught` to
`MazeObjectiveSequence.ResetCurrentObjective`. Being seen is not instant: the
NPC's suspicion has to fill first, so breaking line of sight is always a way
out. When it does fill, the player goes back to the start point and the
objective they were walking to is re-armed - the walk is lost, the story is not.

### Running the maze from a memory manager

Put the whole maze (including the `MazeObjectiveSequence`) under one object and
switch it off. Add `MazeMinigame` somewhere else, drag the maze object into
`Maze Root`, and add the `MazeMinigame` to the manager's Minigames list.
`StartMinigame()` switches the maze on; when the last objective is done it
switches it off and calls `NextMinigame()` on the scene's manager.

Managers that advance on every `onDialogueComplete` (the Fall 26 R&J Present
and R&J Memory managers) would also advance on each objective's dialogue, so
run the maze in a scene whose manager does not do that.

### Commands

- `Peaceland > Maze > Create Prefab Bank From Prototype` - refresh the complete
  root from the prototype scene. Overwrites changes made to the prefabs, so it
  asks first.
- `Peaceland > Maze > Open Prefab Playtest` - open the prefab playtest scene.
- `Peaceland > Maze > Validate Prefab Bank` - run before migration.
- `Peaceland > Maze > Build Sample Level` - lay out
  `Assets/Scenes/Maze/MazeSample.unity`, a small level with a route, two
  objectives and a penalty cell.
- `Peaceland > Maze > Validate Sample Level` - fails if the Layers, the wiring
  or any leg of the errand breaks.
- `Peaceland > Maze > Run Sample Level Check (Play Mode)` - drives the sample
  level and fails if the vision cone stops working or the objectives stop
  taking turns.

The last two also run headless, which is how they are checked before handover:

```
Unity.exe -batchmode -quit -nographics -projectPath .   -executeMethod MazeSampleLevelBuilder.ValidateSampleLevel -logFile validate.log
Unity.exe -batchmode -nographics -projectPath .   -executeMethod MazePlayModeCheck.Run -logFile play.log
```

The Play Mode entry point calls `Exit` itself, so it takes no `-quit`.

## Scene boundary

Keep scene-specific Camera, Directional Light, dialogue objects, and global
GameManager outside both prefab banks. Configure story and gameplay through
Inspector fields and UnityEvents instead of adding scene-name checks to the
runtime scripts.
