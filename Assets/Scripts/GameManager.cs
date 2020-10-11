using System;
using System.Collections.Generic;
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
    public Dictionary<PieceInfo, Piece> pieceInfoMap = new Dictionary<PieceInfo, Piece>();

    private Grid grid = null;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Start()
    {
        // PieceContainer dependencies
        List<PieceInfo> piecesA = new List<PieceInfo>(PieceContainer.MAX_CAPACITY);
        List<PieceInfo> piecesB = new List<PieceInfo>(PieceContainer.MAX_CAPACITY);

        // Get the piece gameobjects
        foreach (Piece piece in FindObjectsOfType<Piece>())
        {
            pieceInfoMap.Add(piece.Info, piece);

            if (piece.Info.Side == Side.A)
            {
                piecesA.Add(piece.Info);
            }
            else if (piece.Info.Side == Side.B)
            {
                piecesB.Add(piece.Info);
            }
            else
            {
                Debug.LogError("Piece has no side!");
                return;
            }
        }

        // Board dependencies
        PieceInfo[,] pieceGrid = new PieceInfo[Board.WIDTH, Board.HEIGHT];
        PieceContainer pieceContainerA = new PieceContainer(piecesA);
        PieceContainer pieceContainerB = new PieceContainer(piecesB);
        GamePhase startingGamePhase = GamePhase.Spawn;
        GameOutput startingGameOutput = GameOutput.None;
        Side startingSide = Side.A;

        // Initialize board
        Board = new Board(
            pieceGrid,
            pieceContainerA,
            pieceContainerB,
            null,
            startingGamePhase,
            startingGameOutput,
            startingSide);

        // Initialize actors
        actorA.Initialize(this, Board, pieceContainerA, pieceContainerB);
        actorB.Initialize(this, Board, pieceContainerB, pieceContainerA);

        // TODO: Configure which can spawn first
        actorA.PerformSpawn();
    }

    public void SpawnPiece(in MoveInfo move)
    {
        if (Board.SpawnPiece(move))
        {
            UpdatePieceWorldPosition(move.PieceInfo);

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
                // Actor A first
                actorA.PerformMove();

                GamePhaseChanged.Invoke(GamePhase.Move);
            }
        }
        else
        {
            Debug.Log("Could not confirm spawn!");
        }
    }

    public void MovePiece(in MoveInfo move)
    {
        if (Board.MovePiece(move))
        {
            UpdatePieceWorldPosition(move.PieceInfo);

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

    private void UpdatePieceWorldPosition(PieceInfo pieceInfo)
    {
        if (!pieceInfo.IsAlive)
            return;

        pieceInfoMap[pieceInfo].transform.position = GetCellToWorld(pieceInfo.BoardPosition);
    }

    #region Helpers
    public BoardPosition GetWorldToCell(in Vector3 worldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        return new BoardPosition(cellPos.x, cellPos.y);
    }

    private Vector3 GetCellToWorld(in BoardPosition pos)
    {
        return grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
    }
    #endregion
}
