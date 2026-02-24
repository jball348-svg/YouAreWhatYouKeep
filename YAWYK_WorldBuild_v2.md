# You Are What You Keep — World Building Plan
### Version 2 — Multi-Scene, Chapter Structure

> This document tracks the creative and environmental build of YAWYK.
> Scaffolding is complete — see YAWYK_ProjectProgress_v6_Scaffolding.md.
>
> **The game is structured as 4–5 chapters of a single life, set in a real city.
> Each chapter is a Unity scene. The player's memories, traits, and identity
> persist across all of them. Real places. The player projects their own story.**
>
> Build one chapter at a time. Don't think about Chapter 3 while building Chapter 1.

---

## The Game — High Level Vision

A life, told in places.

The player moves through 4–5 chapters, each a distinct period of one life, each set in a real city that the player (and the people who made this) actually know. The places are real — or real enough that someone who knows them will recognise them. The life is open enough that anyone can project their own onto it.

The world rewards the same things across every chapter:
- Lingering (echoes build, atmosphere deepens)
- Looking at things rather than past them
- Going off the obvious path
- Returning to places and finding them different

**The through-line:** The same city. Different light, different scale, different weight. The park that felt enormous at 8 feels different at 35. The street corner you walked past every day for years. The flat you lived in alone. The places that held your life without knowing it.

**The payoff:** Someone who knows these places plays this and recognises them. Someone who doesn't play it and thinks of their own places. Both responses are correct.

---

## Technical Structure — Multi-Scene Setup

Each chapter is a Unity scene in `Assets/_Game/Scenes/Locations/`:

```
Scenes/
├── Core/
│   ├── _Boot
│   └── _Persistent          ← all systems live here, never unload
└── Locations/
    ├── Chapter_01_Childhood      ← brief, establishes the place
    ├── Chapter_02_Adolescence    ← same places, different light
    ├── Chapter_03_YoungAdult     ← leaving, or just before
    ├── Chapter_04_Adult          ← return, family, now
    └── Chapter_05_Ending         ← optional — reflection space or fades into Ch4 ending
```

**How persistence works across scenes:**
- All game systems (`MemorySystem`, `IdentitySystem`, `TimeSystem` etc.) live in `_Persistent` and never unload
- When a chapter ends, `GameManager` loads the next scene — memories, traits, and identity all carry over automatically
- Each new scene has its own `UICanvas`, `Player` spawn point, `MomentTriggers`, and `EndingTrigger`
- The `EndingTrigger` in each chapter (except the last) loads the next chapter rather than triggering the ending sequence

**Transition feel:**
A chapter transition should feel like time passing — not a loading screen. Use `MasterFade` to black, a single line of narration or just silence, then fade in to the next chapter. The systems already support this. It fits the tone completely.

---

## Chapter Overview

| Chapter | Life Stage | Scene Name | Status |
|---------|-----------|------------|--------|
| 1 | Childhood — the place at its biggest | Chapter_01_Childhood | 🔲 |
| 2 | Adolescence — same places, different eyes | Chapter_02_Adolescence | 🔲 |
| 3 | Young adult — leaving, or the year before | Chapter_03_YoungAdult | 🔲 |
| 4 | Adult life — return, family, relationships, now | Chapter_04_Adult | 🔲 |
| 5 | Ending — reflection or departure | Chapter_05_Ending | 🔲 optional |

**Start with Chapter 1.** Build it completely before touching Chapter 2. The lessons you learn in Chapter 1 will change how you build everything after it.

---

## Build Phase Overview (per chapter)

Each chapter goes through the same phases. You don't do all chapters at once — you do all phases for Chapter 1, then move to Chapter 2.

| Phase | Focus |
|-------|-------|
| W1 | Design on paper — what is this chapter, what places, what memories |
| W2 | Blockout — geometry and scale in Unity |
| W3 | Lighting — time of day, mood, atmosphere |
| W4 | Memory placement — triggers, prompt text, new memory assets |
| W5 | Materials and textures |
| W6 | Sound |
| W7 | NPCs (if this chapter has them) |
| W8 | Polish and chapter transition |

---

---

# PHASE W1 — Design on Paper (Chapter 1 First)

**Goal:** Before opening Unity, know exactly what Chapter 1 is. Where it's set, what it feels like, what memories live there, how it ends and leads to Chapter 2.

**Time estimate:** One sitting. This is thinking time, not doing time.

