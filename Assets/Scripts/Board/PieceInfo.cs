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
    public PieceRank Rank => rank;
    public Side Side => side;

    public PieceInfo(PieceRank rank, Side side)
    {
        BoardPosition = new BoardPosition(-1, -1);
        IsAlive = true;
        this.rank = rank;
        this.side = side;
    }
}
