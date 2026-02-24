# You Are What You Keep — Scaffolding Document
### Version 6 — Scaffolding Complete

> This document is the complete record of the game scaffolding — all systems, decisions, and scripts built across Phases 1–8. The scaffolding is done. World building is tracked in a separate document: **YAWYK_WorldBuild_v1.md**
>
> Read this top to bottom before touching any system code.

---

## Table of Contents

1. Game Manifesto
2. Project Setup & Tools
3. Folder Structure
4. Scene Architecture
5. Systems Architecture Overview
6. Scripts — Complete Reference
7. UI Canvas — Structure & State
8. Memory Assets
9. Input System
10. Build Phases — Complete Roadmap
11. Pre-wired Dependencies & Future Hooks
12. Known Issues Log

---

# PART 1 — GAME MANIFESTO

### Working Title
**You Are What You Keep**

### Core Philosophy
The goal is not to win — the goal is to live a meaningful life inside a system. This game rejects traditional progression loops (XP, loot, grinding) in favour of emotional payoff, player-authored meaning, and systemic emergence. The game should create memories, not milestones.

### Player Objective
There is no explicit win state. The player is trying to live an interesting life, accumulate meaningful moments, and shape who they become through what they experience. Endings are reflective, not victorious.

---

### Design Pillars

**Moments over Progression** — The primary reward is a felt experience: awe, regret, nostalgia, calm, pride. If a mechanic exists, it must serve a moment.

**Identity as Inventory** — The player collects memories, traits, and ways of being. Your build is what you've lived, what you've kept, what you've let go of.

**Time as Gentle Pressure** — Time is always present but not punishing. Bittersweet, inevitable, motivating. Time creates meaning by making things finite.

**Systems That Respect the Player** — Avoid over-explaining. Trust player interpretation. Allow ambiguity. Meaning emerges through interaction, not participation.

**Self-Authored Reward** — The best moments are not scripted. The game enables useless but beautiful actions, personal rituals, quiet discovery.

---

### Core Mechanics

**Memories as Inventory** — Players collect memories instead of items. Each memory occupies a limited slot, subtly alters gameplay or perception, and shapes identity. You cannot keep everything. Forgetting is a mechanic.

**Limited Inner Space** — 5–8 memory slots. Creates emotional tradeoffs, personal curation, identity evolution. Letting go should feel significant.

**Moments That Transform** — Certain experiences permanently change the player. A near-death fall becomes fearlessness or fragility. Deep solitude becomes heightened awareness. Transformation replaces levelling.

**The World Remembers** — Meaningful actions leave subtle echoes. A place where you linger becomes warmer. The world holds the shape of your presence.

**Skills as Lived Experience** — Skills emerge from behaviour and fade when abandoned. Climb often → natural agility. Sit quietly → heightened awareness.

---

### Tone and Aesthetic
Warm, reflective, slightly melancholic, hopeful. Not bleak. Not cynical. Stylised realism or soft minimalism. Strong lighting language. Expressive colour shifts. Calm environmental storytelling.

Reference points: Journey, Flower (thatgamecompany), Firewatch, Gris.

### One-Line Summary
*A game about becoming someone through the moments you choose to keep.*

---

# PART 2 — PROJECT SETUP & TOOLS

| Item | Value |
|------|-------|
| Engine | Unity (Latest LTS) |
| Render Pipeline | URP (Universal Render Pipeline) |
| Input System | New Unity Input System (package installed) |
| Version Control | Git + GitHub, managed via GitHub Desktop |
| Text Rendering | TextMeshPro (package installed, essentials imported) |

### Unity Project Settings
- Asset Serialization Mode: **Force Text**
- Version Control Mode: **Visible Meta Files**

