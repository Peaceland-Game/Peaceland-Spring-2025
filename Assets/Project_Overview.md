# Peaceland - Technical Project Overview

## 1. Project Description
**Peaceland** is a narrative-driven, tactile 2D interactive experience exploring themes of memory, post-conflict reconciliation, emotional processing, and daily life through botanical work and letter writing. The project centers around a florist shop in a historic and emotional setting, accompanied by museum exhibits and memory vignettes.

The core pillars of the experience are:
- **Narrative Exploration via Yarn Spinner**: Rich dialogue branching with characters (Andrej, Boris, Marc, Mira, Child, Teacher, Official), delivering context, trauma processing, and interpersonal depth.
- **Tactile Floristry Minigames**: Interactive, physics- and pointer-driven floral manipulation—specifically stem dethorning, dynamic stem trimming using sprite masks, and bouquet arrangement into target vases.
- **Emotion-Driven Letter Construction**: A poem/letter drafting minigame inspired by visual novel mechanics (such as Doki Doki Literature Club), where selecting specific emotive vocabulary impacts tonal mood meters (Affectionate, Melancholic, Somber, Passionate) and unlocks procedural soundscapes and personalized epilogues.
- **Physical Impairment & Trauma Simulation**: Adjustable difficulty states that simulate physiological distress, such as shaky hands (randomized cursor jitter) and blurred vision (post-processing depth-of-field and blur scaling), directly influencing gameplay precision.

---

## 2. Gameplay Flow / User Loop

### High-Level User Progression
1. **Boot / Initialization**:
   - The application boots into `Assets/Scenes/DemoStart.unity` or `Assets/Scenes/Museum Intro.unity`.
   - The persistent singleton `GameManager` is instantiated, sets persistent cursor states via Unity's `Cursor.SetCursor`, and flags `DontDestroyOnLoad`.
2. **Museum Framing / Exposition**:
   - The player enters the museum space where historical artifacts, newspapers, and memory anchors are examined.
   - Dialogue nodes with characters like Marc and Mira establish the narrative backdrop.
   - Interacting with specific memory objects unlocks transitions into floral memory scenes.
3. **Flower Shop Memory Loop**:
   - **Order Initialization**: An `OrderObject` ScriptableObject is fetched specifying customer sprites, order dialogue nodes, and required flower configurations (`FlowerType`, `needsDethorning`, `needsTrimming`, `needsArranging`).
   - **Customer Greeting**: `DialogueMinigame` triggers the customer's opening dialogue node via Yarn Spinner's `DialogueRunner`.
   - **Preparation & Dethorning**: `CutManager` instantiates the thorny stem; players drag shears via `GrabAndSwipe` to accurately slice off thorns within designated hitboxes.
   - **Dynamic Trimming**: `CuttableFlower` dynamically computes sprite heights and cuts via `CutLogic`, moving clipped remnants downward using `CutPiece`.
   - **Vase Arrangement**: `DragManager` initializes draggable flower tops and target sockets. Players position flowers into target points matching their `FlowerType`.
   - **Customer Fulfillment**: Post-arrangement dialogue node fires upon completion, followed by order handoff handled by `NextOrderMinigame`.
4. **Letter Writing Minigame**:
   - In standalone or transitional sequences, `LetterManager` presents historical or personal correspondence with fill-in-the-blank gaps.
   - Players select word chips from dynamically generated banks, triggering procedural synthesis audio (`PlayProceduralSound`), chibi bounce animations, and live mood score tracking.
   - Sealing the letter presents a qualitative analysis screen detailing the dominant emotional frequency.
5. **Cycle Continuation & Demo End**:
   - Once all orders or memories terminate, the flow transitions through `LevelLoader` to wrap up in `DemoEnd.unity` or loops back to title.

---

## 3. Architecture

### Core Architecture & State Machine
The game avoids monolithic state spaghetti by decomposing sequences into modular, discrete minigame states handled by `FlowerShopManager` orchestrating `MinigameBehavior` components.

- **Persistent Singleton Layer**:
  - `GameManager` lives throughout runtime sessions (`DontDestroyOnLoad`), caching cross-scene player state (`newsRead`, `introSprawlDone`, `memoryObjectAcquired`), mouse cursors, and difficulty coefficients.