**Output:** Written answers to the questions below. A rough map. A memory list. That's all you need to start building.

---

## Step 1 — The Life You're Building

This game is set in your real city, but the life is open enough for anyone to project onto. That means you're drawing from real places but not necessarily real events. The places are yours. The feelings are universal.

Answer these:

**What city is this?** Name it to yourself even if the game never names it explicitly. Knowing it's specific helps you build it specifically.

**What does this city feel like to someone who grew up here?** Not a tourist description — the felt experience. The smell of a particular street. The way the light hits a certain building. The sound of it on a Sunday morning versus a Friday night.

**What are the 3–4 places in this city that carry the most emotional weight for you?** These become chapter locations. They don't need to be dramatic or beautiful — often the most powerful places are ordinary. A bus stop. A park bench. A particular stretch of road.

**What time of year feels most like this city to you?** Cities have a season that is most themselves. For many British cities it's autumn — a specific grey-gold quality to the light, leaves on wet pavements, the smell of rain on warm stone. Pick one and it becomes the default atmosphere across the whole game.

---

## Step 2 — Chapter 1 Specifically

Chapter 1 is childhood. It's brief — probably the shortest chapter. Its job is to establish the geography of the life. The player sees the city for the first time at its most overwhelming scale.

Answer these for Chapter 1:

**What specific place does childhood happen?** A street, a park, a school, a back garden, a route walked every day. Pick one primary location — you can have secondary ones, but anchor it.

**What does this place feel like to a child?** Bigger than it is. Certain things loom. Certain things are mysterious. The world hasn't been explained yet.

**What time of day is it?** Morning, for a child, has a particular quality — the day is open, anything could happen. Late afternoon has a different weight. Pick one.

**What are the 3–5 memories that live in this chapter?** These should be childhood-specific. Simple, physical, sensory. Not dramatic — children's memories rarely are. Examples:
- Sitting under something large (a tree, a bridge) and feeling hidden
- Running somewhere and not stopping
- Finding something small and ordinary that felt important
- Watching something happen without understanding it
- Being somewhere alone for the first time

**How does Chapter 1 end?** The `EndingTrigger` here doesn't end the game — it transitions to Chapter 2. Where does the player leave from? A gate, a road, a door. What does leaving feel like at this age? (Probably not sad yet — just movement.)

---

## Step 3 — The City as a Character

Because you're building from a real place, you have something most game designers don't: actual reference material. Use it.

**Do this:**
- Walk or drive through the places you're planning to build. Take photos. Notice things you'd stopped seeing.
- Write down specific details — not "a park" but "the specific bench near the east entrance with the broken slat." Not "a street" but "the way the terraced houses step slightly uphill and the chimneys make a jagged line against the sky."
- Notice the light quality at different times of day. Cities have specific light depending on their geography, their building materials, how open or enclosed the streets are.

These specific details are what will make someone who knows this city feel it, and what will make someone who doesn't feel *their* city instead. Specificity creates universality. This sounds paradoxical but it's consistently true.

---

## Step 4 — The Chapter Map

Draw a rough top-down map of Chapter 1's location. Blobs and labels is fine.

**Every chapter map needs:**
- **A start point** — where the player spawns. What do they see first?
- **3–5 distinct areas** — not separate rooms, just areas with different character. A path, an open space, somewhere enclosed, somewhere elevated if possible.
- **A main route** — the obvious way through for players who don't explore
- **At least one off-path area** — somewhere you only find if you look. A hidden memory lives here.
- **An ending point** — the `EndingTrigger` that transitions to Chapter 2. A gate, a path out, a road. Somewhere that feels like leaving.

**Scale for a chapter:** Each chapter should take 5–10 minutes to explore properly. Not vast. Intimate enough that the player can learn it and feel at home in it before they have to leave.

---

## Step 5 — Memory List for Chapter 1

List every memory you want in Chapter 1. For each one, write:
- **Title** (the short name that appears in the memory slot)
- **Category** (from the existing list: Nature, Solitude, Connection, Risk, Creation, Loss, Wonder, Stillness)
- **One sentence description** (what the player reads when they reflect on this memory)
- **Where in the map it lives**
- **How it's triggered** (press E, linger, proximity auto-trigger)
- **Prompt text** (the 3–5 words that appear on screen)

**Target for Chapter 1:** 4–6 memories. Enough to fill most of the player's slots before they leave — because leaving with a full inventory means they have to make a choice about what to carry into Chapter 2. That choice is the game working.

