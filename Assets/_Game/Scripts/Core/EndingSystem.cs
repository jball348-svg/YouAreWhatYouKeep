// EndingSystem.cs
// Orchestrates the full ending sequence.
// Finds UI references at runtime rather than requiring cross-scene Inspector wiring.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndingSystem : MonoBehaviour
{
    // -------------------------------------------------------
    // SINGLETON
    // -------------------------------------------------------
    public static EndingSystem Instance { get; private set; }

    // -------------------------------------------------------
    // CONFIGURATION
    // -------------------------------------------------------
    [Header("Timing")]
    public float fadeToBlackDuration = 3f;
    public float pauseBeforePassages = 2f;
    public float fadeInPassageDuration = 1.5f;
    public float pauseAfterFinal = 4f;

    // -------------------------------------------------------
    // PRIVATE — found at runtime
    // -------------------------------------------------------
    private EndingUI endingUI;
    private CanvasGroup masterFade;
    private bool endingTriggered = false;

    // -------------------------------------------------------
    // AWAKE
    // -------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // -------------------------------------------------------
    // PUBLIC — trigger from anywhere
    // -------------------------------------------------------
    public void TriggerEnding()
    {
        if (endingTriggered) return;

        // Find UI references now — they exist in the current scene
        endingUI = Object.FindFirstObjectByType<EndingUI>();
        masterFade = GameObject.Find("MasterFade")?.GetComponent<CanvasGroup>();

        if (endingUI == null)
            Debug.LogWarning("[EndingSystem] EndingUI not found in scene");

        if (masterFade == null)
            Debug.LogWarning("[EndingSystem] MasterFade not found in scene");

        endingTriggered = true;
        Debug.Log("[EndingSystem] Ending triggered");
        StartCoroutine(EndingSequence());
    }

    // -------------------------------------------------------
    // ENDING SEQUENCE
    // -------------------------------------------------------
    private IEnumerator EndingSequence()
    {
        GameManager.Instance.SetGameState(GameManager.GameState.Ending);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        List<EndingPassage> passages = EndingNarrator.GenerateEnding();

        yield return StartCoroutine(FadeToBlack(fadeToBlackDuration));

        yield return new WaitForSeconds(pauseBeforePassages);

        if (endingUI != null)
        {
            endingUI.gameObject.SetActive(true);
            yield return StartCoroutine(endingUI.ShowPassages(passages));
        }

        yield return new WaitForSeconds(pauseAfterFinal);

        Debug.Log("[EndingSystem] Sequence complete");
    }

    // -------------------------------------------------------
    // FADE TO BLACK
    // -------------------------------------------------------
    private IEnumerator FadeToBlack(float duration)
    {
        if (masterFade == null) yield break;

        float elapsed = 0f;
        masterFade.alpha = 0f;
        masterFade.gameObject.SetActive(true);

        StartCoroutine(FadeAudioOut(duration));

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            masterFade.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        masterFade.alpha = 1f;
    }

    private IEnumerator FadeAudioOut(float duration)
    {
        float elapsed = 0f;
        AudioListener.volume = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            AudioListener.volume = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        AudioListener.volume = 0f;
    }
}