using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInput : Actor
{
    private InputMaster input = null;
    private DragAndDropController dragAndDropController = null;
    
    private bool isCurrentTurn = false;
    private bool isCurrentSpawn = false;

    private void Awake()
    {
        input = new InputMaster();
        input.Player.ClickDown.performed += _ => dragAndDropController.OnClickDown();
        input.Player.ClickUp.performed += _ => OnClickUp();
        input.Player.ClickUp.performed += _ => dragAndDropController.OnClickUp();
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

    public void ConfirmSpawns()
    {
        isCurrentSpawn = false;
        gameManager.ConfirmSpawn();
    }

    private void OnClickUp()
    {
        DragAndDropListener heldObject = dragAndDropController.HeldObject;

        if (heldObject == null)
            return;
        
        heldObject.ReturnToOriginalPosition();

        Piece piece = heldObject.GetComponent<Piece>();

        if (piece.Info.Side != Side.A)
            return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector3 piecePos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, piece.transform.position.z);
        BoardPosition targetPosition = gameManager.GetWorldToCell(piecePos);
        MoveInfo move = new MoveInfo(piece.Info, targetPosition);
        
        if (isCurrentSpawn)
        {
            isCurrentSpawn = false;
            gameManager.SpawnPiece(move);
        }
        else if (isCurrentTurn)
        {
            isCurrentTurn = false;
            gameManager.MovePiece(move);
        }
    }

    private void OnDisable()
    {
        input.Disable();
    }
}
