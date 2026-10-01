# Notebook Manual Test Checklist

Run in `NoteBookTesting` unless noted. Clear save via **Test tools → Clear save** when you need a fresh state.

## Open / close

- [ ] Click HUD **Notebook** → open animation plays; **book pages/tabs/background stay hidden until animation finishes**
- [ ] Open animation scales from small to screen-center (ease curve, not a pop)
- [ ] After open: directory left page shows Present / Memory 1 / Memory 2; right page blank
- [ ] Close notebook → shell hides; reopen replays animation

## Navigation

- [ ] Directory **text lines** → section **directory** spread (index with page numbers)
- [ ] **Bookmark tabs** → section **directory** spread (section index)
- [ ] Bookmark tabs split left/right per current section (physical book rule)
- [ ] Index row click → jumps to entry content page
- [ ] **< / >** edge buttons flip spreads; page numbers update
- [ ] 20 dummy entries: flip through many pages without layout break

## Collect / review

- [ ] `NotebookTest_FloristItemCollect`: click glowing flowers → toast + hint
- [ ] Return to `NoteBookTesting` → florist entries in Memory 1, no re-collect
- [ ] `NotebookTest_IntroNewspaper`: click newspaper → Present entry unlocks
- [ ] `NotebookTest_RandJItemCollect`: two collectibles → Memory 2 entries
- [ ] `NotebookTest_FloristMinigame`: complete button → minigame entry unlocks
- [ ] NEW highlight clears after viewing that entry spread

## Layout (Editor)

- [ ] Adjust **Book Background** + **Content Root** rects to match art
- [ ] Enable **Lock Layout** on `NotebookBookArtLayout` → Play → layout does not snap back
- [ ] Sprites have no large transparent padding (run **Crop Notebook Art Sprites** — physically trims PNGs; clears 9-slice border that blocks Trim)

## Cross-scene consistency

- [ ] `NoteBookTesting` is the only scene with the complete readable Notebook shell
- [ ] Every Notebook test scene has exactly one direct-child `Notebook Overlay` under its Canvas
- [ ] Every Overlay has `Collectible Hint` and `Collected Toast` children
- [ ] Every `Collected Toast` uses `NotebookNotificationAnchor` with screen-safe placement enabled
- [ ] Run **Peaceland > Notebook > Editable Pack > Apply To All Notebook Test Scenes** after changing shared authoring
- [ ] Run **Peaceland > Notebook > Editable Pack > Run 5-Round Self Check (All Test Scenes)** and confirm zero consistency errors

## Known limitations (not bugs)

- Page turns use slide/fade, not physical page mesh flip
- Satellite scenes: HUD does not open full notebook (collect + return home to read)
- Default TMP font (handwriting font not chosen yet)
# Extended checks

Use `NOTEBOOK_CHAIN_TEST_PLAN.md` for the full linked test pass.

## Note interpretation minigame

- [ ] In `Data/NotebookEntry_Memory1FloristFlower.asset`, set `requiresRecordChoice: 1`
- [ ] Configure at least two `recordChoices` in the Inspector; each choice needs a stable ID, label, stat ID, and delta
- [ ] Collect the note in `NotebookTest_FloristMinigame`; the collect trigger should only unlock the note
- [ ] Return to `NoteBookTesting` and open the Notebook; the collected note shows its interpretation choices

- [ ] Open `NotebookEntry_Memory1FloristFlower` and leave without choosing; entry is not finalized reviewed
- [ ] Return to the same entry; record-choice buttons are still visible
- [ ] Select one record choice; entry shows recorded choice and becomes reviewed
- [ ] Hidden stat delta applies exactly once
- [ ] Reopening the entry does not apply the stat delta again
- [ ] Save/load after choosing; recorded choice, reviewed state, and hidden stat persist

## Save / load

- [ ] Use `Content harness` > `Clear notebook save` for a true clean notebook state
- [ ] Collect normal entries, click `Save game`, change scene, click `Load game`; collected/reviewed state persists
- [ ] Exit Play Mode and re-enter; notebook state still loads correctly
- [ ] Confirm `PeacelandSaveService` path and PlayerPrefs fallback are not confused during testing

## Harness

- [ ] Run `Peaceland > Notebook > Harness > Run Full Harness`
- [ ] Confirm `Assets/Notebook/Harness/Reports/last_harness_report.md` has `Result: PASS`
- [ ] Confirm the report has 0 errors
- [ ] Confirm accepted warnings are only non-gameplay orphan entries
