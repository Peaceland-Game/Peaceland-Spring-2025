# Peaceland Save/Load

The project has a standalone 10-slot save scene at
`Assets/Peaceland/Scenes/SaveLoad.unity`. Its layout follows the Figma
`Saves-title` frame: Memory Tree background, two columns, and five cards per
column.

## Run the scene

1. Use `Peaceland/Save/Open Save Load Scene`.
2. Enter Play Mode.
3. Select any empty `Save N` card. The scene creates that slot and stays open.
4. Select another card to create or load a different slot.
5. Confirm the feedback line reports the selected save and notebook state count.

Occupied cards show an `X` button. Click it twice to confirm deletion. Drag an
occupied card onto an empty card to move the complete JSON save, including its
Notebook data. Moving onto an occupied card is rejected and never overwrites it.

The current test scene has `loadGameplaySceneAfterSelection` enabled so a slot
with a valid checkpoint immediately loads that scene. The Back button targets
`DemoStart`.

### Loading test slots

Use `Peaceland/Save/Seed Loading Test Slots` to fill only empty Save 2-4:

- Save 2 loads `NoteBookTesting`.
- Save 3 loads `NotebookTest_FloristMinigame`.
- Save 4 loads `NotebookTest_RandJItemCollect`.

Each test slot contains a different Notebook marker entry. The command never
overwrites an existing slot file.

## Per-slot Notebook contract

There are exactly ten files under `Application.persistentDataPath`:

```text
peaceland_save_slot_0.json
...
peaceland_save_slot_9.json
```

Each JSON document owns its own `notebook` object. Runtime code should use:

```csharp
PeacelandSaveService.Instance.TryGetNotebookSnapshot(slotIndex, out var notebook);
PeacelandSaveService.Instance.SaveNotebookToActiveSlot(notebook);
```

`TryGetNotebookSnapshot` returns a detached copy. Saving Notebook data requires
an active slot, so Notebook state cannot silently fall back to a shared global
file.

## Future gameplay connection

On `Save Load Flow`:

1. Set `loadGameplaySceneAfterSelection` to true.
2. Set `firstGameplaySceneName` to the first playable scene.
3. Keep gameplay calling `SetLastSceneName` and `SetSaveDisplayProgress` so the
   save cards can resume and display location/day.

The `PeacelandSceneCheckpointPolicy` on this scene prevents `SaveLoad` from
overwriting the last gameplay checkpoint.

## Validation

Run `Peaceland/Save/Validate Save Load Scene`. It checks the 10-slot contract,
unique persistent paths, independent Notebook JSON round-trips, and enabled
Build Settings registration. It does not read, modify, or delete player saves.

The project currently has a separate URP global-settings missing-type warning;
it is not produced by this save/load scene.
