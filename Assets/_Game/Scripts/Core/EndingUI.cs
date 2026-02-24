// EndingUI.cs
// Displays the ending passages one by one.
// Each passage fades in, holds, then fades out before the next.
// The final passage holds indefinitely — no prompt to continue.
// The player sits with it.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EndingUI : MonoBehaviour
{
    // -------------------------------------------------------
    // REFERENCES
    // -------------------------------------------------------
    [Header("References")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI passageText;
    public TextMeshProUGUI passageTypeLabel;  // subtle label: "memory" "identity" etc.
    public Image backgroundImage;

    // -------------------------------------------------------
    // FEEL
    // -------------------------------------------------------
    [Header("Feel")]
    public float textFadeInDuration = 2f;
    public float textFadeOutDuration = 1.5f;
    public float minimumHoldTime = 4f;

    // Colour per passage type — sets the subtle background tint
    [Header("Passage Colours")]
    public Color openingColour  = new Color(0.05f, 0.05f, 0.08f);
    public Color memoryColour   = new Color(0.05f, 0.08f, 0.06f);
    public Color identityColour = new Color(0.08f, 0.05f, 0.06f);
    public Color worldColour    = new Color(0.05f, 0.06f, 0.09f);
    public Color closingColour  = new Color(0.03f, 0.03f, 0.04f);

    // -------------------------------------------------------
    // SHOW PASSAGES
    // Called by EndingSystem — coroutine that runs the whole sequence
    // -------------------------------------------------------
    public IEnumerator ShowPassages(List<EndingPassage> passages)
    {
        // Fade in the ending UI itself
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, 1.5f));

        for (int i = 0; i < passages.Count; i++)
        {
            var passage = passages[i];
            bool isFinal = i == passages.Count - 1;

            yield return StartCoroutine(ShowPassage(passage, isFinal));

            if (!isFinal)
                yield return new WaitForSeconds(0.5f);
        }
    }

    // -------------------------------------------------------
    // SHOW A SINGLE PASSAGE
    // -------------------------------------------------------
    private IEnumerator ShowPassage(EndingPassage passage, bool isFinal)
    {
        // Set background colour for this passage type
        if (backgroundImage != null)
        {
            Color targetColour = GetPassageColour(passage.type);
            StartCoroutine(LerpColour(backgroundImage, targetColour, 2f));
        }

        // Set type label
        if (passageTypeLabel != null)
        {
            passageTypeLabel.text = passage.type.ToString().ToUpper();
            passageTypeLabel.alpha = 0f;
        }

        // Set text (invisible)
        passageText.text = passage.text;
        passageText.alpha = 0f;

        // Fade in type label subtly
        if (passageTypeLabel != null)
            StartCoroutine(FadeTextAlpha(passageTypeLabel, 0f, 0.3f, 2f));

        // Fade in text
        yield return StartCoroutine(
            FadeTextAlpha(passageText, 0f, 1f, textFadeInDuration));

        // Hold
        float holdTime = Mathf.Max(minimumHoldTime, passage.displayDuration);
        yield return new WaitForSeconds(holdTime);

        // Final passage never fades out — player sits with it
        if (isFinal) yield break;

        // Fade out
        yield return StartCoroutine(
            FadeTextAlpha(passageText, 1f, 0f, textFadeOutDuration));

        if (passageTypeLabel != null)
            yield return StartCoroutine(
                FadeTextAlpha(passageTypeLabel, 0.3f, 0f, textFadeOutDuration));
    }

    // -------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------
    private Color GetPassageColour(PassageType type)
    {
        switch (type)
        {
            case PassageType.Opening:  return openingColour;
            case PassageType.Memories: return memoryColour;
            case PassageType.Identity: return identityColour;
            case PassageType.World:    return worldColour;
            case PassageType.Closing:  return closingColour;
            default: return openingColour;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg,
        float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        cg.alpha = to;
    }

    private IEnumerator FadeTextAlpha(TextMeshProUGUI text,
        float from, float to, float duration)
    {
        float elapsed = 0f;
        text.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        text.alpha = to;
    }

    private IEnumerator LerpColour(Image image, Color target, float duration)
    {
        float elapsed = 0f;
        Color start = image.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.color = Color.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        image.color = target;
    }
}