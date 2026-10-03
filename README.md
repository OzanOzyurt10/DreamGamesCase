# Dream Games Case Study – Blast Game

A level-based blast game in **Unity 6000.3.10f1** (Built-in Render Pipeline, C#), portrait 9:16.
All 10 levels of the case, special items, combos, obstacles, win/lose flow and persistent progress are implemented.

## Running the game

1. Open the project in Unity **6000.3.10f1**.
2. Open `Assets/Scenes/MainScene.unity` and press Play. In the Game view, choose a 9:16 resolution (e.g. 1080x1920).
3. **Dream Games → Set Level** opens a small window to jump to any level (1–10) or to "Finished".
   Progress is stored in `PlayerPrefs`, so it survives restarts.

Build order: `MainScene` (0), `LevelScene` (1).

**Tests:** Window → General → Test Runner → EditMode → Run All (64 tests).

## Project layout

```
Assets/Scripts/Core      Pure C# model and rules. Own assembly with no UnityEngine reference.
Assets/Scripts/View      MonoBehaviours: board and item views, animations, effects, pooling, input.
Assets/Scripts/Meta      Scene flow, progress, level loading, top panel, end screens.
Assets/Editor            The "Set Level" editor window.
Assets/Tests/EditMode    NUnit tests for the Core rules.
Assets/Resources/Levels  level_01.json … level_10.json
```

## Architecture

### Model and view are separated
`Core` is compiled into its own assembly (`DreamCase.Core.asmdef`) with **no engine references**, so the
compiler guarantees that game rules never touch Unity objects or views.

The model resolves a whole tap **instantly** and returns a result object (`BlastResult`, `ExplosionResult`,
`FallResult`). The view only **replays** that result with DOTween. Rules are therefore independent of animation
timing and fully unit-testable.

### Dependencies without a DI framework
Each scene has a **composition root** (`MainSceneBootstrap`, `LevelSceneBootstrap`): the one place that creates
the services and wires them together. Plain C# classes get their dependencies through constructors; MonoBehaviours
get them through an `Initialize(...)` method. There are no singletons and no `FindObjectOfType` lookups at runtime.

### Turn state machine
`LevelController` runs `Idle → Resolving → Falling → (Shuffling) → CheckEnd → Idle`, or `Ended`.
Taps are accepted only in `Idle`. Win and loss are decided only in `CheckEnd`, after the board has fully
settled, so a chain reaction on the last move can still win the level.

### Items and polymorphism

```
Item
├── Cube
├── SpecialItem ── Rocket, Tnt
└── Obstacle ───── Vase, Stone, ChaliceBox
```

Behaviour lives in virtual members, so board logic never branches on an item's concrete type:

| Member | Meaning |
|---|---|
| `CanFall` | Stone and ChaliceBox return `false` and hold up everything above them. |
| `CanBeTapped(board)` / `Tap(context)` | A cube needs a same-coloured group of 2+; a special item can always be tapped; obstacles can't. |
| `TakeDamage(DamageInfo)` → `None` / `Damaged` / `Cleared` | Each item applies its own damage rules. |
| `OnCleared` / `Detonate` | A special item cleared by an explosion detonates in its own shape (chain reactions). |

The 2x2 `ChaliceBox` is a single object referenced by its four cells. It is created from its bottom-left code
(`cbBL`); the other corner codes land on cells it already covers.

### Registries instead of switches
- `ItemFactory`: level code → item.
- `ItemViewFactory`: item type → view prefab, with one pool per type.
- `GoalsView`: obstacle type → goal icon.

### Damage model
`DamageInfo` carries the damage **type** (`Blast` / `Explosion`), a unique **SourceId** per blast or explosion,
and the number of **affected cells**.
- **Blast:** every item next to the blasted cubes is damaged **once**, with the number of blasted cubes touching it.
- **Explosion:** one hit per cell. A single source never hits the same cell twice, even where a combo's rocket lines cross.
- **Vase:** ignores a SourceId it has already taken damage from.
- **ChaliceBox:** the source that opens the door deals no further damage.

### Explosion timeline
`ExplosionResolver` simulates explosions on a **tick** timeline:
- Rocket halves hit the k-th cell from their origin on tick + k.
- TNT hits its whole area on the next tick.
- A special item cleared by a hit detonates and schedules its own hits on later ticks.

The view plays tick N at N × 0.06 s. Rocket halves fly at constant speed, so a half crosses each cell exactly
when the model hits it.

### Combos
`FloodFill` is generic: it finds same-coloured cubes for blasts and connected special items for combos.
`ComboRules` is the single place that picks the combo:
- At least two TNTs explode in a **7x7** area.
- Exactly one TNT with rockets fires **3 rows + 3 columns** of rockets.
- Rockets only fire a **+** (one horizontal and one vertical rocket).

The members merge into the tapped cell without their own explosions, and the whole combo is one damage source.

### Gravity and refill
`Gravity` scans each column bottom-up, remembering the lowest free cell. An item that can't fall resets that
cell, so nothing ever passes through it. `Refill` fills only the cells open to the top of their column, so holes
under a stone stay empty. New cubes start stacked above the board and slide in through a `SpriteMask`.

### Pooling
Unity's `ObjectPool` is used for item views (one pool per item type), rocket halves and particle effects.

### UI
uGUI + TextMeshPro, Canvas Scaler 1080x1920 (match 0.5). The top panel and end screens are camera-space
canvases, so the celebration particles can be sorted above every UI element:
background −100, board 0–150, top panel 200, end screens 300, celebration sparkles 310.

## Adding a new item

Example: an "Ice" obstacle.

1. **Core:** `public sealed class Ice : Obstacle` and override `TakeDamage` (and `CanFall` if it shouldn't fall).
2. **Factory:** register its level code in `ItemFactory`, e.g. `["i"] = () => new Ice()`.
3. **View:** make a prefab with `ItemView` (or a small `IceView` if its look changes), register it in
   `ItemViewFactory`, and add its goal icon to `GoalsView`.

Board, gravity, damage, explosions, shuffle and the win check need no changes.

## Spec interpretations and additions

| Topic | Decision | Reason |
|---|---|---|
| Chalice Box door | One damage opens the door. | The art pack has a single door sprite and no damaged state. |
| Chalice count | 10 chalices **per box**; the box is cleared when all are collected. | The box art has two shelves, and the case's screenshot shows a box full of cups. |
| TNT + Rocket combo | "3x3 array of exploding rockets in a + shape" is implemented as 3 horizontal + 3 vertical rockets through the cells around the tapped one. | Matches the case's "TNT-Rocket Combo" screenshot. |
| TNT + TNT with rockets | Any combo with at least two TNTs is a 7x7 explosion. | The spec's wording ("at least 2 TNTs"); level 6 has such a row. |
| Merging cubes | Cubes that merge into a special item still damage their neighbours. | Same as Royal Match. |
| **Shuffle** (added) | When no item can be tapped, the cubes are shuffled among their cells: random tries first, then two same-coloured cubes are placed side by side. If no shuffle can create a move, the level is lost. | **Not in the case spec. Added because Royal Match does it.** |
| Top panel and particle effects (added) | Goal counts with check marks, the move counter, and pooled particle effects for every item. | The case asks for the nicest animations possible, and its screenshots show a goal/move panel. |

## Tests

64 EditMode tests in `Assets/Tests/EditMode` cover:
- loading all 10 levels and reading the grid from the bottom-left
- flood fill and valid/invalid taps (only valid taps spend a move)
- gravity, blockers and refill
- special item creation and hints
- rockets, TNT and chain reactions
- every combo
- every obstacle rule
- win/lose, including a chain reaction that wins on the last move
- shuffle

`Core` exposes its internals to the test assembly with `InternalsVisibleTo`, so item rules are tested directly.

## Third-party
- [DOTween](http://dotween.demigiant.com/) (free version), in `Assets/Plugins/Demigiant`.
- Art and level files from the case study pack.
