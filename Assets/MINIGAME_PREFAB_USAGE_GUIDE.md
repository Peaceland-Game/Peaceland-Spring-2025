# Minigame prefab usage

## Rhythm

Use `Assets/Prefabs/Rhythm/RhythmNarrativeSequence.prefab` for the complete
four-step example. Its Canvas, labels, beat container, sequence data, and four
beat references are visible and editable in Prefab Mode.

The four element prefabs can also be used separately:

- `RhythmStoryTapBeat.prefab`
- `RhythmHoldBeat.prefab`
- `RhythmMultiTapBeat.prefab`
- `RhythmMoveBeat.prefab`

Edit timing and gesture values on `RhythmBeatInteraction`. Edit narrative text,
step order, Yarn entry, and completion events on `RhythmNarrativeMinigame`.
Call `StartMinigame()` from the scene manager, dialogue hook, or UnityEvent.

Regenerate the bank with `Peaceland > Rhythm > Create Prefab Bank`.

## Maze

Use `Assets/Prefabs/Maze/PF_Maze_MinigameRoot.prefab` for the complete sample
maze. It contains the level layout, start point, player, patrol NPC, events,
penalty cells, walls, and occluders. Keep the scene Camera outside this prefab;
`MazeGameBootstrap` resolves it when the scene starts.

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

### Commands

- `Peaceland > Maze > Create Prefab Bank From Prototype` - refresh the complete
  root from the prototype scene.
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