### Post Processing — Critical Setup Notes
- Global Volume lives in `_Persistent` scene
- Profile must have Color Adjustments, Bloom, Vignette added as overrides
- Every **individual value checkbox** in the profile must be **ticked blue** — grey/unticked values cannot be changed by scripts at runtime
- `PlayerCamera` must have **Post Processing enabled** in Camera component (or Universal Additional Camera Data component) — this is separate from the Volume setup and is required for effects to actually render

### Audio — Critical Setup Notes
- `PlayerCamera` must have an **Audio Listener** component
- There must be **exactly one** Audio Listener in the scene
- Audio clips for ambient layers go in `Assets/_Game/Audio/Ambient/`
- Moment sting clip uses a time offset (`momentStingSource.time = 0.3f`) to skip dead air — adjust value as needed per clip

### Commit History
1. Initial project setup, folder structure, scenes, Git
2. Phase 2: Player controller with weighted movement and camera bob
3. Phase 3: Memory system — data layer, moment triggers, first memories
4. Phase 4 complete: memory UI fully working — slots, prompts, reflection screen, replace mode, HUD dots
5. Phase 5 complete: emotional response system — post processing world state, layered audio, moment bloom response
6. Phase 6: Identity and trait system — traits derived from memories, movement and perception modifiers
7. Phase 7: Time system and world echo — vividness decay, echo points, time-driven atmosphere
8. Phase 8: Ending and reflection sequence — narrator, passage system, personalised ending
9. **Bug fix: Ending sequence now works — EndingUI registration moved to Awake with fallback search**

---

# PART 3 — FOLDER STRUCTURE

```
Assets/
├── _Game/
│   ├── Art/
│   │   ├── Characters/
│   │   ├── Environment/
│   │   ├── UI/
│   │   └── VFX/
│   ├── Audio/
│   │   ├── Ambient/          ← AMB_ audio files
│   │   ├── Music/
│   │   └── SFX/
│   ├── Materials/
│   ├── Prefabs/
│   │   ├── Characters/       ← PFB_Player
│   │   ├── Environment/
│   │   ├── Memories/         ← MEM_MomentTrigger_Base
│   │   ├── Systems/          ← --- SYSTEMS --- prefab
│   │   └── UI/               ← UI_Canvas, UI_MemoryCard, UI_SlotDot
│   ├── Scenes/
│   │   ├── Core/             ← _Boot, _Persistent
│   │   └── Locations/        ← Location_Opening
│   ├── Scripts/
│   │   ├── Core/             ← GameManager, AudioManager, EmotionalResponseSystem,
│   │   │                        EndingSystem, EndingNarrator
│   │   ├── Identity/         ← IdentitySystem
│   │   ├── Memories/         ← MemoryData, MemoryInstance, MemorySystem
│   │   ├── Player/           ← PlayerController, CameraController
│   │   ├── Time/             ← TimeSystem
│   │   ├── World/            ← MomentTrigger, WorldEchoSystem, EndingTrigger
│   │   └── UI/               ← UIManager, MomentPromptUI, MemorySlotHUD,
│   │                            MemoryReflectUI, MemoryCardUI, EndingUI
│   └── ScriptableObjects/
│       ├── Memories/         ← MEM_ assets
│       ├── Traits/
│       └── Moments/
└── ThirdParty/
```

### Prefab Naming Convention
- `SYS_` — system objects
- `ENV_` — environment pieces
- `MEM_` — memory-related objects
- `UI_` — interface elements
- `CHR_` — characters
- `PFB_` — player prefab

---

# PART 4 — SCENE ARCHITECTURE

### Build Settings Order
1. `_Boot`
2. `_Persistent`
3. `Location_Opening`

### _Persistent Scene Hierarchy
```
_Persistent
└── --- SYSTEMS ---                  ← DontDestroyOnLoad set HERE and ONLY HERE
    ├── GameManager                  ← GameManager.cs
    ├── MemorySystem                 ← MemorySystem.cs
    ├── AudioManager                 ← AudioManager.cs
    ├── EmotionalResponseSystem      ← EmotionalResponseSystem.cs
    ├── IdentitySystem               ← IdentitySystem.cs
    ├── TimeSystem                   ← TimeSystem.cs
    ├── WorldEchoSystem              ← WorldEchoSystem.cs
    └── EndingSystem                 ← EndingSystem.cs
```

