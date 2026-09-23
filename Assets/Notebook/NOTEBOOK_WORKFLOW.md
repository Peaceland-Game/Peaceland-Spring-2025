# Notebook Workflow — Start Here

Use this file as the **only entry point** for day-to-day notebook work.  
Spec details: `NOTEBOOK_IMPLEMENTATION_PLAN.md` · QA: `NOTEBOOK_TEST_CHECKLIST.md` · Art: `NOTEBOOK_ARTWORK.md`

---

## Why it felt like a dead end

| Problem | What happened |
|--------|----------------|
| Layout keeps resetting | Opening `NoteBookTesting` used to auto-run **Author Open UI**, overwriting RectTransforms |
| Two “books” on screen | World `notebookOpened` SpriteRenderers + Canvas `Notebook Open Root` stacked |
| Tuning everywhere | Page height, bookmarks, viewport insets split across code, scene objects, and bootstrap |
| Docs scattered | README, plan, changelog, checklist all partially overlap |

**Exit:** one UI tree, one tuning component, explicit menus (no silent rebuild).

---

## Mental model (3 layers)

```
┌─────────────────────────────────────┐
│  DATA                               │
│  NotebookDatabase + Entry assets    │
│  (what appears in the book)       │
└──────────────┬──────────────────────┘
               ▼
┌─────────────────────────────────────┐
│  LAYOUT                             │
│  Notebook Open Root                 │
│  → NotebookBookArtLayout (numbers)  │
│  → Book Background / Content Root   │
│    (positions — lock when done)     │
└──────────────┬──────────────────────┘
               ▼
┌─────────────────────────────────────┐
│  RUNTIME                            │
│  NotebookController                 │
│  → builds spreads & pagination      │
│  → renders directory / index / body │
└─────────────────────────────────────┘
```

You should **not** need to edit pagination code to tune the book. Tune **Layout**, then Play.

---

## Golden rules

1. **One book UI:** `Notebook Canvas` → `Notebook Open Root` only. Delete root-level `notebookOpened` SpriteRenderers.
2. **Tune layout in the Editor:** move `Book Background` and `Content Root` until text aligns with art lines.
3. **Set numbers on `NotebookBookArtLayout`:**
   - `Page Usable Height` — height of one page’s writable area (match ruled lines)
   - `Bookmark Top Inset` / `Bookmark Horizontal Offset` — tab strip position
   - Then enable **`Lock Layout`** so Author menus won’t move rects again.
4. **Full Author is destructive** to unlocked layout. Use **Wire Controller To Shell** for safe repair.
5. **Test in one scene:** `Assets/Notebook/Scenes/NoteBookTesting.unity`

---

## Daily workflow

| Step | Action |
|------|--------|
| 1 | Edit or add entries under `Assets/Notebook/Data/` |
| 2 | Play `NoteBookTesting` |
| 3 | Collect / flip / bookmark — see `NOTEBOOK_TEST_CHECKLIST.md` |
| 4 | If buttons dead → `Peaceland → Notebook → Wire Controller To Shell` |
| 5 | If entire UI missing → `Peaceland → Notebook → Author Open UI In Active Scene` (resets unlocked layout) |

---

## Navigation (product rules — one table)

| Control | Goes to |
|---------|---------|
| Open notebook | Main directory spread (page 1–2) |
| Directory line (Present / M1 / M2) | That section’s **index** spread |
| Bookmark tab | Same as directory line → section **index** |
| Index row | That entry’s **content** page |
| Page edge (left / right) | Previous / next spread |

**Bookmark sides (physical book):**

| You are viewing | Left tabs | Right tabs |
|-----------------|-----------|------------|
| Main directory | Directory | Present, M1, M2 |
| Present | Directory, Present | M1, M2 |
| Memory 1 | Directory, Present, M1 | M2 |
| Memory 2 | All four | (none) |

---

## Editor menus (grouped)

### Scene cleanup & UI

- **Cleanup NoteBookTesting Scene** — recommended baseline: remove stray sprites, rebuild shell, wire refs, clip masks
- **Author Open UI In Active Scene** — rebuild shell; respects `Lock Layout` for rects
- **Wire Controller To Shell** — safe; reconnects refs only
- **Cleanup Legacy Test UI** — removes old test panels

### Data

- **Sync Notebook Database From Assets**
- **Generate Dummy Pagination Entries** — 20 test notes for Present

### Satellite test scenes

- **Author All Notebook Test Scenes**
- **Author Florist Item Collect Scene** / **Intro Newspaper** / etc.

### Art

- **Crop Notebook Art Sprites**

### Preference

- **Auto-Author NoteBookTesting On Open** — default **off**; leave off unless you want every open to rebuild UI

---

## Layout tuning checklist

1. Open `NoteBookTesting`, select `Notebook Open Root`.
2. Align `Book Background` with desired frame.
3. Align `Content Root` so left/right columns sit on ruled lines.
4. On `NotebookBookArtLayout`:
   - Set `Page Usable Height` (start ~300–360; increase if too many items per page).
   - Set bookmark insets if tabs float wrong.
   - Enable **Lock Layout**.
5. `Peaceland → Notebook → Wire Controller To Shell`
6. Save scene.

---

## What we are *not* doing in this pass

- Handwriting font (planned — `NOTEBOOK_ARTWORK.md`)
- Mesh page-flip animation (UI alpha slide only)
- Quest-log features / entry upgrades
- Production save system (still `PlayerPrefs` for prototype)

---

## File map (where code lives)

| Area | Path |
|------|------|
| Controller / open-close / spreads | `Scripts/Runtime/NotebookController.cs` |
| Pagination | `NotebookBookLayoutBuilder.cs`, `NotebookEntryLayoutMeasurer.cs` |
| Bookmarks | `NotebookBookmarkTabBar.cs` |
| Page turn hits | `NotebookPageTurnHitLayer.cs` |
| Editor rebuild | `Scripts/Editor/NotebookOpenUIAuthoring.cs` |
## Editable Scene Pack (scene-authored UI)

**Folder:** `Assets/Notebook/EditableScenePack/` — copy whole folder when migrating.

- Play **does not rebuild** notebook shell when `NotebookSceneAuthoringProfile.disableRuntimeBootstrapRebuild` is true.
- Every interact has `NotebookSceneInteractMarker` in Hierarchy (editable).
- Menus under `Peaceland/Notebook/Editable Pack/`.
- 5-round self-check writes `EditableScenePack/Reports/scene_self_check_latest.md`.

See [EditableScenePack/README.md](/D:/Peaceland/peaceland_Yu_Edit/Assets/Notebook/EditableScenePack/README.md).


---

## If something still looks wrong

1. Confirm only one book UI (no duplicate sprites).
2. Confirm `Lock Layout` + `Page Usable Height` set.
3. Run **Wire Controller To Shell**, not full Author.
4. Note which **spread kind** you see: main directory / section index / content — they behave differently by design.
