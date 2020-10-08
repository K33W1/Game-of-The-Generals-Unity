using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public const int Width = 9;
    public const int Height = 8;

    public readonly Piece[,] PieceGrid = null;
    public readonly PieceContainer PiecesA = null;
    public readonly PieceContainer PiecesB = null;

    public GamePhase CurrentGamePhase { get; private set; } = GamePhase.Spawn;
    public GameOutput CurrentGameOutput { get; private set; } = GameOutput.None;
    public Side CurrentSide { get; private set; } = Side.Invalid;

    public Board(Piece[,] pieceGrid, PieceContainer piecesA, PieceContainer piecesB, GamePhase currentGamePhase, GameOutput currentGameOutput, Side currentSide)
    {
        PieceGrid = pieceGrid;
        PiecesA = piecesA;
        PiecesB = piecesB;
        CurrentGamePhase = currentGamePhase;
        CurrentGameOutput = currentGameOutput;
        CurrentSide = currentSide;
    }

    public bool SpawnPiece(MoveInfo move)
    {
        BoardPosition pos = move.TargetPosition;

        if (CurrentSide == Side.A)
        {
            if (pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y <= 2)
            {
                PlacePiece(move);
                PiecesA.ActivatePiece(move.Piece);
                return true;
            }

        }
        else
        {
            if (pos.x >= 0 && pos.x < Width && pos.y >= 5 && pos.y <= 7)
            {
                PlacePiece(move);
                PiecesB.ActivatePiece(move.Piece);
                return true;
            }
        }

        return false;
    }

    public bool ConfirmSpawn()
    {
        if (CurrentSide == Side.A)
        {
            if (PiecesA.IsValidSpawn())
            {
                CurrentSide = Side.B;
                return true;
            }
            else
            {
                Debug.LogError("Invalid spawn state!");
            }
        }
        else if (CurrentSide == Side.B)
        {
            if (PiecesB.IsValidSpawn())
            {
                CurrentSide = Side.A;
                CurrentGamePhase = GamePhase.Move;
                return true;
            }
            else
            {
                Debug.LogError("Invalid spawn state!");
            }
        }
        else
        {
            Debug.LogError("Tried to confirm side when no side is assigned!");
        }

        return false;
    }

    public bool MovePiece(MoveInfo move)
    {
        if (TryMovePiece(move))
        {
            // Check if someone won
            GameOutput gameOutput = CheckGameEnd();
            if (gameOutput != GameOutput.None)
            {
                CurrentGamePhase = GamePhase.End;
                CurrentGameOutput = gameOutput;
                return true;
            }

            // Flip side
            if (CurrentSide == Side.A)
            {
                CurrentSide = Side.B;
                return true;
            }
            else
            {
                CurrentSide = Side.A;
                return true;
            }
        }

        return false;
    }

    public GameOutput CheckGameEnd()
    {
        Piece flagA = PiecesA.GetPiece(PieceRank.Flag);
        Piece flagB = PiecesB.GetPiece(PieceRank.Flag);

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

    private bool TryMovePiece(MoveInfo move)
    {
        if (!IsValidMove(move))
            return false;

        Piece thisPiece = move.Piece;
        BoardPosition nextPos = move.TargetPosition;
        Piece otherPiece = PieceGrid[nextPos.x, nextPos.y];

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

    private void PlacePiece(MoveInfo move)
    {
        UpdateGridArray(move);
        UpdatePiecePosition(move);
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
        PieceGrid[pos.x, pos.y] = null;

        if (piece.Properties.Side == Side.A)
            PiecesA.KillPiece(piece);
        else
            PiecesB.KillPiece(piece);
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
            PieceGrid[lastPos.x, lastPos.y] = null;
        PieceGrid[nextPos.x, nextPos.y] = move.Piece;
    }

    public List<MoveInfo> GetAllValidMoves(Side side)
    {
        List<MoveInfo> allPossibleMoves = new List<MoveInfo>();
        PieceContainer pieces = side == Side.A ? PiecesA : PiecesB;

        foreach (Piece piece in pieces.ActivePieces)
            foreach (MoveInfo move in GetPieceValidMoves(piece))
                allPossibleMoves.Add(move);

        return allPossibleMoves;
    }

    public PieceContainer GetPieceContainer(Side side)
    {
        return side == Side.A ? PiecesA : PiecesB;
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

    public bool IsValidMove(MoveInfo move)
    {
        Piece piece = move.Piece;
        BoardPosition targetPos = move.TargetPosition;

        if (!IsPositionInsideGrid(targetPos))
            return false;

        if (!piece.BoardPosition.IsPositionAdjacent(targetPos))
            return false;

        Piece otherPiece = PieceGrid[targetPos.x, targetPos.y];

        if (otherPiece == null)
            return true;

        if (otherPiece.Properties.Side != piece.Properties.Side)
            return true;

        return false;
    }

    public bool IsPositionInsideGrid(BoardPosition pos)
    {
        return !(pos.x < 0 || pos.y < 0 || pos.x >= Width || pos.y >= Height);
    }
}
