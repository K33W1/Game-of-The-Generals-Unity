using Extensions;
using System;
using UnityEngine;

[Serializable]
public class PieceInfo : IDeepCopyable<PieceInfo>
{
    [Header("Settings")]
    [SerializeField] private BoardPosition boardPosition;
    [SerializeField] private bool isAlive;
    [SerializeField] private PieceRank rank;
    [SerializeField] private Side side;

    public int ID { get; set; }
    public BoardPosition BoardPosition { get => boardPosition; set => boardPosition = value; }
    public bool IsAlive { get => isAlive; set => isAlive = value; }
    public PieceRank Rank { get => rank; set => rank = value; }
    public Side Side => side;

    public PieceInfo(int id, BoardPosition boardPosition, bool isAlive, PieceRank rank, Side side)
    {
        Debug.Assert(side != Side.None);

        ID = id;
        this.boardPosition = boardPosition;
        this.isAlive = isAlive;
        this.rank = rank;
        this.side = side;
    }

    public PieceInfo DeepCopy()
    {
        return new PieceInfo(ID, boardPosition, isAlive, rank, side);
    }
}
