using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PuzzleTile : MonoBehaviour
{
    [Header("SWIPE")]
    [SerializeField] private float minSwipeDistance = 20f;

    private PuzzleBoard board;

    private SpriteRenderer spriteRenderer;

    private int correctPosition;

    private int currentPosition;

    private Vector2 dragStartScreenPos;

    private bool isDragging;


    public int CorrectPosition
    {
        get
        {
            return correctPosition;
        }
    }


    public int CurrentPosition
    {
        get
        {
            return currentPosition;
        }
    }


    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();
    }


    public void Setup(
        PuzzleBoard puzzleBoard,
        int correctIndex
    )
    {
        board =
            puzzleBoard;

        correctPosition =
            correctIndex;

        currentPosition =
            correctIndex;

        SetPuzzlePosition(
            correctIndex
        );
    }


    public void SetPuzzlePosition(
        int position
    )
    {
        currentPosition =
            position;

        if (board == null)
            return;

        transform.localPosition =
            board.GetWorldPosition(
                position
            );
    }


    // =========================================================
    // SWIPE INPUT
    // =========================================================

    private void OnMouseDown()
    {
        if (board == null)
            return;

        isDragging = true;

        dragStartScreenPos =
            GetPointerScreenPosition();

        board.TileClicked(this);
    }


    private void OnMouseUp()
    {
        if (board == null)
            return;

        if (!isDragging)
            return;

        isDragging = false;


        Vector2 dragEndScreenPos =
            GetPointerScreenPosition();

        Vector2 delta =
            dragEndScreenPos - dragStartScreenPos;


        // Too small to count as a swipe — treat as a plain tap.
        if (delta.magnitude < minSwipeDistance)
            return;


        Vector2Int direction;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            direction =
                delta.x > 0f ?
                    Vector2Int.right :
                    Vector2Int.left;
        }
        else
        {
            direction =
                delta.y > 0f ?
                    Vector2Int.up :
                    Vector2Int.down;
        }


        Debug.Log(
            "TILE SWIPED: " +
            currentPosition +
            " DIRECTION: " +
            direction
        );

        board.SwipeMoveTile(this, direction);
    }


    private Vector2 GetPointerScreenPosition()
    {
        // Real phones have a Touchscreen device, not a Mouse —
        // check touch first so this actually works on Android
        // and iOS, not just the Editor's simulated mouse input.
        if (Touchscreen.current != null)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        return Vector2.zero;
    }
}