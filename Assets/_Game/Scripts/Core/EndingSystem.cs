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
    // REGISTRATION — called by EndingUI on Start
    // -------------------------------------------------------
    public void RegisterEndingUI(EndingUI ui)
    {
        endingUI = ui;
        Debug.Log("[EndingSystem] EndingUI registered successfully");
    }

    // -------------------------------------------------------
    // PUBLIC — trigger from anywhere
    // -------------------------------------------------------
    public void TriggerEnding()
    {
        if (endingTriggered) return;

        // Find MasterFade in the current scene
        masterFade = GameObject.Find("MasterFade")?.GetComponent<CanvasGroup>();

        // If EndingUI wasn't registered via Start(), find it now (including inactive objects)
        if (endingUI == null)
        {
            endingUI = Object.FindFirstObjectByType<EndingUI>(FindObjectsInactive.Include);
            if (endingUI != null)
                Debug.Log("[EndingSystem] EndingUI found via fallback search");
        }

        if (endingUI == null)
            Debug.LogError("[EndingSystem] EndingUI still not found. Make sure EndingUI.cs is attached to EndingScreen in the UICanvas > Screens hierarchy.");

        if (masterFade == null)
            Debug.LogWarning("[EndingSystem] MasterFade not found. Make sure an object named exactly 'MasterFade' exists in the scene.");

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

        // Fade to black
        yield return StartCoroutine(FadeToBlack(fadeToBlackDuration));

        // Sit in darkness briefly
        yield return new WaitForSeconds(pauseBeforePassages);

        // Show ending passages
        if (endingUI != null)
        {
            endingUI.gameObject.SetActive(true);
            yield return StartCoroutine(endingUI.ShowPassages(passages));
        }
        else
        {
            Debug.LogError("[EndingSystem] Cannot show passages — EndingUI is null.");
        }

        yield return new WaitForSeconds(pauseAfterFinal);

        Debug.Log("[EndingSystem] Sequence complete");
    }

    // -------------------------------------------------------
    // FADE TO BLACK
    // -------------------------------------------------------
    private IEnumerator FadeToBlack(float duration)
    {
        if (masterFade == null)
        {
            Debug.LogWarning("[EndingSystem] Skipping fade — MasterFade is null.");
            yield break;
        }

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