**Critical rules:**
- `--- SYSTEMS ---` must be at the **absolute root** of `_Persistent` — not nested under any parent object
- Only the root `--- SYSTEMS ---` calls `DontDestroyOnLoad`
- Child systems do NOT call `DontDestroyOnLoad` — they inherit persistence from the root
- Each system's Awake uses `transform.SetParent(null)` before `DontDestroyOnLoad` as a safety net for additive scene loading
- After adding/changing anything in this hierarchy: select `--- SYSTEMS ---` → **Overrides → Apply All to Prefab**

### Location_Opening Scene Hierarchy
```
Location_Opening
├── Player                           ← PFB_Player (Audio Listener on PlayerCamera)
├── [Environment geometry]           ← floor on Ground layer
├── Moment_[Name]                    ← MomentTrigger objects
├── Ending_Leave                     ← EndingTrigger object
└── UICanvas                         ← UI_Canvas prefab
    ├── HUD
    │   ├── MemorySlotHUD
    │   └── MomentPrompt
    ├── Screens
    │   ├── MemoryReflect
    │   └── EndingScreen             ← EndingUI.cs attached here
    └── MasterFade                   ← Must be named exactly "MasterFade"
```

**Cross-scene reference rule:** `EndingSystem` lives in `_Persistent` but needs `EndingUI` and `MasterFade` from the location scene. These cannot be wired in the Inspector. Instead, `EndingUI` registers itself with `EndingSystem` on `Awake()`. `EndingSystem.TriggerEnding()` also performs a fallback search including inactive objects if registration was missed. `MasterFade` is found by name at trigger time.

---

# PART 5 — SYSTEMS ARCHITECTURE OVERVIEW

```
┌─────────────────────────────────────────────────────────────┐
│                      DATA LAYER                             │
│  MemoryData (ScriptableObject) — defines a memory           │
│  TraitType (enum) — identity categories                     │
│  MemoryCategory (enum) — experience categories              │
│  Season (enum) — time of year                               │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                   RUNTIME STATE                             │
│  MemoryInstance (plain C# class)                           │
│  WorldEcho (plain C# class inside WorldEchoSystem)         │
│  EndingPassage (plain C# class)                            │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│              PERSISTENT MANAGERS (never unload)             │
│                                                             │
│  GameManager              ← state machine                   │
│  MemorySystem             ← slots, keep/forget, events      │
│  AudioManager             ← layered category audio          │
│  EmotionalResponseSystem  ← post processing world state     │
│  IdentitySystem           ← trait profile, threshold events │
│  TimeSystem               ← clock, vividness decay          │
│  WorldEchoSystem          ← echo points, linger detection   │
│  EndingSystem             ← orchestrates ending sequence    │
└──────┬───────────────┬────────────────────────────────────--┘
       │               │
┌──────▼──────┐  ┌─────▼──────────────────────────────────────┐
│   WORLD     │  │  UI LAYER                                   │
│   OBJECTS   │  │  UIManager, MomentPromptUI                  │
│             │  │  MemorySlotHUD, MemoryReflectUI             │
│  Moment     │  │  MemoryCardUI, EndingUI                     │
│  Trigger    │  └─────────────────────────────────────────────┘
│  World Echo │
│  Ending     │
│  Trigger    │
└─────────────┘
       │
┌──────▼──────────────────────────────────────────────────────┐
│                      PLAYER                                 │
│  PlayerController    ← movement + trait modifiers           │
│  CameraController    ← bob, tilt, FOV + trait modifiers     │
└─────────────────────────────────────────────────────────────┘
```

### Communication Pattern
All systems communicate through **C# events**, not direct references. `MemorySystem` fires events. All other systems subscribe. No system needs to know another exists. This keeps code decoupled and maintainable.