- **Minigame State Machine Pattern**:
  - `MinigameBehavior` acts as the polymorphic base class offering contract methods `StartMinigame()` and `StopMinigame()`.
  - `FlowerShopManager` manages an ordered linear list of `MinigameBehavior` implementations (`DialogueMinigame`, `CutManager`, `DragManager`, `InteractManager`, `NextOrderMinigame`). It iterates to the next minigame upon completion events (such as `dialogueRunner.onDialogueComplete` or internal cut counters).
- **Data-Driven Progression**:
  - Narrative dialogue scripts (`.yarn`) communicate bidirectionally with gameplay logic through Yarn functions, Yarn commands, and event listeners.
  - Gameplay tasks and customer parameters are externalized into ScriptableObjects (`OrderObject`, `LetterData`).
- **Input Abstraction**:
  - Built on Unity's New Input System (`PlayerInput`, `InputSystem_Actions`).
  - `InputHelper` normalizes pointer positions across screen-to-world projections, feeding touch/mouse positions into drag handlers and shear slicers.

---

## 4. Game Systems & Domain Concepts

### Global Game Manager & Difficulty System
Manages global flags, custom cursor management, and physiological impairment difficulty modifiers.
- `GameManager`: Persistent singleton maintaining global state, custom hardware cursors, and difficulty levels.
- `DifficultyYarn`: Bridge script allowing Yarn Spinner dialogues to dynamically read and update the `GameManager.Instance.difficulty` setting.

To extend this system, add additional global tracking variables or difficulty modifiers to `GameManager`, and register corresponding Yarn commands in `DifficultyYarn.cs`.
Pattern: Singleton Pattern for global accessibility and state retention.
Location: `Assets/Scripts/Managers/` and `Assets/Scripts/`

### Minigame Orchestration System
Controls the sequencing, transitions, and state changes between dialogues, cutting, arranging, and order processing.
- `FlowerShopManager`: Coordinates order workflows, switches active minigames, maps flower types to sprite textures, and loads subsequent scenes.
- `MinigameBehavior`: Abstract base class defining `StartMinigame()` and `StopMinigame()`.
- `DialogueMinigame`: Wraps Yarn Spinner's `DialogueRunner` into a minigame step, executing dialogues before triggering next steps.
- `NextOrderMinigame`: Handles transitions between floral orders, swapping customer sprites and triggering brief door animations.

To extend this system, inherit from `MinigameBehavior`, implement custom game logic, and insert the new behavior component into the `FlowerShopManager.minigames` serialized list.
Pattern: State Pattern combined with Template Method.
Location: `Assets/Scripts/Minigames/` and `Assets/Scripts/Managers/`

### Botanical Cutting & Trimming System
Simulates tactile stem preparation, thorn removal, and precise scissors cutting using 2D physics and sprite masking.
- `CutManager`: Tracks active floral cuts, instantiates stem and flower prefabs, and coordinates camera post-processing blur effects under elevated difficulty.
- `CutLogic`: Detects shear colliders, records draw lines, calculates average deviation from ideal guide lines, and generates accuracy ratings.
- `CutPiece`: Animates severed thorn and stem remnants down toward waste bins using lerped scale and position curves.
- `CuttableFlower`: Coordinates dual sprite masks for upper and lower stems, dynamically sizing masks and orienting slices based on cut vectors.
- `Stem`: Procedurally spawns thorns with randomized vertical offsets and sides, managing cut counters to signal completion.
- `GuideLine`: Renders target dashed slicing guide vectors for player shears.
- `GrabAndSwipe`: Translates pointer inputs into high-velocity slicing trajectories, adding random jitter based on player difficulty.

To extend this system, instantiate new flower prefabs with customized `CutLogic` colliders or add new cut line assessment metrics in `CutLogic.CalculateCutScore()`.
Pattern: Observer Pattern via `CutLogic.onCut` event notifications.
Location: `Assets/Scripts/` and `Assets/Scripts/DynamicCutting/`

### Draggable Bouquet Arrangement System
Handles the positioning and snapping of cut flowers into target vases.
- `DragManager`: Spawns draggable flower entities and target sockets based on current `OrderObject` configurations.
- `Draggable`: Handles pointer tracking, bounding box clamp checks, jitter effects, and target snapping logic.
- `DragTarget`: Target recipient socket that validates whether incoming flowers match the expected `FlowerType`.

