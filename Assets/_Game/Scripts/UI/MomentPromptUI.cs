// MomentPromptUI.cs
using UnityEngine;
using TMPro;

public class MomentPromptUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI promptText;
    public CanvasGroup canvasGroup;

    [Header("Feel")]
    public float fadeSpeed = 3f;

    private float targetAlpha = 0f;
    private bool isHiding = false;

    private void Awake()
    {
        // Start invisible and with no text
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        targetAlpha = 0f;
    }

    private void Update()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = Mathf.Lerp(
            canvasGroup.alpha,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );

        // Once fully faded out, clear the text
        if (isHiding && canvasGroup.alpha < 0.01f)
        {
            promptText.text = "";
            isHiding = false;
        }
    }

    public void Show(string text)
    {
        isHiding = false;
        promptText.text = text;
        targetAlpha = 1f;
    }

    public void Hide()
    {
        isHiding = true;
        targetAlpha = 0f;
    }
}