### Event Flow (complete)
```
MomentTrigger.ExperienceMoment()
    → MemorySystem.OfferMemory()
        → MemorySystem.OnMemoryKept fired
            → AudioManager subscribes       → fades in category layer, plays sting
            → EmotionalResponseSystem       → recalculates world state, moment bloom
            → IdentitySystem                → shifts trait values
            → WorldEchoSystem               → registers echo at player position
            → MemorySlotHUD                 → refreshes dot indicators
            → MemoryReflectUI (if open)     → rebuilds card list

TimeSystem.Update()
    → hourly: TickVividnessDecay()
        → MemorySystem.NotifyMemoriesChanged()
            → EmotionalResponseSystem       → recalculates colour blend
            → MemoryReflectUI               → cards update vividness opacity

IdentitySystem.ShiftTrait()
    → OnTraitThresholdCrossed fired
        → EmotionalResponseSystem           → world surge + audio boost
        → PlayerController reads traits     → movement modifiers applied each frame
        → CameraController reads traits     → FOV + bob modifiers applied each frame

EndingTrigger.TriggerEnding()
    → EndingSystem.TriggerEnding()
        → GameManager.SetGameState(Ending)  → freezes player input
        → FadeToBlack + FadeAudioOut
        → EndingNarrator.GenerateEnding()   → reads all systems, generates passages
        → EndingUI.ShowPassages()           → displays personalised reflection
```

---

# PART 6 — SCRIPTS COMPLETE REFERENCE

---

## GameManager.cs
**Path:** `Assets/_Game/Scripts/Core/GameManager.cs`
**Attached to:** `GameManager` (child of `--- SYSTEMS ---`)
**Pattern:** Singleton — `GameManager.Instance`

**Game States:**
```csharp
Booting, Playing, Reflecting, Transitioning, Ending
```

**Note:** `CurrentState` temporarily hardcoded to `GameState.Playing` in Awake for testing.

**Awake pattern:**
```csharp
private void Awake()
{
    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    Instance = this;
    transform.SetParent(null);      // safety for additive scene loading
    DontDestroyOnLoad(gameObject);
}
```

**Public API:**
```csharp
GameManager.Instance.SetGameState(GameState state)
GameManager.Instance.IsPlaying()                // bool
GameManager.Instance.CurrentState               // GameState
GameManager.Instance.OnGameStateChanged         // event Action<GameState>
```

---

## PlayerController.cs
**Path:** `Assets/_Game/Scripts/Player/PlayerController.cs`
**Attached to:** Root `Player` object
**Prefab:** `Assets/_Game/Prefabs/Characters/PFB_Player`
**Requires on same object:** Rigidbody, CapsuleCollider
**Tag:** `Player`

**Rigidbody:** Mass 70, Drag 5, Angular Drag 999, Freeze Rotation XYZ
**Capsule:** Height 1.8, Radius 0.4, Center Y 0.9
**Floor geometry:** Must be on `Ground` layer for jump ground check

**Base Inspector values:**
| Field | Default |
|-------|---------|
| Walk Speed | 3.5 |
| Acceleration | 8 |
| Deceleration | 12 |
| Jump Force | 6 (uses VelocityChange) |
| Gravity Multiplier | 2.5 |
| Look Sensitivity | 0.15 |
| Vertical Look Clamp | 80 |

**Jump uses ForceMode.VelocityChange** — ignores Rigidbody mass, value of 6-8 feels natural.

**Trait modifier fields (Phase 6):**
```csharp
float agileSpeedBonus = 1.5f
float fearlessJumpBonus = 2f
float fragileSpeedPenalty = 0.8f
float fragileGravityBonus = 1.5f
float fragileJumpPenalty = 1f
```

Traits apply as: `effectiveValue = baseValue + (traitStrength * modifier * 2f)` where `traitStrength = GetTraitStrength()` returns 0–0.5 above neutral.

