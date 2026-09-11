using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    [Header("NEXT SCENE")]
    [Tooltip("Set this once your Main Menu / Level Map scene exists and is added to Build Settings.")]
    [SerializeField] private string nextSceneName = "MainMenu";

    [Header("UI")]
    [Tooltip("Optional — use if your progress bar is a Slider.")]
    [SerializeField] private Slider progressSlider;

    [Tooltip("Optional — use if your progress bar is an Image with Fill Amount (Filled type).")]
    [SerializeField] private Image progressFillImage;

    [Header("TIMING")]
    [Tooltip("Loading screen stays up at least this long, even if the scene loads instantly.")]
    [SerializeField] private float minimumDisplayTime = 1.5f;


    private void Start()
    {
        StartCoroutine(
            LoadNextScene()
        );
    }


    // =========================================================
    // LOAD NEXT SCENE
    // =========================================================

    private IEnumerator LoadNextScene()
    {
        // If something set an override (e.g. Home's Play button
        // wants "Gameplay" instead of whatever's in the
        // Inspector), use that — and clear it immediately so it
        // doesn't leak into some later, unrelated load.
        string targetScene =
            nextSceneName;

        if (!string.IsNullOrEmpty(PuzzleProgress.NextSceneOverride))
        {
            targetScene =
                PuzzleProgress.NextSceneOverride;

            PuzzleProgress.NextSceneOverride = "";
        }

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(targetScene);

        if (operation == null)
        {
            Debug.LogError(
                "COULD NOT START LOADING '" +
                targetScene +
                "' — CHECK IT'S SPELLED CORRECTLY AND ADDED " +
                "TO FILE > BUILD PROFILES > SCENES IN BUILD."
            );

            yield break;
        }

        // Unity caps progress at 0.9 until activation is
        // allowed — holding activation back lets us show a
        // true 0-100% bar instead of it jumping 0->90->100.
        operation.allowSceneActivation = false;

        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;

            float timeBasedProgress =
                Mathf.Clamp01(elapsed / minimumDisplayTime);

            float loadBasedProgress =
                Mathf.Clamp01(operation.progress / 0.9f);

            // The bar fills at whichever pace is SLOWER, so it
            // always visibly animates over at least
            // minimumDisplayTime — even if the actual scene
            // load finishes almost instantly (very common in
            // the Editor).
            float displayProgress =
                Mathf.Min(
                    timeBasedProgress,
                    loadBasedProgress
                );

            SetProgress(displayProgress);

            bool timeDone =
                elapsed >= minimumDisplayTime;

            bool loadDone =
                operation.progress >= 0.9f;

            if (timeDone && loadDone)
                break;

            yield return null;
        }

        SetProgress(1f);

        // Let the fully-filled bar actually render for a
        // moment before switching scenes.
        yield return new WaitForSeconds(0.15f);


        Debug.Log(
            "LOADING COMPLETE — ENTERING '" +
            targetScene +
            "'"
        );

        operation.allowSceneActivation = true;
    }


    // =========================================================
    // SET PROGRESS
    // =========================================================

    private void SetProgress(float value)
    {
        if (progressSlider != null)
        {
            progressSlider.value = value;
        }

        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = value;
        }
    }
}