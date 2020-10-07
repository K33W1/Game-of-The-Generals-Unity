using System;
using UnityEngine;

[Serializable]
public struct PieceProps
{
    [Header("Settings")]
    [SerializeField] private PieceRank rank;
    [SerializeField] private Side side;

    public PieceProps(PieceRank rank, Side side)
    {
        this.rank = rank;
        this.side = side;
    }

    public PieceRank Rank => rank;
    public Side Side => side;
}