---

## CameraController.cs
**Path:** `Assets/_Game/Scripts/Player/CameraController.cs`
**Attached to:** `PlayerCamera`
**Tagged:** MainCamera
**Has:** Camera component, Audio Listener

**Player Hierarchy:**
```
Player ← PlayerController, Rigidbody, CapsuleCollider, Tag:Player
├── CameraRoot (Y pos: 1.7 — eye height)
│   └── PlayerCamera ← Camera, CameraController, Audio Listener, Tag:MainCamera
└── BodyRoot
```

**Trait perception modifiers:**
- Calm → reduces camera bob amplitude
- Curious → increases FOV (base 60, max +8 at full trait)
- Fragile → slightly narrows FOV (max -3 at full trait)
- FOV clamped 50–80 degrees

**Public:**
```csharp
TriggerEmotionalMoment(float intensity)   // gentle push-in coroutine
```

---

## MemoryData.cs
**Path:** `Assets/_Game/Scripts/Memories/MemoryData.cs`
**Type:** ScriptableObject
**Create via:** Right-click → Memory → New Memory
**Assets in:** `Assets/_Game/ScriptableObjects/Memories/`

**Fields:**
```csharp
string memoryTitle
string memoryDescription
MemoryCategory category
float emotionalWeight               // 0-1, influence strength

// Phase 5 — ACTIVE
Color worldTintContribution         // drives post processing colour blend

// Phase 7 — HOOK (not yet wired)
AudioClip ambientLayer              // per-memory personal audio layer — leave empty

// Phase 6 — ACTIVE
TraitType[] reinforcedTraits        // traits nudged up when memory kept
TraitType[] erodedTraits            // traits nudged down when memory kept

// Presentation
Sprite memoryIcon
Color memoryColour                  // UI card accent + MomentTrigger glow colour
```

**Enums (defined in MemoryData.cs):**
```csharp
MemoryCategory: Nature, Solitude, Connection, Risk, Creation, Loss, Wonder, Stillness
TraitType: Fearless, Fragile, Curious, Calm, Aware, Warm, Agile, Melancholic, Resilient, Open
```

---

## MemoryInstance.cs
**Path:** `Assets/_Game/Scripts/Memories/MemoryInstance.cs`
**Type:** Plain C# class (not MonoBehaviour)

```csharp
MemoryData data
float timeAcquired
bool hasBeenReflectedOn
float vividness                     // 0-1, ticked down hourly by TimeSystem
```

**Convenience properties:** `.Title` `.Description` `.Category` `.EmotionalWeight` `.MemoryColour` `.Icon`

---

## MemorySystem.cs
**Path:** `Assets/_Game/Scripts/Memories/MemorySystem.cs`
**Pattern:** Singleton — `MemorySystem.Instance`
**Config:** `maxMemorySlots = 6`

**Events:**
```csharp
OnMemoryKept(MemoryInstance)
OnMemoryForgotten(MemoryInstance)
OnMemorySlotsFull(MemoryData, List<MemoryInstance>)
OnMemoriesChanged()
```

**Public API:**
```csharp
OfferMemory(MemoryData)
KeepMemory(MemoryData)
ForgetMemory(MemoryInstance)
ReplaceMemory(MemoryInstance toForget, MemoryData toKeep)
GetAllMemories()
HasFreeSlot()
HasMemoryOfCategory(MemoryCategory)
GetTotalEmotionalWeight()
GetUsedSlots()
GetSlotCount()
AlreadyHolding(MemoryData)
NotifyMemoriesChanged()
```

---

## MomentTrigger.cs
**Path:** `Assets/_Game/Scripts/World/MomentTrigger.cs`
**Base Prefab:** `Assets/_Game/Prefabs/Memories/MEM_MomentTrigger_Base`

