// MomentPromptUI.cs
// A quiet prompt that fades in when the player is near a moment.
// Should feel like a whisper, not a UI notification.
// Attach to the MomentPrompt object under HUD.

using UnityEngine;
using TMPro;

public class MomentPromptUI : MonoBehaviour
{
    // -------------------------------------------------------
    // REFERENCES — assign in Inspector
    // -------------------------------------------------------
    [Header("References")]
    public TextMeshProUGUI promptText;
    public CanvasGroup canvasGroup;     // Controls fade

    // -------------------------------------------------------
    // FEEL
    // -------------------------------------------------------
    [Header("Feel")]
    public float fadeSpeed = 3f;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private float targetAlpha = 0f;

    // -------------------------------------------------------
    // UPDATE — smooth fade
    // -------------------------------------------------------
    private void Update()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = Mathf.Lerp(
            canvasGroup.alpha,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );
    }

    // -------------------------------------------------------
    // PUBLIC
    // -------------------------------------------------------
    public void Show(string text)
    {
        promptText.text = text;
        targetAlpha = 1f;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        targetAlpha = 0f;
    }
}