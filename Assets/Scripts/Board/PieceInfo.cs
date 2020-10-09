using System;
using UnityEngine;

[Serializable]
public class PieceInfo
{
    [Header("Settings")]
    [SerializeField] private BoardPosition boardPosition;
    [SerializeField] private bool isAlive;
    [SerializeField] private PieceRank rank;
    [SerializeField] private Side side;

    public BoardPosition BoardPosition { get => boardPosition; set => boardPosition = value; }
    public bool IsAlive { get => isAlive; set => isAlive = value; }
    public PieceRank Rank { get => rank; set => rank = value; }
    public Side Side => side;

    public PieceInfo(BoardPosition boardPosition, bool isAlive, PieceRank rank, Side side)
    {
        this.boardPosition = boardPosition;
        this.isAlive = isAlive;
        this.rank = rank;
        this.side = side;
    }

    public PieceInfo Copy()
    {
        return new PieceInfo(boardPosition, isAlive, rank, side);
    }
}