**Inspector:**
```csharp
MemoryData memoryData
float triggerRadius = 3f
string promptText
float lingerTime = 0f           // 0 = press E, >0 = auto-trigger after N seconds
bool consumeOnUse = true
bool visibleInWorld = true
float glowRadius = 2.5f
float glowIntensity = 0.5f
float pulseSpeed = 1.2f
float pulseAmount = 0.15f
```

---

## AudioManager.cs
**Path:** `Assets/_Game/Scripts/Core/AudioManager.cs`
**Pattern:** Singleton — `AudioManager.Instance`

**Architecture:** One AudioSource per layer, all on this GameObject. Layers have target volumes. Update() smoothly lerps toward them.

**Inspector:**
```csharp
AudioClip baseAmbientClip
float baseAmbientVolume = 0.15f
AudioLayerConfig[] categoryLayers
float fadeDuration = 3f
float maxLayerVolume = 0.4f
AudioClip momentStingClip
float momentStingVolume = 0.6f
```

**Public API:**
```csharp
TriggerMomentSting()
BoostLayer(MemoryCategory, float boostAmount, float duration)
```

---

## EmotionalResponseSystem.cs
**Path:** `Assets/_Game/Scripts/Core/EmotionalResponseSystem.cs`
**Pattern:** Singleton — `EmotionalResponseSystem.Instance`

**Inspector values (recommended):**
```
Base Saturation: -30      Peak Saturation: 60
Base Bloom: 0.2           Peak Bloom: 2.5
Base Vignette: 0.2        Peak Vignette: 0.5
Moment Bloom Spike: 4     Moment Time Dilation: 0.75
Transition Speed: 3
```

**Public API:**
```csharp
PushEmotionalState(float satBoost, float bloomBoost, float duration)
```

---

## IdentitySystem.cs
**Path:** `Assets/_Game/Scripts/Identity/IdentitySystem.cs`
**Pattern:** Singleton — `IdentitySystem.Instance`

**Events:**
```csharp
OnTraitChanged(TraitType, float newValue)
OnTraitThresholdCrossed(TraitType, float newValue)
```

**Public API:**
```csharp
GetTraitValue(TraitType)
HasTrait(TraitType, float threshold = 0.6f)
GetTraitStrength(TraitType)
GetDominantTraits(float threshold)
GetFullProfile()
```

---

## TimeSystem.cs
**Path:** `Assets/_Game/Scripts/Time/TimeSystem.cs`
**Pattern:** Singleton — `TimeSystem.Instance`

**Config:**
```csharp
float timeScale = 60f
float startingHour = 8f
float vividnessDecayPerHour = 0.02f
float minimumVividness = 0.15f
int daysPerSeason = 7
```

**Events:**
```csharp
OnHourChanged(float hour)
OnDayChanged(int day)
OnSeasonChanged(Season season)
OnTimeOfDayUpdated(float dayProgress)
```

---

## WorldEchoSystem.cs
**Path:** `Assets/_Game/Scripts/World/WorldEchoSystem.cs`
**Pattern:** Singleton — `WorldEchoSystem.Instance`

**Config:**
```csharp
float echoFeelRadius = 5f
float lingerThreshold = 8f
float echoAtmosphereStrength = 8f
```

**Public API:**
```csharp
IsNearEcho()
GetCurrentEchoStrength()
GetAllEchoes()
RegisterSignificantEcho(Vector3, Color, string)
```

---

## EndingSystem.cs
**Path:** `Assets/_Game/Scripts/Core/EndingSystem.cs`
**Pattern:** Singleton — `EndingSystem.Instance`

**How EndingUI is found:**
`EndingUI.Awake()` calls `EndingSystem.Instance.RegisterEndingUI(this)`. If registration is missed (e.g. due to initialisation order), `TriggerEnding()` performs a fallback `FindFirstObjectByType<EndingUI>(FindObjectsInactive.Include)`. Either path works.

