using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Grid))]
public class Board : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Actor actorA = null;
    [SerializeField] private Actor actorB = null;

    public const int Width = 9;
    public const int Height = 8;

    public GamePhaseObservable CurrentGamePhase { get; } = new GamePhaseObservable();
    public GameOutput CurrentGameOutput { get; private set; } = GameOutput.None;
    public Side CurrentSide { get; private set; } = Side.Invalid;

    private Grid grid = null;
    private Piece[,] pieceGrid = null;
    private PieceContainer piecesA = null;
    private PieceContainer piecesB = null;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        pieceGrid = new Piece[Width, Height];
        piecesA = new PieceContainer();
        piecesB = new PieceContainer();
    }

    private void Start()
    {
        // Starting values
        CurrentGamePhase.Value = GamePhase.Spawn;
        CurrentGameOutput = GameOutput.None;
        CurrentSide = Side.A;

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

        actorA.PerformSpawn();
    }

    public void SpawnPiece(MoveInfo move)
    {
        if (CurrentGamePhase.Value != GamePhase.Spawn)
        {
            Debug.LogError("Tried to spawn piece on wrong game phase!");
            return;
        }

        BoardPosition pos = move.TargetPosition;

        if (CurrentSide == Side.A)
        {
            if (pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y <= 2)
            {
                PlacePiece(move);
                piecesA.ActivatePiece(move.Piece);
            }

            actorA.PerformSpawn();
        }
        else
        {
            if (pos.x >= 0 && pos.x < Width && pos.y >= 5 && pos.y <= 7)
            {
                PlacePiece(move);
                piecesB.ActivatePiece(move.Piece);
            }

            actorB.PerformSpawn();
        }
    }

    public void ConfirmSpawn()
    {
        if (CurrentGamePhase.Value != GamePhase.Spawn)
        {
            Debug.LogError("Tried to confirm spawn on wrong game phase!");
            return;
        }

        if (CurrentSide == Side.A)
        {
            if (piecesA.IsValidSpawn())
            {
                CurrentSide = Side.B;
                actorB.PerformSpawn();
            }
            else
            {
                Debug.LogError("Invalid spawn state!");
                actorA.PerformSpawn();
            }
        }
        else
        {
            if (piecesB.IsValidSpawn())
            {
                CurrentSide = Side.A;
                CurrentGamePhase.Value = GamePhase.Move;
                actorA.PerformMove();
            }
            else
            {
                Debug.LogError("Invalid spawn state!");
                actorB.PerformSpawn();
            }
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
            if (CurrentSide == Side.A)
            {
                CurrentSide = Side.B;
                actorB.PerformMove();
            }
            else
            {
                CurrentSide = Side.A;
                actorA.PerformMove();
            }
        }
        else
        {
            // Try again
            if (CurrentSide == Side.A)
                actorA.PerformMove();
            else
                actorB.PerformMove();
        }
    }

    public GameOutput CheckGameEnd()
    {
        Piece flagA = piecesA.GetPiece(PieceRank.Flag);
        Piece flagB = piecesB.GetPiece(PieceRank.Flag);

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
            if (CurrentSide == Side.B && flagA.BoardPosition.y == Height - 1)
                return GameOutput.A;
            else if (CurrentSide == Side.A && flagB.BoardPosition.y == 0)
                return GameOutput.B;
        }

        return GameOutput.None;
    }

    private void EndGame(GameOutput gameOutput)
    {
        CurrentGamePhase.Value = GamePhase.End;
        CurrentGameOutput = gameOutput;
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

        if (piece.Properties.Side == Side.A)
            piecesA.KillPiece(piece);
        else
            piecesB.KillPiece(piece);
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
    public PieceContainer GetPieceContainer(Side side)
    {
        return side == Side.A ? piecesA : piecesB;
    }
    
    public List<MoveInfo> GetAllValidMoves(Actor actor)
    {
        List<MoveInfo> allPossibleMoves = new List<MoveInfo>();
        PieceContainer pieces = actor == actorA ? piecesA : piecesB;

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

        if (otherPiece.Properties.Side != piece.Properties.Side)
            return true;

        return false;
    }
    #endregion
}
