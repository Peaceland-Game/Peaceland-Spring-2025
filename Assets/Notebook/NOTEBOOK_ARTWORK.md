# Notebook Artwork Checklist

Status key: **Required** = already required for current spec · **Likely** = likely needed soon · **Polish** = optional polish

---

## Required (confirmed)

| Asset | Purpose | Current status |
|-------|---------|----------------|
| `notebook.png` | Closed notebook / HUD icon base | In `notebook-art/` |
| `notebookOpen_1.png` … `notebookOpen_3.png` | Opening animation keyframes | In `notebook-art/` |
| `notebookOpened.png` | Open book background (`Book Background`) | In `notebook-art/` |
| Page inset reference | Align text to left/right page gutters | Derived from `notebookOpened.png` margins (tune in Editor) |
| Page number typography | Bottom outer corners per spread | TextMeshPro (no art file yet) |

---

## Likely (probably needed)

| Asset | Purpose | Notes |
|-------|---------|-------|
| **Bookmark tab sprites** | Left/right cloth/paper tabs instead of flat UI rects | One sprite per section color, or one 9-slice strip |
| **Handwriting TMP font** | Directory lines, entry body, page numbers | See `NOTEBOOK_IMPLEMENTATION_PLAN.md` → Typography |
| Entry inline images | Optional image per `NotebookEntryDefinition` | Placeholder OK for v1; real art per memory thread |
| Collectible **glow / sparkle** sprite | World-space highlight on flowers & interactables | Currently procedural pulse on flower sprite |
| `notebookUi.png` | Extra chrome (corners, stains, tape) | In repo; usage TBD |
| Section-specific page paper tint | Subtle Present vs Memory paper color | Can be flat color first |

---

## Polish (optional)

| Asset | Purpose |
|-------|---------|
| Page-turn flip sprites / mesh | Realistic forward/back animation (beyond slide) |
| Bookmark ribbon tails | Hanging ribbon ends for active section |
| NEW / unreviewed entry mark | Small ink dot, asterisk, or margin flair |
| Collect toast paper strip | Toast background matching notebook margin |
| HUD notebook icon (closed only) | Distinct small icon separate from full book |
| Spine shadow / page curl overlay | Depth on gutter between left/right pages |
| Per-section divider flourish | Small ink line between index categories |
| Audio: page rustle, collect stamp | Not artwork but pairs with flip/collect |

---

## Folder convention

```
Assets/Notebook/
  notebook-art/     ← book shell & animation (required set above)
  Fonts/            ← handwriting TMP SDF (planned)
  Data/             ← entry ScriptableObjects (may reference entry images)
```

When adding art, prefer **PNG with transparency**, **consistent pixel scale** with `notebookOpened.png`, and note the **safe text area** inset for each page side.
