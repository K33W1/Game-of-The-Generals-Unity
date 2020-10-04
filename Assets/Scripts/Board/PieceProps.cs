using System;
using UnityEngine;

[Serializable]
public struct PieceProps
{
    [Header("Settings")]
    [SerializeField] private PieceRank rank;
    [SerializeField] private bool isPlayerPiece;

    public PieceProps(PieceRank rank, bool isPlayerPiece)
    {
        this.rank = rank;
        this.isPlayerPiece = isPlayerPiece;
    }

    public PieceRank Rank => rank;
    public bool IsPlayerPiece => isPlayerPiece;
}