**Sequence:**
1. Set `GameState.Ending`
2. Unlock cursor
3. Generate passages via `EndingNarrator.GenerateEnding()`
4. Fade to black (3s) + fade audio out
5. Wait 2s
6. Activate `EndingScreen`, call `EndingUI.ShowPassages()`
7. Final passage holds indefinitely

**Public API:**
```csharp
EndingSystem.Instance.TriggerEnding()
EndingSystem.Instance.RegisterEndingUI(EndingUI ui)
```

---

## EndingNarrator.cs
**Path:** `Assets/_Game/Scripts/Core/EndingNarrator.cs`
**Type:** Static class

Reads all systems to generate 5 personalised passages: Opening, Memories, Identity, World, Closing.

---

## EndingUI.cs
**Path:** `Assets/_Game/Scripts/UI/EndingUI.cs`
**Attached to:** `EndingScreen` (child of Screens, child of UICanvas)

**Important:** Uses `Awake()` for registration (not `Start()`), with a coroutine fallback if `EndingSystem` isn't ready yet. `EndingScreen` can be inactive at scene load — `EndingSystem` will activate it when needed.

---

# PART 7 — UI CANVAS STRUCTURE

## Full Hierarchy
```
UICanvas                             ← UIManager.cs, Canvas Scaler 1920x1080 Match 0.5
├── MasterFade                       ← Image (black), CanvasGroup alpha 0, ALWAYS ACTIVE
│                                       Must be named exactly "MasterFade"
├── HUD
│   ├── MemorySlotHUD                ← MemorySlotHUD.cs, anchored bottom-right
│   │   └── SlotContainer            ← Horizontal Layout Group
│   └── MomentPrompt                 ← MomentPromptUI.cs, CanvasGroup, bottom-centre
│       └── PromptText               ← TMP, italic, cream
└── Screens
    ├── MemoryReflect                ← MemoryReflectUI.cs, CanvasGroup alpha 0, inactive
    └── EndingScreen                 ← EndingUI.cs, CanvasGroup, inactive by default
        ├── Background               ← Image, full stretch, near black
        ├── PassageTypeLabel         ← TMP, top-centre, small caps, faint
        └── PassageText              ← TMP, centre, size 20, italic, cream, wrapping ON
```

## Critical UI Notes

**Viewport masking:** Use **Rect Mask 2D** on Viewport — NOT Mask + Image.

**Raycast Target rules:**
- MasterFade: OFF
- Backdrop: ON
- Panel: OFF
- Cards: ON

**MasterFade position:** Must be a direct child of UICanvas and listed first in hierarchy.

---

# PART 8 — MEMORY ASSETS

All in `Assets/_Game/ScriptableObjects/Memories/`

| Asset | Title | Category | Weight | Reinforced | Eroded | Tint |
|-------|-------|----------|--------|-----------|--------|------|
| MEM_SatByWater | Sat by the water alone | Stillness | 0.6 | Calm, Aware | Fragile | Soft cool blue |
| MEM_StoodOnHighGround | Stood somewhere high | Wonder | 0.7 | Fearless, Curious, Melancholic | Fragile | Pale gold |
| MEM_WalkedIntoRain | Walked into the rain | Risk | 0.5 | Fearless, Resilient | Fragile | Deep slate |
| MEM_WatchedSunMove | Watched the light change | Stillness | 0.4 | Calm, Melancholic | — | Warm amber |
| MEM_FoundHiddenPlace | Found somewhere nobody knew | Solitude | 0.8 | Curious, Aware, Open | Warm | Forest green |
| MEM_WentToTheEdge | Went to the edge | Risk | 0.5 | Fearless, Resilient | Fragile, Calm | Deep red |

---

# PART 9 — INPUT SYSTEM

| Action | Type | Binding | Used By |
|--------|------|---------|---------|
| Move | Value/Vector2 | WASD + Arrows | PlayerController, CameraController |
| Look | Value/Vector2 | Mouse Delta | PlayerController |
| Jump | Button | Space | PlayerController |
| Interact | Button | E | MomentTrigger, EndingTrigger |
| Reflect | Button | Tab | UIManager |