To extend this system, add custom placement constraints, vase layouts, or multi-flower clustering rules in `DragManager.cs`.
Pattern: Component-based interaction pattern with spatial snapping thresholds.
Location: `Assets/Scripts/Minigames/Draggable/`

### Letter Writing & Mood Analysis System
An interactive epistolary minigame where players choose words with distinct emotional values to fill text blanks.
- `LetterManager`: Coordinates UI rendering of letter text, spawns dynamic word choice buttons, manages mood counters, and generates procedural sound effects.
- `LetterWordButton`: UI button component handling hover animations, scale lerping, and mood icons.
- `LetterBlank`: Manages individual text blank states and word assignments.
- `LetterData`: ScriptableObject schema holding letter templates, blanks, word choices, and associated mood tags (`LetterMoodType`).

To extend this system, create new `LetterData` ScriptableObjects with custom blank configurations and associate them in `LetterManager`.
Pattern: Data-driven UI generation with real-time state tallying.
Location: `Assets/Scripts/` and `Assets/Scripts/ScriptableObjectScripts/`

### Scene Interaction & Investigation System
Drives point-and-click environmental item examination in museum and prologue scenes.
- `InteractManager`: Dispatches raycasts or bounds checks over scene colliders, triggering associated dialogue nodes and handling alpha fades.
- `Interactable`: Attached to scene props (bricks, broken glass, calendars, spray cans) with associated dialogue start nodes.
- `Touchable`: Lightweight pointer event interceptor for screen touches.

To extend this system, place an `Interactable` component on any 2D sprite with a `Collider2D` and specify the Yarn start node name.
Pattern: Facade pattern encapsulating pointer collision queries and Yarn node execution.
Location: `Assets/Scripts/Managers/` and `Assets/Scripts/`

---

## 5. Scene Overview

- `Assets/Scenes/DemoStart.unity`: Entry-point scene for the demo; initializes game settings, presents title UI, and transitions into introduction sequences.
- `Assets/Scenes/Museum Intro.unity`: Museum narrative sequence. Players examine historical exhibits, interact with characters (Marc, Mira), and acquire the memory flower anchor.
- `Assets/Scenes/InteractableIntro.unity`: Environmental investigation sequence where players inspect damaged surroundings (shards, bricks, flowers, spray can) via `InteractManager`.
- `Assets/Scenes/FlowerMemoryScene.unity`: Primary gameplay scene housing `FlowerShopManager`, customer portraits, dialogue flows, dethorning, trimming, and arranging minigames.
- `Assets/Scenes/LetterWritingMinigameTest.unity`: Dedicated testing and play space for the letter assembly system, word bank buttons, mood HUD, and seal mechanics.
- `Assets/Scenes/DethornTest.unity` & `DynamicFlowerTest.unity`: Test harness scenes specifically designed for tuning shear physics, thorn hitboxes, and dynamic sprite masking.
- `Assets/Scenes/DemoEnd.unity`: Epilogue and demo wrap-up scene allowing players to restart using `RestartGame.cs`.

### Scene Flow Rules & Transitions
- Scenes load sequentially using Unity `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1)` managed by `FlowerShopManager.NextMinigame()` and `LevelLoader.cs`.
- As a consequence of index-based progression, scenes must be maintained in exact sequence within the Unity Build Settings.
- `GameManager` persists across scene transitions, carrying global narrative flags and player impairment values.

---

## 6. UI System

### UI Technology & Hierarchy
The project utilizes **Unity UI (UGUI)** augmented by **TextMesh Pro (TMP)** for crisp typographic presentation across dialogues, word banks, and mood indicators.

### Structure & Screen Flow
- **Yarn Spinner Dialogue Views**:
  - Managed by Yarn Spinner's standard dialogue UI prefab (`DialogueRunner`), styled with custom dialogue boxes (`DialogueBox.png`, `Dialogue Box Skinny.png`) and typography.
