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
in route order. Keep blocking geometry on `MazePhysical` or `Wall`.

Refresh the complete root from the prototype with
`Peaceland > Maze > Create Prefab Bank From Prototype`.
Run `Peaceland > Maze > Validate Prefab Bank` before migration.
Run the create command once after importing into a project with different
custom Layer slots. The Builder keeps existing Layers, adds missing Maze
Layers, and rewrites the prefab masks by Layer name. Runtime bindings repeat
this lookup so the prefabs remain safe when Layer indices differ.

## Scene boundary

Keep scene-specific Camera, Directional Light, dialogue objects, and global
GameManager outside both prefab banks. Configure story and gameplay through
Inspector fields and UnityEvents instead of adding scene-name checks to the
runtime scripts.
