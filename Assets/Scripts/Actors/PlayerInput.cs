using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInput : Actor
{
    [Header("References")]
    [SerializeField] private Board board = null;
    [SerializeField] private PieceContainer pieces = null;

    public PieceContainer PieceContainer => pieces;

    private InputMaster input = null;
    private DragAndDropController dragAndDropController = null;
    
    private bool isCurrentTurn = false;
    private bool isCurrentSpawn = false;

    private void Awake()
    {
        input = new InputMaster();
        input.Player.ClickDown.performed += _ => dragAndDropController.OnClickDown();
        input.Player.ClickUp.performed += _ => dragAndDropController.OnClickUp();
        input.Player.ClickUp.performed += _ => OnClickUp();
        dragAndDropController = GetComponent<DragAndDropController>();
    }

    private void OnEnable()
    {
        input.Enable();
    }

    public override void PerformSpawn()
    {
        isCurrentSpawn = true;
    }

    public override void PerformMove()
    {
        isCurrentTurn = true;
    }

    private void OnClickUp()
    {
        DragAndDropListener heldObject = dragAndDropController.HeldObject;

        if (heldObject == null)
            return;
        
        Piece piece = heldObject.GetComponent<Piece>();

        if (!piece.Properties.IsPlayerPiece)
            return;

        heldObject.ReturnToOriginalPosition();

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector3 piecePos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, piece.transform.position.z);
        BoardPosition targetPosition = board.GetWorldToCell(piecePos);
        MoveInfo move = new MoveInfo(piece, targetPosition);
        
        if (isCurrentSpawn)
        {
            isCurrentSpawn = false;
            board.SpawnPiece(move);
        }
        else if (isCurrentTurn)
        {
            isCurrentTurn = false;
            board.TryMove(move);
        }
    }

    private void OnDisable()
    {
        input.Disable();
    }
}