- **Letter Writing Interface**:
  - `letterPaperRect`: Root transform framing the paper background texture.
  - `letterBodyText`: TextMeshProUGUI displaying dynamic letter segments and formatted blanks (`<mark>` highlight tags and dotted underlines).
  - `wordBankPanel` & `wordButtonsContainer`: Layout groups dynamically instantiating `WordButton.prefab` chips with hover-scale interpolations.
  - `moodTrackerPanel`: HUD displaying real-time counters and chibis that bounce upon word selection (`AnimateChibiJump`).
  - `resultsPanel`: Fullscreen summary card displaying dominant emotion, poetic interpretation, and percentage breakdowns.
- **Flower Shop In-Game UI**:
  - In-world sprite renderers for customer portraits (`characterPortrait`, `characterPortrait2`), interactables, and background transitions.
  - Custom hardware cursors switched via `GameManager.OnButtonCursorEnter()` / `OnButtonCursorExit()`.

### How to Add / Modify Screens
1. To modify the letter UI, update the canvas hierarchy in `Assets/Scenes/LetterWritingMinigameTest.unity` and inspect bindings on `LetterManager`.
2. To create a new customer dialogue overlay, register character tags and portraits in `PortaitLogic.cs` and configure node names in the relevant `OrderObject`.

---

## 7. Asset & Data Model

### Data Assets & Schemas
- **`OrderObject`** (`Assets/Scripts/ScriptableObjectScripts/OrderObject.cs`):
  - `nameForOrder` (`string`): Identifier for the transaction.
  - `mainCharSprites` / `secondCharSprites` (`Sprite[]`): Customer emotional portraits.
  - `dialogueStartNode` / `dialogueEndNode` (`string`): Entry and exit Yarn node keys.
  - `flowers` (`List<Flower>`): Struct list defining `flowerType` (`FlowerType` enum), `needsDethorning`, `needsTrimming`, and `needsArranging`.
- **`LetterData`** (`Assets/Scripts/ScriptableObjectScripts/LetterData.cs`):
  - `letterName`, `recipient`, `signoff` (`string`): Header metadata.
  - `textParts` (`string[]`): Story paragraphs sandwiching blank positions.
  - `blanks` (`List<BlankData>`): List containing word choices, point weights, and mood classifications (`LetterMoodType`).
- **Yarn Dialogue Scripts** (`Assets/Yarn/`):
  - `.yarn` plain-text dialogue trees compiled by `.yarnproject` assets (`FlowerShopProject.yarnproject`, `MuseumIntro.yarnproject`).

### Asset Organization Conventions
- `Assets/Art/`: Textures, portraits, flower sprites, backgrounds, and cursors.
- `Assets/Prefabs/`: Modular entities including `CuttableFlower`, `Shears`, `InteractableDoor`, and `WordButton`.
- `Assets/Scripts/`: MonoBehaviours categorized into `Managers/`, `Minigames/`, `DynamicCutting/`, and `ScriptableObjectScripts/`.
- `Assets/Yarn/`: Narrative scripts structured under character and location subdirectories.

---

## 8. Notes, Caveats & Gotchas

- **Build Index Scene Progression**: `FlowerShopManager` advances scenes via `SceneManager.GetActiveScene().buildIndex + 1`. If scenes in the Unity Build Settings are rearranged or omitted, completing minigames can trigger accidental scene loads or runtime boundary exceptions.
- **Dynamic Mask Cleanup**: `CuttableFlower` binds to static event `CutLogic.onCut` during `Start()` and unbinds on `OnDestroy()`. Ensure cuttable flower prefabs are properly destroyed through `CutManager` to avoid dangling event references.
- **Procedural Sound Generation**: `LetterManager` synthesizes audio clips programmatically at runtime (`AudioClip.Create`) without referencing audio assets on disk. If migrating audio to a centralized sound manager, adjust `PlayProceduralSound` accordingly.
- **Difficulty Post-Processing Dependency**: `CutManager` and `DragManager` directly fetch `Camera.main.GetComponent<PostProcessVolume>()`. If the main camera lacks a `PostProcessVolume` component or post-processing layers are misconfigured, difficulty scaling will throw null reference warnings.
- **AABB Manual Hitbox Queries**: `InteractManager.DoClick()` uses manual coordinate math over `Collider2D` boundaries rather than physics raycasting. Altering object scales or camera projections may necessitate reviewing bounds calculations in `DoClick()`.
- **LetterBlank Class Naming**: In `Assets/Scripts/LetterBank.cs`, the class is defined as `LetterBlank` (not `LetterBank`), which is intentional for tracking blank state.