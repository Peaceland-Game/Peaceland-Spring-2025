# Notebook Chain Test Plan

Use this plan when validating notebook, save/load, and the record-choice minigame as one complete gameplay chain.

## Scope

Validate that notebook content is collectable, readable, review-gated, saved, loaded, and connected to hidden stat consequences.

Primary scene:

- `Assets/Notebook/Scenes/NoteBookTesting.unity`

Satellite collect scenes:

- `Assets/Notebook/Scenes/NotebookTest_FloristMinigame.unity`
- `Assets/Notebook/Scenes/NotebookTest_FloristItemCollect.unity`
- `Assets/Notebook/Scenes/NotebookTest_RandJItemCollect.unity`
- `Assets/Notebook/Scenes/NotebookTest_IntroNewspaper.unity`

## Feature Map

| Feature | User entry point | Implementation |
|---|---|---|
| Open notebook | HUD `Notebook` button in `NoteBookTesting` | `NotebookController.OpenNotebook` |
| Directory and page navigation | Directory lines, bookmark tabs, page edge buttons | `NotebookController.RenderCurrentSpread` |
| Scene collection | Click collect objects in satellite scenes | `NotebookCollectTrigger.Collect` |
| Cross-scene collection | Collect without an active notebook controller | `NotebookGlobalBridge`, `NotebookSaveUtility` |
| Reviewed/new state | Viewing pages and leaving the spread | `NotebookEntryView.Bind`, `NotebookController.FinalizePendingReview` |
| Record-choice minigame | Choice buttons inside a required-choice note | `NotebookEntryView.BindRecordChoice`, `NotebookController.HandleRecordChoiceSelected` |
| Hidden stat consequence | Record-choice stat delta | `PeacelandStatManager.AddDelta` |
| Notebook save/load | Save service or PlayerPrefs fallback | `NotebookController.SaveState`, `NotebookController.LoadState` |
| Full game save/load | Runtime content harness `Save game` / `Load game` | `PeacelandSaveService` |

## Preflight Checks

1. Open Unity with project `D:/Peaceland/peaceland_Yu_Edit`.
2. Confirm the Console has no compile errors.
3. Run `Peaceland > Notebook > Harness > Run Full Harness`.
4. Open `Assets/Notebook/Harness/Reports/last_harness_report.md`.
5. Pass condition: `Result: PASS` and `0 errors`.
6. Warnings are acceptable only if they are documented non-gameplay orphan entries.

## Clean-State Setup

1. Open `Assets/Notebook/Scenes/NoteBookTesting.unity`.
2. Enter Play Mode.
3. Open the notebook.
4. Expand `Content harness`.
5. Click `Clear notebook save`.
6. Close and reopen the notebook.
7. Confirm no gameplay entries are already collected, except pagination dummy entries used by the test scene.

Do not rely only on the top playtest bar `Clear Save` button for full validation. That button clears the notebook PlayerPrefs key, while `Clear notebook save` uses the active notebook controller and save service path.

## Chain A: Basic Notebook UI

1. Click the HUD `Notebook` button.
2. Confirm the book opens with animation and the book content is hidden until the animation completes.
3. Confirm the main directory shows `Present`, `Memory 1`, and `Memory 2`.
4. Click each directory line and confirm it opens the matching section directory.
5. Click each bookmark tab and confirm it opens the matching section directory.
6. Use `<` and `>` page edge buttons across several spreads.
7. Confirm page numbers update and layout does not overlap.
8. Confirm bookmark tabs switch sides without vertical drift.

Pass condition: navigation is stable, page numbers match spreads, and no UI element overlaps enough to block reading or clicking.

## Chain B: Normal Collection And Review

1. Use the top playtest bar to load `Newspaper`.
2. Click the newspaper collect object.
3. Return to `Home`.
4. Open notebook.
5. Confirm `NotebookEntry_PresentNewspaper` appears under `Present`.
6. Confirm it is marked new before reading.
7. View the entry, leave the spread, then return to it.
8. Confirm the new highlight is cleared.
9. Repeat the same collect/read cycle for:
   - `Florist Collect`: five `NotebookEntry_FloristFlower_*` entries in `Memory 1`.
   - `R&J Collect`: `NotebookEntry_Memory2RJBalcony` and `NotebookEntry_Memory2RJLetter` in `Memory 2`.

Pass condition: each collect object unlocks the expected entry once, duplicate collection does not add duplicates, and viewed entries stop being new.

## Chain C: Florist Minigame Collection

1. From `Home`, use the top playtest bar to load `Florist MG`.
2. Click `Minigame Complete Button`.
3. Return to `Home`.
4. Open notebook.
5. Navigate to `Memory 1`.
6. Confirm `NotebookEntry_Memory1FloristFlower` is present.

Pass condition: the UI button triggers collection without needing a physics collider or `Physics2DRaycaster`.

## Chain D: Record-Choice Gate

1. Open `NotebookEntry_Memory1FloristFlower`.
2. Do not choose any record-choice option yet.
3. Leave the entry spread by flipping page, changing section, or closing the notebook.
4. Return to the same entry.
5. Confirm the record-choice buttons are still visible.
6. Confirm the entry still behaves as unresolved/new until a choice is made.

Pass condition: a required-choice entry is not finalized as reviewed before the player selects a choice.

## Chain E: Record-Choice Consequence

1. Note the current hidden stat values from the playtest bar or `Content harness > Log all stats`.
2. Select one record-choice option in `NotebookEntry_Memory1FloristFlower`.
3. Confirm the entry changes to a recorded state and shows `Recorded: ...`.
4. Confirm the expected hidden stat changed by the option's configured delta.
5. Close and reopen the entry.
6. Confirm choice buttons do not reappear.
7. Try to interact with the recorded entry again.

Pass condition: the selected choice is displayed, the entry is reviewed, and the hidden stat delta is applied exactly once.

## Chain F: Save And Load

1. After completing Chains B through E, open `Content harness`.
2. Click `Save game`.
3. Switch to another notebook test scene with the top playtest bar.
4. Return to `Home`.
5. Click `Load game`.
6. Confirm all collected entries remain collected.
7. Confirm all reviewed entries remain reviewed.
8. Confirm the florist minigame recorded choice remains recorded.
9. Confirm the hidden stat value remains the post-choice value.
10. Exit Play Mode, enter Play Mode again, and repeat checks 6-9.

Pass condition: notebook state and hidden stat state survive scene changes, explicit load, and Play Mode restart.

## Regression Signals

Investigate immediately if any of these happen:

- A required-choice note becomes reviewed before a choice is selected.
- A recorded choice can be selected more than once.
- Hidden stats increase again after reopening a recorded note.
- A collect scene unlocks an entry but the entry appears in the wrong section.
- `Run Full Harness` reports any error.
- A UI Button collect trigger is reported as requiring a physics collider.
- A world collect trigger has no collider or cannot be clicked.

## Evidence To Capture

For each test pass, record:

- Unity date/time and branch.
- `last_harness_report.md` result.
- Which chains were manually tested.
- Any warnings kept as accepted.
- Any scene where click behavior failed.
