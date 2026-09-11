using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeScreenController : MonoBehaviour
{
    [Header("CURRENT LEVEL DISPLAY")]
    [SerializeField] private TMPro.TMP_Text currentLevelText;

    [Header("BUTTONS")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button restartLevelsButton;
    [SerializeField] private Button exitGameButton;

    [Header("NEXT SCENE")]
    [Tooltip("Usually your Loading scene, which then continues into Gameplay.")]
    [SerializeField] private string sceneToLoad = "Loading";


    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayPressed);
        }

        if (restartLevelsButton != null)
        {
            restartLevelsButton.onClick.AddListener(OnRestartLevelsPressed);
        }

        if (exitGameButton != null)
        {
            exitGameButton.onClick.AddListener(OnExitGamePressed);
        }

        RefreshDisplay();
    }


    // =========================================================
    // REFRESH DISPLAY
    // =========================================================

    private void RefreshDisplay()
    {
        if (currentLevelText != null)
        {
            currentLevelText.text =
                "Level " + PuzzleProgress.CurrentLevel;
        }
    }


    // =========================================================
    // BUTTON HANDLERS
    // =========================================================

    private void OnPlayPressed()
    {
        PuzzleProgress.LevelToLoad =
            PuzzleProgress.CurrentLevel;

        // Tell the (shared) Loading scene to go to Gameplay
        // this time, instead of whatever its Inspector default
        // is set to (e.g. "Home", used by the Splash flow).
        PuzzleProgress.NextSceneOverride =
            "Gameplay";

        Debug.Log(
            "PLAY PRESSED — RESUMING LEVEL " +
            PuzzleProgress.CurrentLevel
        );

        SceneManager.LoadScene(sceneToLoad);
    }


    private void OnRestartLevelsPressed()
    {
        PuzzleProgress.ResetAllProgress();

        RefreshDisplay();

        Debug.Log(
            "ALL LEVEL PROGRESS RESET — BACK TO LEVEL 1"
        );
    }


    private void OnExitGamePressed()
    {
        Debug.Log(
            "EXIT GAME PRESSED"
        );

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}