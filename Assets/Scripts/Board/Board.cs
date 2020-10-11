using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public const int WIDTH = 9;
    public const int HEIGHT = 8;

    public readonly PieceInfo[,] PieceGrid = null;
    public readonly PieceContainer PiecesA = null;
    public readonly PieceContainer PiecesB = null;

    public BoardChange? BoardChange { get; private set; }

    public GamePhase CurrentGamePhase { get; private set; } = GamePhase.Spawn;
    public GameOutput CurrentGameOutput { get; private set; } = GameOutput.None;
    public Side CurrentSide { get; private set; } = Side.None;

    public Board(PieceInfo[,] pieceGrid,
                 PieceContainer piecesA,
                 PieceContainer piecesB,
                 BoardChange? boardChange,
                 GamePhase currentGamePhase,
                 GameOutput currentGameOutput,
                 Side currentSide)
    {
        PieceGrid = pieceGrid;
        PiecesA = piecesA;
        PiecesB = piecesB;
        BoardChange = boardChange;
        CurrentGamePhase = currentGamePhase;
        CurrentGameOutput = currentGameOutput;
        CurrentSide = currentSide;
    }

    public Board GetCopyWithHiddenPieces(Side side)
    {
        // Board dependencies
        PieceInfo[,] pieceGrid = new PieceInfo[WIDTH, HEIGHT];
        PieceContainer piecesA = side == Side.A ? PiecesA.Copy() : PiecesA.CopyWithHiddenPieces();
        PieceContainer piecesB = side == Side.A ? PiecesB.CopyWithHiddenPieces() : PiecesB.Copy();

        // Place alive pieces back into grid
        foreach (PieceInfo pieceInfo in piecesA.ActivePieces)
        {
            BoardPosition pos = pieceInfo.BoardPosition;
            pieceGrid[pos.x, pos.y] = pieceInfo;
        }
        foreach (PieceInfo pieceInfo in piecesB.ActivePieces)
        {
            BoardPosition pos = pieceInfo.BoardPosition;
            pieceGrid[pos.x, pos.y] = pieceInfo;
        }

        return new Board(
            pieceGrid,
            piecesA,
            piecesB,
            BoardChange,
            CurrentGamePhase,
            CurrentGameOutput,
            CurrentSide);
    }

    public List<MoveInfo> GetAllValidMoves()
    {
        List<MoveInfo> allPossibleMoves = new List<MoveInfo>(PieceContainer.MAX_CAPACITY * 4);
        PieceContainer pieces = CurrentSide == Side.A ? PiecesA : PiecesB;

        foreach (PieceInfo piece in pieces.ActivePieces)
            foreach (MoveInfo move in GetPieceValidMoves(piece))
                allPossibleMoves.Add(move);

        return allPossibleMoves;
    }

    public PieceContainer GetPieceContainer(Side side)
    {
        return side == Side.A ? PiecesA : PiecesB;
    }

    public bool SpawnPiece(in MoveInfo move)
    {
        BoardPosition pos = move.NewPosition;

        if (CurrentSide == Side.A)
        {
            if (pos.x >= 0 && pos.x < WIDTH && pos.y >= 0 && pos.y <= 2)
            {
                PlacePiece(move);
                PiecesA.ActivatePiece(move.PieceInfo);
                return true;
            }

        }
        else
        {
            if (pos.x >= 0 && pos.x < WIDTH && pos.y >= 5 && pos.y <= 7)
            {
                PlacePiece(move);
                PiecesB.ActivatePiece(move.PieceInfo);
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

    public bool MovePiece(in MoveInfo move)
    {
        BoardChange? boardChange = TryMovePiece(move);

        if (boardChange.HasValue)
        {
            BoardChange = boardChange.Value;

            // Check if someone won
            GameOutput gameOutput = CheckGameEnd();
            if (gameOutput != GameOutput.None)
            {
                CurrentSide = Side.None;
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

    private BoardChange? TryMovePiece(in MoveInfo move)
    {
        if (!IsValidMove(move))
        {
            Debug.Log("Invalid move!");
            return null;
        }

        BoardPosition newPos = move.NewPosition;
        PieceInfo thisPieceInfo = move.PieceInfo;
        PieceInfo otherPieceInfo = PieceGrid[newPos.x, newPos.y];
        Side winningSide = Side.None;

        // If other piece exists on target position
        if (otherPieceInfo != null)
        {
            winningSide = AttackPiece(thisPieceInfo, otherPieceInfo);

            // Check if attacking piece won the battle
            if (winningSide == Side.A)
                PlacePiece(move);
        }
        else
        {
            PlacePiece(move);
        }

        return new BoardChange(move, otherPieceInfo, winningSide);
    }

    private void PlacePiece(in MoveInfo move)
    {
        UpdateGridArray(move);
        UpdatePiecePosition(move);
    }

    private Side AttackPiece(PieceInfo pieceA, PieceInfo pieceB)
    {
        Side winningSide = GameRules.GetWinningSide(pieceA, pieceB);

        // Remove losing piece
        if (winningSide != Side.None)
        {
            PieceInfo losingPiece = winningSide == Side.A ? pieceB : pieceA;
            KillPiece(losingPiece);
        }
        else
        {
            KillPiece(pieceA);
            KillPiece(pieceB);
        }

        return winningSide;
    }

    private void KillPiece(PieceInfo piece)
    {
        BoardPosition pos = piece.BoardPosition;
        PieceGrid[pos.x, pos.y] = null;

        if (piece.Side == Side.A)
            PiecesA.KillPiece(piece);
        else
            PiecesB.KillPiece(piece);
    }

    private GameOutput CheckGameEnd()
    {
        PieceInfo flagA = PiecesA.Flag;
        PieceInfo flagB = PiecesB.Flag;

        if (!flagA.IsAlive)
        {
            return GameOutput.B;
        }
        else if (!flagB.IsAlive)
        {
            return GameOutput.A;
        }
        else
        {
            if (CurrentSide == Side.B && flagA.BoardPosition.y == HEIGHT - 1)
                return GameOutput.A;
            else if (CurrentSide == Side.A && flagB.BoardPosition.y == 0)
                return GameOutput.B;
        }

        return GameOutput.None;
    }

    private void UpdatePiecePosition(in MoveInfo move)
    {
        PieceInfo pieceInfo = move.PieceInfo;
        BoardPosition newPos = move.NewPosition;
        pieceInfo.BoardPosition = newPos;
    }

    private void UpdateGridArray(in MoveInfo move)
    {
        BoardPosition oldPos = move.OldPosition;
        BoardPosition newPos = move.NewPosition;

        if (IsPositionInsideGrid(oldPos))
            PieceGrid[oldPos.x, oldPos.y] = null;

        PieceGrid[newPos.x, newPos.y] = move.PieceInfo;
    }

    private List<MoveInfo> GetPieceValidMoves(PieceInfo piece)
    {
        List<MoveInfo> moves = new List<MoveInfo>(4);

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

    private bool IsValidMove(in MoveInfo move)
    {
        PieceInfo thisPiece = move.PieceInfo;
        BoardPosition oldPos = move.OldPosition;
        BoardPosition newPos = move.NewPosition;

        if (!IsPositionInsideGrid(newPos))
            return false;

        if (!oldPos.IsPositionAdjacent(newPos))
            return false;

        PieceInfo otherPiece = PieceGrid[newPos.x, newPos.y];

        if (otherPiece == null)
            return true;

        if (otherPiece.Side != thisPiece.Side)
            return true;

        return false;
    }

    private bool IsPositionInsideGrid(in BoardPosition pos)
    {
        return !(pos.x < 0 || pos.y < 0 || pos.x >= WIDTH || pos.y >= HEIGHT);
    }
}