---

# PART 10 — BUILD PHASES ROADMAP

| Phase | Focus | Status |
|-------|-------|--------|
| 1 | Project setup, folder structure, scenes, GameManager, Git | ✅ Complete |
| 2 | First-person player, weighted movement, camera bob | ✅ Complete |
| 3 | Memory system — data layer, moment triggers | ✅ Complete |
| 4 | Memory UI — slots, prompts, reflection screen, replace mode | ✅ Complete |
| 5 | Emotional Response System — post processing, layered audio | ✅ Complete |
| 6 | Identity & Trait System — traits change movement/perception | ✅ Complete |
| 7 | Time System + World Echo — vividness decay, echo points | ✅ Complete |
| 8 | Ending & Reflection Sequence | ✅ Complete |
| Bug fix | EndingUI registration — Awake + fallback search | ✅ Resolved |
| **World Building** | Locations, art, NPCs, memories, environment | **See YAWYK_WorldBuild_v1.md** |

---

# PART 11 — PRE-WIRED DEPENDENCIES & FUTURE HOOKS

## Hook 1 — ambientLayer on MemoryData
Per-memory personal audio layer. Wire in polish phase by extending AudioManager.

## Hook 2 — GetTotalEmotionalWeight()
Built, used by EndingNarrator. Could also drive ending music selection.

## Hook 3 — RegisterSignificantEcho()
Built, never called. For scripted story moments that permanently mark a location.

## Hook 4 — TriggerEmotionalMoment() on CameraController
Built, never called from outside. For cinematic emphasis during scripted events.

## Hook 5 — GameState.Transitioning
Enum value exists, never set. For scene transitions.

## Hook 6 — Season enum
Seasons fire events and shift post processing. Future: season-locked memories.

---

# PART 12 — KNOWN ISSUES LOG

| Issue | Severity | Status | Fix |
|-------|----------|--------|-----|
| EndingUI not found — ending screen text never appears | High | ✅ **Resolved** | EndingUI.Awake() registers with EndingSystem. Fallback search added in TriggerEnding(). |
| DontDestroyOnLoad warning | Medium | ✅ Resolved | `transform.SetParent(null)` before DontDestroyOnLoad |
| Memory cards not visible | High | ✅ Resolved | Replaced Mask+Image on Viewport with Rect Mask 2D |
| Close button not clickable | High | ✅ Resolved | Panel Image Raycast Target set to OFF |
| Panel proportions wrong | Medium | ✅ Resolved | Panel anchor set to centre point |
| Post processing not rendering | High | ✅ Resolved | PlayerCamera needed Post Processing enabled |
| PP values calculating but not changing | Medium | ✅ Resolved | Individual value checkboxes in PP profile must be ticked blue |
| Tab key not opening reflect screen | Medium | ✅ Resolved | Input action binding lost — rebind Tab if recurs |
| HUD dots not updating | Medium | ✅ Resolved | Subscriptions moved inside Invoke-delayed InitialBuild() |
| Moment prompt persists after memory taken | Low | ✅ Resolved | HideMomentPrompt() called in ExperienceMoment() |
| Audio Listener warning | Low | ✅ Resolved | Audio Listener added to PlayerCamera |
| Moment sting plays from wrong point | Low | ✅ Resolved | momentStingSource.time offset in TriggerMomentSting() |
| Jump not working | Medium | ✅ Resolved | Changed ForceMode to VelocityChange, Jump Force to 6 |
| anyChanged compiler warning | Low | ✅ Resolved | Variable removed from Update() |
| OnMemoriesChanged invoked from outside MemorySystem | Low | ✅ Resolved | Added NotifyMemoriesChanged() public method |

---

*Document version 6 — Scaffolding Complete. All systems built and working. World building tracked separately in YAWYK_WorldBuild_v1.md*
