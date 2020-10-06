using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Grid))]
public class Board : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Actor actorA = null;
    [SerializeField] private Actor actorB = null;
    [SerializeField] private PieceContainer actorAPieces = null;
    [SerializeField] private PieceContainer actorBPieces = null;

    [Header("Scriptable Objects")]
    [SerializeField] private GamePhaseObject currentGamePhase = null;
    [SerializeField] private GameOutputObject currentGameOutput = null;
    [SerializeField] private StringValue winnerName = null;

    [Header("Settings")]
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 8;

    public int Width => width;
    public int Height => height;

    private Grid grid = null;
    private Piece[,] pieceGrid = null;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        pieceGrid = new Piece[width, height];
    }

    private void Start()
    {
        actorA.PerformSpawn();
    }

    public void SpawnPiece(MoveInfo move)
    {
        BoardPosition pos = move.TargetPosition;

        if (currentGamePhase.Value == GamePhase.SpawnA)
        {
            if (pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y <= 2)
            {
                PlacePiece(move);
                actorAPieces.ActivatePiece(move.Piece);
            }

            actorA.PerformSpawn();
        }
        else if (currentGamePhase.Value == GamePhase.SpawnB)
        {
            if (pos.x >= 0 && pos.x < Width && pos.y >= 5 && pos.y <= 7)
            {
                PlacePiece(move);
                actorBPieces.ActivatePiece(move.Piece);
            }

            actorB.PerformSpawn();
        }
        else
        {
            Debug.LogError("Tried to spawn a piece in wrong game phase!");
        }
    }

    public void ConfirmSpawn()
    {
        if (currentGamePhase.Value == GamePhase.SpawnA)
        {
            if (actorAPieces.IsValidSpawn())
            {
                currentGamePhase.Value = GamePhase.SpawnB;
                actorB.PerformSpawn();
            }
            else
            {
                Debug.LogError("Invalid spawn state!");
                actorA.PerformSpawn();
            }
        }
        else if (currentGamePhase.Value == GamePhase.SpawnB)
        {
            if (actorBPieces.IsValidSpawn())
            {
                currentGamePhase.Value = GamePhase.MoveA;
                actorA.PerformMove();
            }
            else
            {
                Debug.LogError("Invalid spawn state!");
                actorB.PerformSpawn();
            }
        }
        else
        {
            Debug.LogError("Tried to confirm spawn on wrong game phase!");
        }
    }

    public void PlacePiece(MoveInfo move)
    {
        UpdateGridArray(move);
        UpdatePieceWorldPosition(move);
        UpdatePiecePosition(move);
    }

    public void MovePiece(MoveInfo move)
    {
        if (TryMovePiece(move))
        {
            // Check if someone won
            GameOutput gameOutput = CheckGameEnd();
            if (gameOutput != GameOutput.None)
            {
                EndGame(gameOutput);
                return;
            }

            // Flip side
            if (currentGamePhase.Value == GamePhase.MoveA)
            {
                currentGamePhase.Value = GamePhase.MoveB;
                actorB.PerformMove();
            }
            else
            {
                currentGamePhase.Value = GamePhase.MoveA;
                actorA.PerformMove();
            }
        }
        else
        {
            // Try again
            if (currentGamePhase.Value == GamePhase.MoveA)
            {
                actorA.PerformMove();
            }
            else
            {
                actorB.PerformMove();
            }
        }
    }

    private void EndGame(GameOutput gameOutput)
    {
        if (gameOutput == GameOutput.A)
            winnerName.Value = "Player";
        else
            winnerName.Value = "AI";

        currentGameOutput.Value = GameOutput.None;
        currentGamePhase.Value = GamePhase.End;
    }

    private GameOutput CheckGameEnd()
    {
        Piece flagA = actorAPieces.GetPiece(PieceRank.Flag);
        Piece flagB = actorBPieces.GetPiece(PieceRank.Flag);

        if (flagA == null)
        {
            return GameOutput.B;
        }
        else if (flagB == null)
        {
            return GameOutput.A;
        }
        else
        {
            if (currentGamePhase.Value == GamePhase.MoveB && flagA.BoardPosition.y == Height - 1)
                return GameOutput.A;
            else if (currentGamePhase.Value == GamePhase.MoveA && flagB.BoardPosition.y == 0)
                return GameOutput.B;
        }

        return GameOutput.None;
    }

    private bool TryMovePiece(MoveInfo move)
    {
        if (!IsValidMove(move))
            return false;

        Piece thisPiece = move.Piece;
        BoardPosition nextPos = move.TargetPosition;
        Piece otherPiece = pieceGrid[nextPos.x, nextPos.y];

        // If other piece exists on target position
        if (otherPiece != null)
        {
            if (otherPiece.Properties.IsPlayerPiece == thisPiece.Properties.IsPlayerPiece)
                return false;

            AttackPiece(thisPiece, otherPiece);

            // Check if piece is still alive
            if (thisPiece.gameObject.activeSelf)
                PlacePiece(move);
        }
        else
        {
            PlacePiece(move);
        }

        return true;
    }

    private void AttackPiece(Piece pieceA, Piece pieceB)
    {
        Piece winningPiece = GameRules.GetWinningPiece(pieceA, pieceB);
        // TODO: Battle animation

        // Remove losing piece
        if (winningPiece != null)
        {
            Piece losingPiece = winningPiece == pieceA ? pieceB : pieceA;
            KillPiece(losingPiece);
        }
        else
        {
            KillPiece(pieceA);
            KillPiece(pieceB);
        }
    }

    private void KillPiece(Piece piece)
    {
        BoardPosition pos = piece.BoardPosition;
        pieceGrid[pos.x, pos.y] = null;

        if (piece.Properties.IsPlayerPiece)
            actorAPieces.KillPiece(piece);
        else
            actorBPieces.KillPiece(piece);
    }

    private void UpdatePieceWorldPosition(MoveInfo move)
    {
        if (!move.Piece.gameObject.activeInHierarchy)
            return;

        move.Piece.transform.position = GetCellToWorld(move.TargetPosition);
    }

    private void UpdatePiecePosition(MoveInfo move)
    {
        move.Piece.BoardPosition = move.TargetPosition;
    }

    private void UpdateGridArray(MoveInfo move)
    {
        BoardPosition lastPos = move.Piece.BoardPosition;
        BoardPosition nextPos = move.TargetPosition;

        if (IsPositionInsideGrid(lastPos))
            pieceGrid[lastPos.x, lastPos.y] = null;
        pieceGrid[nextPos.x, nextPos.y] = move.Piece;
    }

    #region Helpers
    public List<MoveInfo> GetAllValidMoves(Actor actor)
    {
        List<MoveInfo> allPossibleMoves = new List<MoveInfo>();
        PieceContainer pieces = actor == actorA ? actorAPieces : actorBPieces;

        foreach (Piece piece in pieces.ActivePieces)
            foreach (MoveInfo move in GetPieceValidMoves(piece))
                allPossibleMoves.Add(move);

        return allPossibleMoves;
    }

    public Vector3 GetCellToWorld(BoardPosition pos)
    {
        return grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
    }

    public BoardPosition GetWorldToCell(Vector3 worldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        return new BoardPosition(cellPos.x, cellPos.y);
    }

    public List<MoveInfo> GetPieceValidMoves(Piece piece)
    {
        List<MoveInfo> moves = new List<MoveInfo>();

        BoardPosition origin = piece.BoardPosition;
        MoveInfo moveUp = new MoveInfo(piece, origin.Up);
        MoveInfo moveDown = new MoveInfo(piece, origin.Down);
        MoveInfo moveLeft = new MoveInfo(piece, origin.Left);
        MoveInfo moveRight = new MoveInfo(piece, origin.Right);

        if (IsValidMove(moveUp)) moves.Add(moveUp);
        if (IsValidMove(moveDown)) moves.Add(moveDown);
        if (IsValidMove(moveLeft)) moves.Add(moveLeft);
        if (IsValidMove(moveRight)) moves.Add(moveRight);

        return moves;
    }

    public bool IsPositionInsideGrid(BoardPosition pos)
    {
        return !(pos.x < 0 || pos.y < 0 || pos.x >= Width || pos.y >= Height);
    }

    public bool IsValidMove(MoveInfo move)
    {
        Piece piece = move.Piece;
        BoardPosition targetPos = move.TargetPosition;

        if (!IsPositionInsideGrid(targetPos))
            return false;

        if (!piece.BoardPosition.IsPositionAdjacent(targetPos))
            return false;

        Piece otherPiece = pieceGrid[targetPos.x, targetPos.y];

        if (otherPiece == null)
            return true;

        if (otherPiece.Properties.IsPlayerPiece != piece.Properties.IsPlayerPiece)
            return true;

        return false;
    }
    #endregion
}
