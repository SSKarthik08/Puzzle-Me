using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PuzzleBoard : MonoBehaviour
{
    [Header("LEVEL")]
    [SerializeField] private PuzzleLevelDatabase levelDatabase;
    [SerializeField] private int currentLevel = 1;

    [Header("IMAGE")]
    [SerializeField] private Texture2D[] puzzleImages;

    [Header("BOARD")]
    [SerializeField] private float tileSize = 2f;
    [SerializeField] private float tileGap = 0.475f;

    [Header("REFERENCE IMAGE")]
    [SerializeField] private Image referenceImageUI;

    [Header("WIN UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMPro.TMP_Text winText;
    [SerializeField] private GameObject finalWinPanel;

    [Header("TIMER")]
    [Tooltip("Fallback only — real per-level time limits come from PuzzleLevelDatabase's Time Limit Tiers.")]
    [SerializeField] private float timeLimitSeconds = 300f;
    [SerializeField] private TMPro.TMP_Text timerText;
    [SerializeField] private GameObject timeUpPanel;

    [Header("MOVES")]
    [SerializeField] private TMPro.TMP_Text movesText;

    [Header("PAUSE / SETTINGS")]
    [SerializeField] private GameObject settingsPanel;

    private float timeRemaining;
    private bool isTimeUp;
    private bool isPaused;
    private int movesCount;

    private PuzzleTile[] tiles;
    private PuzzleTile selectedTile;

    private int gridSize;
    private int shuffleMoves;

    private bool puzzleComplete;

    private Texture2D currentImage;

    private List<Texture2D> remainingImages;

    public int GridSize => gridSize;
    public float TileSize => tileSize;
    public float TileGap => tileGap;


    private void Start()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (finalWinPanel != null)
        {
            finalWinPanel.SetActive(false);
        }

        int levelToStart =
            PuzzleProgress.LevelToLoad > 0 ?
                PuzzleProgress.LevelToLoad :
                currentLevel;

        StartLevel(levelToStart);
    }


    private void Update()
    {
        if (!puzzleComplete && !isTimeUp && !isPaused)
        {
            TickTimer();
        }

        if (puzzleComplete || isTimeUp || isPaused)
            return;

        if (selectedTile == null)
            return;

        HandleKeyboardInput();
    }


    // =========================================================
    // START LEVEL
    // =========================================================

    public void StartLevel(int levelNumber)
    {
        // IMPORTANT:
        // Do NOT change the PuzzleBoard Transform here.
        // The board position is controlled by the Unity Editor.
        // This prevents the board from jumping to (0,0,0)
        // whenever Play mode starts.

        currentLevel = levelNumber;

        PuzzleProgress.SetCurrentLevel(currentLevel);

        puzzleComplete = false;

        selectedTile = null;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (finalWinPanel != null)
        {
            finalWinPanel.SetActive(false);
        }

        timeRemaining =
            levelDatabase != null ?
                levelDatabase.GetTimeLimitForLevel(currentLevel) :
                timeLimitSeconds;

        isTimeUp = false;
        isPaused = false;
        movesCount = 0;

        if (timeUpPanel != null)
        {
            timeUpPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        UpdateTimerText();
        UpdateMovesText();

        if (levelDatabase == null)
        {
            Debug.LogError(
                "PuzzleLevelDatabase is NOT assigned!"
            );

            return;
        }

        gridSize =
            levelDatabase.GetGridSizeForLevel(
                currentLevel
            );

        shuffleMoves =
            levelDatabase.GetShuffleMovesForLevel(
                currentLevel
            );

        Debug.Log(
            "LEVEL " +
            currentLevel +
            " | GRID " +
            gridSize +
            "x" +
            gridSize
        );

        ClearOldTiles();

        currentImage = GetRandomImage();

        if (currentImage == null)
        {
            Debug.LogError(
                "NO PUZZLE IMAGE ASSIGNED!"
            );

            return;
        }

        Debug.Log(
            "IMAGE SELECTED: " +
            currentImage.name
        );

        UpdateReferenceImage(currentImage);

        CreateTiles();

        ShufflePuzzle();

        Debug.Log("PUZZLE READY!");
    }


    // =========================================================
    // REFERENCE IMAGE
    // =========================================================

    private void UpdateReferenceImage(Texture2D image)
    {
        if (referenceImageUI == null)
        {
            Debug.LogWarning(
                "REFERENCE IMAGE UI NOT ASSIGNED — SKIPPING PREVIEW"
            );

            return;
        }

        float squareSize =
            Mathf.Min(image.width, image.height);

        float cropStartX =
            (image.width - squareSize) / 2f;

        float cropStartY =
            (image.height - squareSize) / 2f;

        Sprite referenceSprite =
            Sprite.Create(
                image,
                new Rect(
                    cropStartX,
                    cropStartY,
                    squareSize,
                    squareSize
                ),
                new Vector2(0.5f, 0.5f)
            );

        referenceImageUI.sprite =
            referenceSprite;

        referenceImageUI.preserveAspect = true;

        referenceImageUI.gameObject.SetActive(true);
    }


    // =========================================================
    // RANDOM IMAGE
    // =========================================================

    private Texture2D GetRandomImage()
    {
        if (
            puzzleImages == null ||
            puzzleImages.Length == 0
        )
        {
            return null;
        }

        if (
            remainingImages == null ||
            remainingImages.Count == 0
        )
        {
            remainingImages =
                new List<Texture2D>(puzzleImages);

            Debug.Log(
                "IMAGE POOL REFILLED: " +
                remainingImages.Count +
                " IMAGES AVAILABLE"
            );
        }

        int index =
            Random.Range(
                0,
                remainingImages.Count
            );

        Texture2D chosen =
            remainingImages[index];

        remainingImages.RemoveAt(index);

        return chosen;
    }


    // =========================================================
    // CREATE TILES
    // =========================================================

    private void CreateTiles()
    {
        int totalTiles =
            gridSize * gridSize;

        tiles =
            new PuzzleTile[totalTiles];

        float squareSize =
            Mathf.Min(
                currentImage.width,
                currentImage.height
            );

        float cropStartX =
            (currentImage.width - squareSize) / 2f;

        float cropStartY =
            (currentImage.height - squareSize) / 2f;

        float tilePixelSize =
            squareSize / gridSize;

        float pixelsPerUnit =
            tilePixelSize /
            tileSize;

        for (
            int i = 0;
            i < totalTiles;
            i++
        )
        {
            GameObject tileObject =
                new GameObject(
                    "Tile " + (i + 1)
                );

            tileObject.transform.SetParent(
                transform
            );

            tileObject.transform.localScale =
                Vector3.one;

            SpriteRenderer renderer =
                tileObject.AddComponent<SpriteRenderer>();

            BoxCollider2D collider =
                tileObject.AddComponent<BoxCollider2D>();

            PuzzleTile tile =
                tileObject.AddComponent<PuzzleTile>();

            int row =
                i / gridSize;

            int col =
                i % gridSize;

            Rect rect =
                new Rect(
                    cropStartX +
                    col * tilePixelSize,

                    cropStartY +
                    (gridSize - 1 - row) *
                    tilePixelSize,

                    tilePixelSize,

                    tilePixelSize
                );

            Sprite sprite =
                Sprite.Create(
                    currentImage,
                    rect,
                    new Vector2(0.5f, 0.5f),
                    pixelsPerUnit
                );

            renderer.sprite =
                sprite;

            collider.size =
                new Vector2(
                    tileSize,
                    tileSize
                );

            tile.Setup(
                this,
                i
            );

            tiles[i] =
                tile;
        }
    }


    // =========================================================
    // TILE POSITION
    // =========================================================

    public Vector3 GetWorldPosition(
        int position
    )
    {
        int row =
            position / gridSize;

        int col =
            position % gridSize;

        float spacing =
            tileSize +
            tileGap;

        float totalSize =
            (gridSize - 1) *
            spacing;

        float x =
            (col * spacing) -
            totalSize / 2f;

        float y =
            totalSize / 2f -
            (row * spacing);

        return new Vector3(
            x,
            y,
            0f
        );
    }


    // =========================================================
    // SHUFFLE
    // =========================================================

    private void ShufflePuzzle()
    {
        if (
            tiles == null ||
            tiles.Length < 2
        )
        {
            return;
        }

        for (
            int i = 0;
            i < shuffleMoves;
            i++
        )
        {
            int a =
                Random.Range(
                    0,
                    tiles.Length
                );

            int b =
                Random.Range(
                    0,
                    tiles.Length
                );

            if (a == b)
            {
                i--;
                continue;
            }

            SwapTiles(
                tiles[a],
                tiles[b]
            );
        }

        if (CheckSolvedWithoutShowingWin())
        {
            SwapTiles(
                tiles[0],
                tiles[1]
            );
        }

        Debug.Log(
            "PUZZLE SHUFFLED!"
        );
    }


    // =========================================================
    // TILE CLICK
    // =========================================================

    public void TileClicked(
        PuzzleTile tile
    )
    {
        if (puzzleComplete || isTimeUp || isPaused)
            return;

        selectedTile =
            tile;

        Debug.Log(
            "TILE SELECTED: " +
            tile.CurrentPosition
        );
    }


    // =========================================================
    // SWIPE MOVE
    // =========================================================

    public void SwipeMoveTile(
        PuzzleTile tile,
        Vector2Int direction
    )
    {
        if (puzzleComplete || isTimeUp || isPaused)
            return;

        if (tile == null)
            return;

        selectedTile =
            tile;

        MoveSelectedTile(
            direction
        );
    }


    // =========================================================
    // NEW INPUT SYSTEM
    // =========================================================

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        if (
            Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame
        )
        {
            MoveSelectedTile(
                Vector2Int.up
            );

            return;
        }

        if (
            Keyboard.current.sKey.wasPressedThisFrame ||
            Keyboard.current.downArrowKey.wasPressedThisFrame
        )
        {
            MoveSelectedTile(
                Vector2Int.down
            );

            return;
        }

        if (
            Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame
        )
        {
            MoveSelectedTile(
                Vector2Int.left
            );

            return;
        }

        if (
            Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame
        )
        {
            MoveSelectedTile(
                Vector2Int.right
            );

            return;
        }
    }


    // =========================================================
    // MOVE SELECTED TILE
    // =========================================================

    private void MoveSelectedTile(
        Vector2Int direction
    )
    {
        if (selectedTile == null)
            return;

        if (isPaused || isTimeUp || puzzleComplete)
            return;

        int current =
            selectedTile.CurrentPosition;

        int row =
            current / gridSize;

        int col =
            current % gridSize;

        int newRow =
            row - direction.y;

        int newCol =
            col + direction.x;

        if (
            newRow < 0 ||
            newRow >= gridSize ||
            newCol < 0 ||
            newCol >= gridSize
        )
        {
            Debug.Log(
                "CANNOT MOVE THAT WAY!"
            );

            return;
        }

        int targetPosition =
            newRow * gridSize +
            newCol;

        PuzzleTile targetTile =
            FindTileAtPosition(
                targetPosition
            );

        if (targetTile == null)
            return;

        SwapTiles(
            selectedTile,
            targetTile
        );

        IncrementMoves();

        Debug.Log(
            "TILES SWAPPED: " +
            current +
            " <-> " +
            targetPosition
        );

        CheckPuzzleComplete();
    }


    // =========================================================
    // FIND TILE
    // =========================================================

    private PuzzleTile FindTileAtPosition(
        int position
    )
    {
        if (tiles == null)
            return null;

        for (
            int i = 0;
            i < tiles.Length;
            i++
        )
        {
            if (
                tiles[i].CurrentPosition ==
                position
            )
            {
                return tiles[i];
            }
        }

        return null;
    }


    // =========================================================
    // SWAP
    // =========================================================

    private void SwapTiles(
        PuzzleTile a,
        PuzzleTile b
    )
    {
        int positionA =
            a.CurrentPosition;

        int positionB =
            b.CurrentPosition;

        a.SetPuzzlePosition(
            positionB
        );

        b.SetPuzzlePosition(
            positionA
        );
    }


    // =========================================================
    // CHECK COMPLETE
    // =========================================================

    private void CheckPuzzleComplete()
    {
        if (tiles == null)
            return;

        for (
            int i = 0;
            i < tiles.Length;
            i++
        )
        {
            if (
                tiles[i].CurrentPosition !=
                tiles[i].CorrectPosition
            )
            {
                return;
            }
        }

        puzzleComplete = true;

        Debug.Log(
            "PUZZLE COMPLETE!"
        );

        ShowWinPanel();
    }


    // =========================================================
    // CHECK SOLVED
    // =========================================================

    private bool CheckSolvedWithoutShowingWin()
    {
        if (tiles == null)
            return false;

        for (
            int i = 0;
            i < tiles.Length;
            i++
        )
        {
            if (
                tiles[i].CurrentPosition !=
                tiles[i].CorrectPosition
            )
            {
                return false;
            }
        }

        return true;
    }


    // =========================================================
    // DEBUG
    // =========================================================

    [ContextMenu("Debug: Log Mismatched Tiles")]
    private void DebugLogMismatchedTiles()
    {
        if (tiles == null)
        {
            Debug.Log("NO TILES TO CHECK.");
            return;
        }

        int mismatchCount = 0;

        for (
            int i = 0;
            i < tiles.Length;
            i++
        )
        {
            if (
                tiles[i].CurrentPosition !=
                tiles[i].CorrectPosition
            )
            {
                mismatchCount++;

                Debug.Log(
                    "MISMATCH — TILE '" +
                    tiles[i].name +
                    "' IS AT POSITION " +
                    tiles[i].CurrentPosition +
                    " BUT BELONGS AT " +
                    tiles[i].CorrectPosition
                );
            }
        }

        if (mismatchCount == 0)
        {
            Debug.Log(
                "ALL TILES CORRECT — PUZZLE SHOULD BE COMPLETE!"
            );
        }
        else
        {
            Debug.Log(
                mismatchCount +
                " TILE(S) STILL OUT OF PLACE."
            );
        }
    }


    // =========================================================
    // WIN PANEL
    // =========================================================

    private void ShowWinPanel()
    {
        PuzzleProgress.MarkLevelCompleted(currentLevel);

        bool isFinalLevel =
            levelDatabase != null &&
            currentLevel >= levelDatabase.MaxDefinedLevel;

        if (isFinalLevel)
        {
            if (finalWinPanel != null)
            {
                finalWinPanel.SetActive(true);
                finalWinPanel.transform.SetAsLastSibling();
            }
            else if (winPanel != null)
            {
                if (winText != null)
                {
                    winText.text =
                        "🏆 FINAL LEVEL COMPLETE!";
                }

                winPanel.SetActive(true);
                winPanel.transform.SetAsLastSibling();
            }

            Debug.Log(
                "FINAL LEVEL COMPLETE!"
            );

            return;
        }

        if (winPanel == null)
        {
            Debug.LogError(
                "WIN PANEL IS NOT ASSIGNED!"
            );

            return;
        }

        bool isTierComplete = false;

        string tierCompleteLabel = "";

        if (levelDatabase != null)
        {
            PuzzleLevelDatabase.DifficultyTier tier =
                levelDatabase.GetTierForLevel(
                    currentLevel
                );

            if (
                tier != null &&
                currentLevel == tier.endLevel
            )
            {
                isTierComplete = true;

                tierCompleteLabel =
                    tier.gridSize +
                    "x" +
                    tier.gridSize;
            }
        }

        if (winText != null)
        {
            winText.text =
                isTierComplete ?
                    tierCompleteLabel + " COMPLETE!" :
                    "PUZZLE COMPLETE!";
        }

        winPanel.SetActive(true);

        winPanel.transform.SetAsLastSibling();

        Debug.Log(
            isTierComplete ?
                tierCompleteLabel + " COMPLETE!" :
                "WIN PANEL SHOWN!"
        );
    }


    // =========================================================
    // TIMER
    // =========================================================

    private void TickTimer()
    {
        timeRemaining -=
            Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;

            UpdateTimerText();

            TriggerTimeUp();

            return;
        }

        UpdateTimerText();
    }


    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int totalSeconds =
            Mathf.CeilToInt(timeRemaining);

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        timerText.text =
            minutes +
            ":" +
            seconds.ToString("00");
    }


    private void TriggerTimeUp()
    {
        if (isTimeUp)
            return;

        isTimeUp = true;

        selectedTile = null;

        Debug.Log(
            "TIME'S UP!"
        );

        if (timeUpPanel != null)
        {
            timeUpPanel.SetActive(true);

            timeUpPanel.transform.SetAsLastSibling();
        }
    }


    // =========================================================
    // MOVES
    // =========================================================

    private void IncrementMoves()
    {
        movesCount++;

        UpdateMovesText();
    }


    private void UpdateMovesText()
    {
        if (movesText == null)
            return;

        movesText.text =
            movesCount.ToString();
    }


    // =========================================================
    // SETTINGS PANEL
    // =========================================================

    public void OpenSettings()
    {
        if (puzzleComplete || isTimeUp)
            return;

        isPaused = true;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            settingsPanel.transform.SetAsLastSibling();
        }
    }


    public void CloseSettings()
    {
        isPaused = false;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }


    // =========================================================
    // NAVIGATION
    // =========================================================

    public void GoToHome()
    {
        Debug.Log(
            "GOING BACK TO HOME"
        );

        SceneManager.LoadScene("Home");
    }


    public void ExitGame()
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


    // =========================================================
    // FINAL WIN PANEL ACTIONS
    // =========================================================

    public void ResetProgressAndGoHome()
    {
        PuzzleProgress.ResetAllProgress();

        GoToHome();
    }


    public void QuitAndResetProgress()
    {
        PuzzleProgress.ResetAllProgress();

        ExitGame();
    }


    // =========================================================
    // RESTART
    // =========================================================

    public void RestartPuzzle()
    {
        StartLevel(
            currentLevel
        );
    }


    // =========================================================
    // NEXT LEVEL
    // =========================================================

    public void NextLevel()
    {
        int nextLevelNumber =
            currentLevel + 1;

        if (
            levelDatabase != null &&
            nextLevelNumber > levelDatabase.MaxDefinedLevel
        )
        {
            Debug.Log(
                "ALL LEVELS COMPLETE! (" +
                levelDatabase.MaxDefinedLevel +
                " TOTAL)"
            );

            return;
        }

        StartLevel(
            nextLevelNumber
        );
    }


    // =========================================================
    // CLEAR OLD TILES
    // =========================================================

    private void ClearOldTiles()
    {
        PuzzleTile[] oldTiles =
            GetComponentsInChildren<PuzzleTile>();

        for (
            int i = oldTiles.Length - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                oldTiles[i].gameObject
            );
        }

        tiles = null;
    }
}