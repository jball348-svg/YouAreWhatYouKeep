// EndingNarrator.cs
// Generates personalised ending text from the player's full profile.
// Reads: held memories, vividness, traits, emotional weight, echoes.
// Output: a series of text passages shown in the ending sequence.
//
// DESIGN INTENT:
// The narration should feel like the game knows you.
// Not because it's clever — because it's honest.
// It only says what actually happened.
// Players who lived differently read different words.

using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class EndingNarrator
{
    // -------------------------------------------------------
    // MAIN ENTRY POINT
    // Generates the full ending passage list
    // -------------------------------------------------------
    public static List<EndingPassage> GenerateEnding()
    {
        var passages = new List<EndingPassage>();

        // Gather all the data we need
        var memories = MemorySystem.Instance?.GetAllMemories()
            ?? new List<MemoryInstance>();
        var traitProfile = IdentitySystem.Instance?.GetFullProfile()
            ?? new Dictionary<TraitType, float>();
        var dominantTraits = IdentitySystem.Instance?.GetDominantTraits(0.65f)
            ?? new List<TraitType>();
        float emotionalWeight = MemorySystem.Instance?.GetTotalEmotionalWeight() ?? 0f;
        var echoes = WorldEchoSystem.Instance?.GetAllEchoes()
            ?? new List<WorldEchoSystem.WorldEcho>();
        string formattedTime = TimeSystem.Instance?.GetFormattedTime() ?? "an unknown time";

        // Build passages in sequence
        passages.Add(GenerateOpeningPassage(memories, formattedTime));
        passages.Add(GenerateMemoryPassage(memories));
        passages.Add(GenerateIdentityPassage(dominantTraits, traitProfile));
        passages.Add(GenerateWorldPassage(echoes, memories));
        passages.Add(GenerateClosingPassage(memories, emotionalWeight, dominantTraits));

        // Remove any null passages
        passages.RemoveAll(p => p == null || string.IsNullOrEmpty(p.text));

        return passages;
    }

    // -------------------------------------------------------
    // OPENING — sets the tone
    // -------------------------------------------------------
    private static EndingPassage GenerateOpeningPassage(
        List<MemoryInstance> memories, string time)
    {
        string text;

        if (memories.Count == 0)
        {
            text = $"You were here until {time}.\n\nYou kept nothing.\n\n" +
                   $"Maybe that was its own kind of answer.";
        }
        else if (memories.Count <= 2)
        {
            text = $"You were here until {time}.\n\n" +
                   $"You moved through this place quietly.\n\n" +
                   $"Some people leave deep marks. You left something gentler than that.";
        }
        else
        {
            text = $"You were here until {time}.\n\n" +
                   $"You lived in this place. Really lived in it.\n\n" +
                   $"That is rarer than it sounds.";
        }

        return new EndingPassage(text, PassageType.Opening);
    }

    // -------------------------------------------------------
    // MEMORIES — what was kept and what was let go
    // -------------------------------------------------------
    private static EndingPassage GenerateMemoryPassage(List<MemoryInstance> memories)
    {
        if (memories.Count == 0)
            return null;

        var sb = new StringBuilder();
        sb.AppendLine("You kept these:");
        sb.AppendLine();

        // Sort by vividness — freshest first
        memories.Sort((a, b) => b.vividness.CompareTo(a.vividness));

        foreach (var memory in memories)
        {
            string vividnessQualifier = GetVividnessQualifier(memory.vividness);
            sb.AppendLine($"  {memory.Title}");
            if (!string.IsNullOrEmpty(vividnessQualifier))
                sb.AppendLine($"  {vividnessQualifier}");
            sb.AppendLine();
        }

        // Count reflected-on memories
        int reflectedCount = memories.FindAll(m => m.hasBeenReflectedOn).Count;
        if (reflectedCount > 0)
        {
            sb.AppendLine(reflectedCount == memories.Count
                ? "You thought about all of them."
                : $"You thought carefully about {reflectedCount} of them.");
        }

        return new EndingPassage(sb.ToString(), PassageType.Memories);
    }

    // -------------------------------------------------------
    // IDENTITY — who the player became
    // -------------------------------------------------------
    private static EndingPassage GenerateIdentityPassage(
        List<TraitType> dominantTraits,
        Dictionary<TraitType, float> profile)
    {
        if (dominantTraits.Count == 0)
        {
            return new EndingPassage(
                "You didn't settle into any particular way of being.\n\n" +
                "You stayed open. That takes a kind of courage too.",
                PassageType.Identity
            );
        }

        var sb = new StringBuilder();
        sb.AppendLine("By the end, you had become:");
        sb.AppendLine();

        foreach (var trait in dominantTraits)
        {
            sb.AppendLine($"  {GetTraitDescription(trait)}");
        }

        // Check for interesting contradictions
        if (profile.ContainsKey(TraitType.Fearless) &&
            profile.ContainsKey(TraitType.Fragile))
        {
            float fearless = profile[TraitType.Fearless];
            float fragile = profile[TraitType.Fragile];

            if (fearless > 0.6f && fragile > 0.6f)
            {
                sb.AppendLine();
                sb.AppendLine("Both brave and breakable. Most people are one or the other.");
            }
        }

        if (profile.ContainsKey(TraitType.Calm) &&
            profile.ContainsKey(TraitType.Curious))
        {
            float calm = profile[TraitType.Calm];
            float curious = profile[TraitType.Curious];

            if (calm > 0.6f && curious > 0.6f)
            {
                sb.AppendLine();
                sb.AppendLine("Still, but always looking. That is a good way to be.");
            }
        }

        return new EndingPassage(sb.ToString(), PassageType.Identity);
    }

    // -------------------------------------------------------
    // WORLD — what traces were left
    // -------------------------------------------------------
    private static EndingPassage GenerateWorldPassage(
        List<WorldEchoSystem.WorldEcho> echoes,
        List<MemoryInstance> memories)
    {
        var sb = new StringBuilder();

        int memoryEchoes = echoes.FindAll(
            e => e.type == WorldEchoSystem.EchoType.MemoryFormed).Count;
        int lingerEchoes = echoes.FindAll(
            e => e.type == WorldEchoSystem.EchoType.Lingered).Count;

        if (echoes.Count == 0)
        {
            sb.AppendLine("The world won't remember you were here.");
            sb.AppendLine();
            sb.AppendLine("That's alright. You remember.");
        }
        else
        {
            sb.AppendLine("You left marks in this place.");
            sb.AppendLine();

            if (lingerEchoes > 3)
                sb.AppendLine("You stayed in places longer than most people do.");

            if (lingerEchoes > 0 && lingerEchoes <= 3)
                sb.AppendLine("You found a few places worth staying in.");

            if (memoryEchoes > 0)
            {
                sb.AppendLine();
                sb.AppendLine(memoryEchoes == 1
                    ? "One place will hold a warmth for a while."
                    : $"{memoryEchoes} places will hold a warmth for a while.");
            }

            sb.AppendLine();
            sb.AppendLine("The world is slightly different for your having been in it.");
        }

        return new EndingPassage(sb.ToString(), PassageType.World);
    }

    // -------------------------------------------------------
    // CLOSING — the final word
    // -------------------------------------------------------
    private static EndingPassage GenerateClosingPassage(
        List<MemoryInstance> memories,
        float emotionalWeight,
        List<TraitType> dominantTraits)
    {
        string text;

        bool hasSolitude = memories.Exists(
            m => m.Category == MemoryCategory.Solitude);
        bool hasRisk = memories.Exists(
            m => m.Category == MemoryCategory.Risk);
        bool hasStillness = memories.Exists(
            m => m.Category == MemoryCategory.Stillness);
        bool hasWonder = memories.Exists(
            m => m.Category == MemoryCategory.Wonder);
        bool isMelancholic = dominantTraits.Contains(TraitType.Melancholic);
        bool isFearless = dominantTraits.Contains(TraitType.Fearless);
        bool isCalm = dominantTraits.Contains(TraitType.Calm);

        if (emotionalWeight > 3f && isFearless)
        {
            text = "You lived with your whole weight.\n\n" +
                   "Not everyone does.";
        }
        else if (isCalm && hasStillness)
        {
            text = "You found something in the quiet.\n\n" +
                   "That's harder than it looks.";
        }
        else if (hasRisk && !isFearless)
        {
            text = "You went to the edges even when it scared you.\n\n" +
                   "That counts for something.";
        }
        else if (isMelancholic && hasWonder)
        {
            text = "You saw the beauty in things and felt the weight of it.\n\n" +
                   "That's not a flaw. That's just being alive to it.";
        }
        else if (hasSolitude && memories.Count <= 3)
        {
            text = "You kept your own company well.\n\n" +
                   "Some people never learn that.";
        }
        else if (memories.Count == 0)
        {
            text = "You were here.\n\nThat was enough.";
        }
        else
        {
            text = "You lived a life worth having.\n\n" +
                   "That's the whole point, isn't it.";
        }

        return new EndingPassage(text, PassageType.Closing);
    }

    // -------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------
    private static string GetVividnessQualifier(float vividness)
    {
        if (vividness > 0.85f) return string.Empty; // fresh — no qualifier needed
        if (vividness > 0.6f)  return "  (already fading a little)";
        if (vividness > 0.35f) return "  (growing distant now)";
        return "  (barely more than a feeling)";
    }

    private static string GetTraitDescription(TraitType trait)
    {
        switch (trait)
        {
            case TraitType.Fearless:   return "Someone who went to the edges";
            case TraitType.Fragile:    return "Someone who felt things deeply";
            case TraitType.Curious:    return "Someone who kept looking";
            case TraitType.Calm:       return "Someone who knew how to be still";
            case TraitType.Aware:      return "Someone who paid attention";
            case TraitType.Warm:       return "Someone who turned toward others";
            case TraitType.Agile:      return "Someone who moved through the world easily";
            case TraitType.Melancholic: return "Someone who understood that things end";
            case TraitType.Resilient:  return "Someone who kept going";
            case TraitType.Open:       return "Someone who stayed open to what came";
            default: return trait.ToString();
        }
    }
}

// -------------------------------------------------------
// SUPPORTING DATA
// -------------------------------------------------------
public class EndingPassage
{
    public string text;
    public PassageType type;
    public float displayDuration = 6f; // seconds before auto-advancing

    public EndingPassage(string text, PassageType type)
    {
        this.text = text;
        this.type = type;

        // Longer passages get more time
        int lineCount = text.Split('\n').Length;
        displayDuration = Mathf.Max(5f, lineCount * 1.5f);
    }
}

public enum PassageType
{
    Opening,
    Memories,
    Identity,
    World,
    Closing
}