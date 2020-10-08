using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Grid))]
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Actor actorA = null;
    [SerializeField] private Actor actorB = null;

    public event Action<GamePhase> GamePhaseChanged;

    public Board Board { get; private set; } = null;

    private Grid grid = null;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Start()
    {
        // Board dependencies
        Piece[,] pieceGrid = new Piece[Board.Width, Board.Height];
        PieceContainer piecesA = new PieceContainer();
        PieceContainer piecesB = new PieceContainer();
        GamePhase startingGamePhase = GamePhase.Spawn;
        GameOutput startingGameOutput = GameOutput.None;
        Side startingSide = Side.A;

        // Get the piece gameobjects
        foreach (Piece piece in FindObjectsOfType<Piece>())
        {
            if (piece.Properties.Side == Side.A)
            {
                piecesA.InactivePieces.Add(piece);
            }
            else if (piece.Properties.Side == Side.B)
            {
                piecesB.InactivePieces.Add(piece);
            }
            else
            {
                Debug.LogError("Piece has no side!");
                return;
            }
        }

        // Initialize board
        Board = new Board(pieceGrid, piecesA, piecesB, startingGamePhase, startingGameOutput, startingSide);

        // TODO: Configure which can spawn first
        actorA.PerformSpawn();
    }

    public void SpawnPiece(MoveInfo move)
    {
        if (Board.SpawnPiece(move))
        {
            UpdatePieceWorldPosition(move);

            if (Board.CurrentSide == Side.A)
                actorA.PerformSpawn();
            else
                actorB.PerformSpawn();
        }
        else
        {
            Debug.LogError("Error spawning piece!");
        }
    }

    public void ConfirmSpawn()
    {
        if (Board.ConfirmSpawn())
        {
            if (Board.CurrentSide == Side.B)
            {
                actorB.PerformSpawn();
            }
            else
            {
                GamePhaseChanged.Invoke(GamePhase.Move);
                actorA.PerformMove();
            }
        }
        else
        {
            Debug.Log("Could not confirm spawn!");
        }
    }

    public void MovePiece(MoveInfo move)
    {
        if (Board.MovePiece(move))
        {
            UpdatePieceWorldPosition(move);

            if (Board.CurrentGameOutput != GameOutput.None)
                EndGame();
            else if (Board.CurrentSide == Side.A)
                actorA.PerformMove();
            else if (Board.CurrentSide == Side.B)
                actorB.PerformMove();
            else
                Debug.LogError("Board's current side is invalid!");
        }
        else
        {
            // Try again
            if (Board.CurrentSide == Side.A)
                actorA.PerformMove();
            else if (Board.CurrentSide == Side.B)
                actorB.PerformMove();
            else
                Debug.LogError("Board's current side is invalid!");
        }
    }

    private void EndGame()
    {
        GamePhaseChanged.Invoke(GamePhase.End);
    }

    private void UpdatePieceWorldPosition(MoveInfo move)
    {
        if (!move.Piece.gameObject.activeSelf)
            return;

        move.Piece.transform.position = GetCellToWorld(move.TargetPosition);
    }

    #region Helpers
    public BoardPosition GetWorldToCell(Vector3 worldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        return new BoardPosition(cellPos.x, cellPos.y);
    }

    private Vector3 GetCellToWorld(BoardPosition pos)
    {
        return grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
    }
    #endregion
}
