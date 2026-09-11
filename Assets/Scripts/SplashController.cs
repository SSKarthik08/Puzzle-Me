using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashController : MonoBehaviour
{
    [Header("VIDEO (optional)")]
    [Tooltip("If assigned, the splash plays this video and advances when it finishes. If left empty, falls back to a plain timer using Splash Duration below.")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("AUDIO (optional)")]
    [Tooltip("If your video has no embedded audio, assign a separate AudioSource here — it starts playing at the same moment as the video.")]
    [SerializeField] private AudioSource audioSource;

    [Header("TIMING")]
    [Tooltip("Used only if no Video Player is assigned above.")]
    [SerializeField] private float splashDuration = 3f;

    [Tooltip("Safety net — advances anyway if the video hasn't finished by this long (e.g. it failed to load).")]
    [SerializeField] private float fallbackMaxDuration = 8f;

    [Header("NEXT SCENE")]
    [SerializeField] private string loadingSceneName = "Loading";

    private bool hasAdvanced;


    private void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached +=
                OnVideoFinished;

            videoPlayer.Play();

            if (audioSource != null)
            {
                audioSource.Play();
            }

            // Safety net in case the video fails to play or
            // loopPointReached never fires for some reason.
            Invoke(
                nameof(GoToLoadingScene),
                fallbackMaxDuration
            );
        }
        else
        {
            Invoke(
                nameof(GoToLoadingScene),
                splashDuration
            );
        }
    }


    private void OnVideoFinished(VideoPlayer source)
    {
        GoToLoadingScene();
    }


    private void GoToLoadingScene()
    {
        // Guard against being called twice (video finish +
        // fallback timeout both landing).
        if (hasAdvanced)
            return;

        hasAdvanced = true;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -=
                OnVideoFinished;
        }

        if (audioSource != null &&
            audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log(
            "SPLASH DONE — LOADING '" +
            loadingSceneName +
            "'"
        );

        SceneManager.LoadScene(loadingSceneName);
    }
}