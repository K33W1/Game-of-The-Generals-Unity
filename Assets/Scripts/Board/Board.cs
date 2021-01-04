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
    public Side CurrentGameOutput { get; private set; } = Side.None;
    public Side CurrentSide { get; private set; } = Side.None;

    public Board(PieceInfo[,] pieceGrid,
        PieceContainer piecesA,
        PieceContainer piecesB,
        BoardChange? boardChange,
        GamePhase currentGamePhase,
        Side currentGameOutput,
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
        PieceContainer piecesA = side == Side.A ? PiecesA.DeepCopy() : PiecesA.DeepCopyWithHiddenPieces();
        PieceContainer piecesB = side == Side.A ? PiecesB.DeepCopyWithHiddenPieces() : PiecesB.DeepCopy();

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
        List<MoveInfo> allPossibleMoves = new List<MoveInfo>();
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

    public bool SpawnPiece(in SpawnInfo spawnInfo)
    {
        BoardPosition pos = spawnInfo.NewPosition;

        if (CurrentSide == Side.A)
        {
            if (pos.x < 0 || pos.x >= WIDTH || pos.y < 0 || pos.y > 2)
                return false;

            if (PieceGrid[pos.x, pos.y] != null)
                return false;

            PlacePiece(spawnInfo.PieceInfo, spawnInfo.NewPosition);
            PiecesA.ActivatePiece(spawnInfo.PieceInfo);
        }
        else if (CurrentSide == Side.B)
        {
            if (pos.x < 0 || pos.x >= WIDTH || pos.y < 5 || pos.y > 7)
                return false;

            if (PieceGrid[pos.x, pos.y] != null)
                return false;

            PlacePiece(spawnInfo.PieceInfo, spawnInfo.NewPosition);
            PiecesB.ActivatePiece(spawnInfo.PieceInfo);
        }
        else
        {
            Debug.LogError("Invalid current side when spawning!");
            return false;
        }

        return true;
    }

    public bool ConfirmSpawn()
    {
        if (CurrentSide == Side.A)
        {
            if (IsValidSpawn(CurrentSide))
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
            if (IsValidSpawn(CurrentSide))
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
            Side gameOutput = CheckGameEnd();
            if (gameOutput != Side.None)
            {
                CurrentSide = Side.None;
                CurrentGamePhase = GamePhase.End;
                CurrentGameOutput = gameOutput;
                return true;
            }

            // Flip sides
            CurrentSide = CurrentSide.Flip();
            return true;
        }

        return false;
    }

    public bool DoesPieceExistInPosition(in BoardPosition pos)
    {
        return PieceGrid[pos.x, pos.y] != null;
    }

    public void ForceMove(in MoveInfo move)
    {
        Debug.Assert(!DoesPieceExistInPosition(move.NewPosition));

        // Place piece
        PlacePiece(GetPieceFromPosition(move.OldPosition), move.NewPosition);
    }

    public void ForceMoveWin(in MoveInfo move)
    {
        Debug.Assert(DoesPieceExistInPosition(move.NewPosition));

        // Get needed vars
        BoardPosition oldPos = move.OldPosition;
        BoardPosition newPos = move.NewPosition;
        PieceInfo attackingPiece = PieceGrid[oldPos.x, oldPos.y];
        PieceInfo dyingPiece = PieceGrid[newPos.x, newPos.y];

        // Kill and move into other piece
        KillPiece(dyingPiece);
        PlacePiece(attackingPiece, newPos);
    }

    public void ForceMoveLose(in MoveInfo move)
    {
        Debug.Assert(DoesPieceExistInPosition(move.NewPosition));

        // Get needed vars
        BoardPosition oldPos = move.OldPosition;
        PieceInfo pieceInfo = PieceGrid[oldPos.x, oldPos.y];

        // Kill this piece
        KillPiece(pieceInfo);
    }

    public void ForceFlipSides()
    {
        CurrentSide = CurrentSide.Flip();
    }

    private BoardChange? TryMovePiece(in MoveInfo move)
    {
        if (!IsValidMove(move))
        {
            Debug.Log("Invalid move!");
            return null;
        }

        BoardPosition oldPos = move.OldPosition;
        BoardPosition newPos = move.NewPosition;
        PieceInfo thisPieceInfo = PieceGrid[oldPos.x, oldPos.y];
        PieceInfo otherPieceInfo = PieceGrid[newPos.x, newPos.y];
        Side winningSide = Side.None;

        // If other piece exists on target position
        if (otherPieceInfo != null)
        {
            winningSide = AttackPiece(thisPieceInfo, otherPieceInfo);

            // Check if attacking piece won the battle
            if (winningSide == Side.A)
                PlacePiece(thisPieceInfo, newPos);
        }
        else
        {
            PlacePiece(thisPieceInfo, newPos);
        }

        return new BoardChange(move, thisPieceInfo, otherPieceInfo, winningSide);
    }

    private void PlacePiece(PieceInfo pieceInfo, in BoardPosition pos)
    {
        UpdateGridArray(pieceInfo, pieceInfo.BoardPosition, pos);
        UpdatePiecePosition(pieceInfo, pos);
    }

    private Side AttackPiece(PieceInfo pieceA, PieceInfo pieceB)
    {
        Side winningSide = GameRules.GetWinningSide(pieceA.Rank, pieceB.Rank);

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

    private Side CheckGameEnd()
    {
        PieceInfo flagA = PiecesA.Flag;
        PieceInfo flagB = PiecesB.Flag;

        if (!flagA.IsAlive)
        {
            return Side.B;
        }
        else if (!flagB.IsAlive)
        {
            return Side.A;
        }
        else
        {
            if (CurrentSide == Side.B && flagA.BoardPosition.y == HEIGHT - 1)
                return Side.A;
            else if (CurrentSide == Side.A && flagB.BoardPosition.y == 0)
                return Side.B;
        }

        return Side.None;
    }

    private bool IsValidSpawn(Side side)
    {
        int minRow = side == Side.A ? 0 : 5;
        int maxRow = side == Side.A ? 2 : 7;
        int count = 0;

        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = minRow; j <= maxRow; j++)
            {
                if (PieceGrid[i, j] != null)
                    count++;
            }
        }

        return count == PieceContainer.MAX_CAPACITY;
    }

    private void UpdatePiecePosition(PieceInfo pieceInfo, in BoardPosition pos)
    {
        pieceInfo.BoardPosition = pos;
    }

    private void UpdateGridArray(
        PieceInfo pieceInfo,
        in BoardPosition oldPos,
        in BoardPosition newPos)
    {
        if (IsValidPosition(oldPos))
            PieceGrid[oldPos.x, oldPos.y] = null;

        PieceGrid[newPos.x, newPos.y] = pieceInfo;
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
        BoardPosition oldPos = move.OldPosition;
        PieceInfo thisPiece = PieceGrid[oldPos.x, oldPos.y];
        BoardPosition newPos = move.NewPosition;

        if (!IsValidPosition(newPos))
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

    public bool IsValidPosition(in BoardPosition pos)
    {
        return !(pos.x < 0 || pos.y < 0 || pos.x >= WIDTH || pos.y >= HEIGHT);
    }

    public bool IsValidPosition(int x, int y)
    {
        return !(x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT);
    }

    public PieceInfo GetPieceFromPosition(int x, int y)
    {
        return IsValidPosition(x, y) ? PieceGrid[x, y] : null; 
    }

    public PieceInfo GetPieceFromPosition(in BoardPosition pos)
    {
        return IsValidPosition(pos) ? PieceGrid[pos.x, pos.y] : null;
    }
}