---

## Step 6 — The Transition Moment

When the player reaches the `EndingTrigger` in Chapter 1, what happens?

Design this now, even if you implement it later:
- Does the screen fade to black silently?
- Is there a single line of text? (e.g. "Years pass." or just the chapter title: "Adolescence")
- Does the audio shift immediately or gradually?
- Where does the player appear in Chapter 2, and what do they see first?

The transition between chapters is one of the most powerful moments in the game. It should feel like time passing — like turning a page.

---

## Stopping Point for W1

You're done with W1 when you have, written down somewhere:
- ✅ The city named and described in felt terms
- ✅ The 3–4 real places that become chapter locations
- ✅ Chapter 1 fully designed: location, map, 4–6 memories, transition
- ✅ A rough sense of what Chapters 2–4 are (you don't need detail yet — just enough to know Chapter 1 is pointing in the right direction)

**Don't start W2 until W1 is done. The blockout will be faster and better if you know exactly what you're building.**

---

---

# PHASE W2 — Blockout (Chapter 1)

**Goal:** Build the shape of Chapter 1 in Unity using primitive geometry. No textures. No detail. Just the ground, the spaces, and the scale. Get it walkable.

**Open scene:** `Chapter_01_Childhood`

---

## Step 1 — Scene Setup

- Duplicate the `Location_Opening` scene as a starting point (it has Player, UICanvas, and the right hierarchy already)
- Rename it `Chapter_01_Childhood`
- Add it to Build Settings (File → Build Settings → Add Open Scenes)
- Delete the old test plane and any test moment triggers
- Create `--- ENVIRONMENT ---` as an empty root GameObject — all geometry goes inside it

---

## Step 2 — Ground and Elevation

Use Unity **Terrain** for organic environments or scaled **Cubes/Planes** for more architectural/urban spaces. For a city environment, a mix works well:
- Flat pavement areas: scaled Planes or Cubes
- Parks and open ground: Terrain works better
- Streets: long thin Planes with building Cubes either side

**Everything walkable must be on the `Ground` layer** — the player's jump check depends on this.

**Urban scale references:**
- Standard pavement width: 2–3m
- Road width (single lane): 3.5m
- Terraced house frontage: 5–7m wide, 8–10m tall
- A small urban park: 50–80m across feels right for this game's scale
- Tree height: 8–15m for mature street trees

---

## Step 3 — Block Each Zone

Go zone by zone from your W1 map. For each zone:
- Block in the basic shape with primitives
- Name the root object after the zone (e.g. `ZONE_Park`, `ZONE_BackStreet`)
- Walk through it in Play mode — does the scale feel right?

**For city environments specifically:**
- Streets feel more real if they're slightly enclosed — buildings either side even as simple cubes make an enormous difference
- Open spaces (parks, squares) need at least one landmark to orientate around — a tree stand-in cube, a central feature
- Varying ceiling height matters — the gap between terraced houses versus an open square versus under a bridge all feel different and create emotional variety

---

## Step 4 — Navigation Pass

Walk the whole chapter in Play mode. Check:
- Can you get everywhere naturally?
- Does the off-path area feel genuinely off-path, not just slightly to the side?
- Does following the main route feel like moving through a place, not a corridor?
- Does the ending point feel like departure?

**City-specific navigation note:** Streets are linear by nature — resist the temptation to make the whole chapter one long street. Add a turning, a cut-through, a space that opens up unexpectedly. Urban environments that feel good to move through always have rhythm — enclosed, open, enclosed, open.

---

## Step 5 — Place Memory Markers

Empty GameObjects named `MARKER_[MemoryName]` at each planned trigger location. Walk to each one and ask:
- Does it feel right to have a memory form here?
- Is the approach natural — does the player discover it, or does it feel planted?
- Does the prompt text from W1 still make sense standing in this physical space?

---

## Stopping Point for W2

- ✅ Chapter 1 is walkable end-to-end
- ✅ All zones blocked out
- ✅ Scale feels right (tested by walking, not by measuring)
- ✅ Memory marker positions feel good
- ✅ Ending point is physically present

**Commit. Then W3.**

---

---

# PHASE W3 — Lighting (Chapter 1)

**Goal:** Make Chapter 1 feel like a time of day and a time of life. Lighting is the fastest way to create emotional atmosphere.

---

## City Lighting Considerations

Cities have different light than countryside. Key differences:
- **Reflected light** — buildings bounce light around. Shaded areas in cities are rarely as dark as in open nature.
- **Artificial light** — even daytime urban environments have lit shop windows, illuminated signs, interior light spilling through windows. These small lights matter.
- **Sky visibility** — cities often have narrow strips of sky between buildings. The colour of that strip is the mood.

---

## Step 1 — Time of Day for Chapter 1

Chapter 1 is childhood. Some options and what they imply:

**Morning (8–9am):** The day hasn't decided what it is yet. Open, slightly uncertain. A child's morning. Recommended if the chapter has a hopeful or exploratory tone.

**After school (3–4pm):** Golden afternoon light, the particular freedom of the end of a school day. Warm, slightly elevated, good shadows.

**Sunday afternoon (2–3pm):** A specific stillness. Quieter than weekdays. The city at rest. Works well for a chapter that's more contemplative.

Set `Directional Light` rotation and colour accordingly. For afternoon UK city light: warm but not aggressively golden. The light in British cities has a particular soft quality — filtered through cloud even on clear days. Avoid pure bright white or intense orange. Aim for something warmer than grey but subtler than full golden hour.

---

## Step 2 — Skybox

For a city environment, a simple gradient sky works better than a dramatic HDRI. Something with:
- A slightly hazy horizon (cities have atmospheric haze)
- The suggestion of cloud without heavy overcast
- A colour that reads as the right time of day

Unity's default procedural skybox is actually reasonable — adjust Atmosphere Thickness and ground/sky colours to suit.

---

## Step 3 — Fog

Essential for cities. Use **Linear fog** (more controllable than exponential in urban environments):
- Start Distance: 40–60m (so near geometry is crisp)
- End Distance: 150–200m (geometry fades out rather than hard-cutting)
- Fog Colour: should match your sky's horizon colour almost exactly

This does two things: hides geometry edges (you don't have to model everything), and makes the space feel real — cities always have atmospheric depth.

---

## Step 4 — Artificial Light Points

Add small Point Lights or Spot Lights for:
- Shop window spill (warm yellow, low intensity, close range)
- Any interior light visible through a window
- Street lamps if it's evening (even in daytime they can be on)

These don't need to be functional. They need to suggest a world that exists beyond what the player can see.

---

## Stopping Point for W3

- ✅ Time of day established and feels right for this chapter's life stage
- ✅ Skybox and fog set
- ✅ Directional light colour and angle feel right for a real city at this time
- ✅ At least a few point lights suggesting the world beyond the playable space
- ✅ Walking through it feels like being somewhere specific

**Commit. Then W4.**

---

---

# PHASE W4 — Memory Placement (Chapter 1)

**Goal:** Wire up all memory triggers. Write final prompt text and descriptions. Add any new memories that emerged from actually walking the blockout.

---

## Childhood Memory Guidance

Chapter 1 memories should be:
- **Sensory and physical** — what children remember is how things felt, looked, smelled. Not events, experiences.
- **Small scale** — the specific, not the general. Not "played in the park" but "found something under the bench"
- **Slightly mysterious** — children don't always understand what they're experiencing. Memories from this chapter can be more ambiguous than later ones.
- **Tinged with scale** — the world being bigger than it is now. Distances that were enormous. Spaces that felt huge.

**Good childhood memory categories for this game:** Wonder, Solitude, Nature, Stillness. Connection (a specific person, briefly). Risk (something small but felt enormous at the time).

---

## Step 1 — Create Memory ScriptableObjects

For each memory in your W1 list that doesn't exist yet:
- Right-click `ScriptableObjects/Memories` → Memory → New Memory
- Name it `MEM_[ShortTitle]`
- Fill in all fields
- Choose a `worldTintContribution` colour — for childhood memories, consider softer, slightly washed-out colours. Memories from this period have a particular quality of light.

---

## Step 2 — Place and Configure Each Trigger

Replace each `MARKER_` object with a `MEM_MomentTrigger_Base` prefab. Configure:
- `memoryData` → assign the ScriptableObject
- `triggerRadius` → smaller in enclosed spaces (2–2.5m), larger in open areas (3.5–4m)
- `lingerTime` → childhood memories suit lingering. Consider 3–5s for most of them — the player has to stay in the spot, like a child would.
- `promptText` → short, present tense, inviting. "Stay a while." "Look up." "Just stand here."

---

## Step 3 — The Chapter Transition Trigger

The `EndingTrigger` in Chapter 1 doesn't end the game — it loads Chapter 2. 

For now, you can use the existing `EndingTrigger.cs` to fade to black, and then manually load the next scene. Later, a proper `ChapterTransition.cs` script will handle this more elegantly (loading the next scene, displaying a transition line of text, fading back in). Document this as a future hook for now.

**Temporary implementation:** In `EndingTrigger.cs`, add a `sceneToLoad` string field. If it's populated, load that scene instead of triggering the ending sequence. This is a one-line change you can make now and replace properly later.

---

## Stopping Point for W4

- ✅ All Chapter 1 memories placed and wired
- ✅ All prompt texts feel right in physical context
- ✅ Chapter transition trigger is in place (even if implementation is temporary)
- ✅ Full walkthrough: enter chapter, find memories, leave. Does it feel like a chapter of a life?

**Commit. Then W5.**

---

---

# PHASES W5–W8 — Same as Before, Chapter by Chapter

Phases W5 (Materials), W6 (Sound), W7 (NPCs), and W8 (Polish) follow the same approach as described in the original world build plan, applied to Chapter 1 first.

**The key difference for a multi-chapter game:**

Each chapter should have a **distinct visual and audio signature** that reflects the life stage. Consider:

| Chapter | Suggested Visual Tone | Suggested Audio Character |
|---------|-----------------------|--------------------------|
| 1 — Childhood | Slightly washed out, soft edges, high contrast on specific things | Distant, simple. Birdsong. Wind. Very little complexity. |
| 2 — Adolescence | Moodier. More contrast. Longer shadows. Night-adjacent. | More present. Music bleeding from somewhere. The world getting louder. |
| 3 — Young Adult | Cooler, more urban. Interior light. Artificial warmth. | Traffic. The particular sound of living alone. |
| 4 — Adult | Warmer than it used to be. Fuller. More settled light. | Layered. More going on. Quieter underneath. |

The `EmotionalResponseSystem` handles per-memory colour shifts — but the **base** lighting and audio atmosphere should already feel distinct per chapter before any memories are collected.

---

---

# CHAPTER DESIGN NOTES (Fill In As You Go)

## Chapter 1 — Childhood
**Location:** _(write the real place here)_
**Time of day:** _(morning / after school / Sunday afternoon)_
**Season/weather:** _(autumn / late summer / winter light)_
**Memories:** _(list as you finalise them)_
**Transition line:** _(the single line of text, if any, when leaving this chapter)_
**Notes:** _(anything that came up while building)_

---

## Chapter 2 — Adolescence
**Location:** _(write the real place here)_
**Time of day:** _
**Season/weather:** _
**Memories:** _
**Transition line:** _
**Notes:** _

---

## Chapter 3 — Young Adult
**Location:** _(write the real place here)_
**Time of day:** _
**Season/weather:** _
**Memories:** _
**Transition line:** _
**Notes:** _

---

## Chapter 4 — Adult Life
**Location:** _(write the real place here)_
**Time of day:** _
**Season/weather:** _
**Memories:** _
**Transition line:** _
**Notes:** _

---

## Chapter 5 — Ending (if separate scene)
**Concept:** _
**Notes:** _

---

---

# FUTURE TECHNICAL HOOKS (World Build Phase)

These will need to be built — document them here so they don't get forgotten.

**ChapterTransition.cs** — A proper script to handle loading the next chapter scene, displaying a transition title or line of text, managing fade in/out. Currently using EndingTrigger as a temporary stand-in.

**Chapter-aware EndingNarrator** — Currently EndingNarrator reads all memories at the end regardless of which chapter they came from. Later, it could reference which chapter a memory came from and weight the narrative accordingly. ("You carried this since childhood" vs "You found this only recently.")

**Memory provenance on MemoryInstance** — Add a `string chapterOrigin` field to `MemoryInstance` so the narrator knows where a memory came from. Wire in after all chapters are built.

**Per-chapter ambient audio profiles** — Currently AudioManager has one set of category layers. Later, each chapter loads its own ambient profile. This requires AudioManager to support scene-aware layer swapping.

---

*Document version 2 — Updated for multi-scene chapter structure. Real city, 4–5 chapters, young adult and adult life as primary chapters. Build Chapter 1 completely before starting Chapter 2.*
