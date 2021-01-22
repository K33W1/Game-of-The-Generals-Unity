using Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceContainer : IEnumerable<PieceInfo>, IDeepCopyable<PieceContainer>
{
    public const int MAX_CAPACITY = 21;

    public List<PieceInfo> AllPieces { get; }
    public List<PieceInfo> ActivePieces { get; }
    public List<PieceInfo> InactivePieces { get; }
    public PieceInfo Flag { get; }

    public PieceContainer(List<PieceInfo> allPieces)
    {
        Debug.Assert(allPieces.Capacity == MAX_CAPACITY);

        AllPieces = allPieces;
        ActivePieces = new List<PieceInfo>(MAX_CAPACITY);
        InactivePieces = new List<PieceInfo>(MAX_CAPACITY);
        Flag = allPieces.Find(pieceInfo => pieceInfo.Rank == PieceRank.Flag);

        foreach (PieceInfo pieceInfo in allPieces)
        {
            if (pieceInfo.IsAlive)
                ActivePieces.Add(pieceInfo);
            else
                InactivePieces.Add(pieceInfo);
        }
    }
    
    public PieceContainer(List<PieceInfo> allPieces, List<PieceInfo> activePieces, List<PieceInfo> inactivePieces, PieceInfo flag)
    {
        Debug.Assert(activePieces.Capacity == MAX_CAPACITY);
        Debug.Assert(inactivePieces.Capacity == MAX_CAPACITY);

        AllPieces = allPieces;
        ActivePieces = activePieces;
        InactivePieces = inactivePieces;
        Flag = flag;
    }

    public PieceContainer DeepCopy()
    {
        List<PieceInfo> allPieces = AllPieces;
        List<PieceInfo> activePieces = ActivePieces.DeepCopy();
        List<PieceInfo> inactivePieces = InactivePieces.DeepCopy();
        PieceInfo flag = activePieces.Find(piece => piece.Rank == PieceRank.Flag);
        return new PieceContainer(allPieces, activePieces, inactivePieces, flag);
    }

    public PieceContainer DeepCopyWithHiddenPieces()
    {
        PieceContainer newPieceContainer = DeepCopy();

        // Invalidate all pieces
        foreach (PieceInfo pieceInfo in newPieceContainer)
            pieceInfo.Rank = PieceRank.Invalid;

        return newPieceContainer;
    }

    public PieceInfo this[int i]
    {
        // TODO: replace get with AllPieces[i];
        get => i < ActivePieces.Count ? ActivePieces[i] : InactivePieces[i - ActivePieces.Count];
        set => this[i] = value;
    }

    public PieceInfo GetPieceFromRank(PieceRank rank)
    {
        return AllPieces.Find(info => info.Rank == rank);
    }

    public List<PieceInfo> GetAllPiecesFromRank(PieceRank rank)
    {
        return AllPieces.FindAll(info => info.Rank == rank);
    }

    public void ActivatePiece(PieceInfo piece)
    {
        if (InactivePieces.Remove(piece))
        {
            piece.IsAlive = true;
            ActivePieces.Add(piece);
        }
    }

    public void KillPiece(PieceInfo piece)
    {
        if (ActivePieces.Remove(piece))
        {
            piece.IsAlive = false;
            InactivePieces.Add(piece);
        }
        else
        {
            Debug.LogError("Piece can't be removed!");
        }
    }

    public IEnumerator<PieceInfo> GetEnumerator()
    {
        for (int i = 0; i < ActivePieces.Count; i++)
            yield return ActivePieces[i];
        for (int i = 0; i < InactivePieces.Count; i++)
            yield return InactivePieces[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
