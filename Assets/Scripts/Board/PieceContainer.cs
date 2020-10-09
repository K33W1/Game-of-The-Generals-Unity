using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceContainer : IEnumerable<PieceInfo>
{
    public const int MAX_CAPACITY = 21;

    public List<PieceInfo> ActivePieces { get; }
    public List<PieceInfo> InactivePieces { get; }
    public PieceInfo Flag { get; private set; }

    private int enumeratorIndex = 0;

    public PieceContainer()
    {
        ActivePieces = new List<PieceInfo>(21);
        InactivePieces = new List<PieceInfo>(21);
        Flag = null;
    }

    public PieceContainer(List<PieceInfo> activePieces, List<PieceInfo> inactivePieces, PieceInfo flag)
    {
        Debug.Assert(activePieces.Capacity == MAX_CAPACITY);
        Debug.Assert(inactivePieces.Capacity == MAX_CAPACITY);
        Debug.Assert(flag != null);

        ActivePieces = activePieces;
        InactivePieces = inactivePieces;
        Flag = flag;
    }

    public PieceContainer Copy()
    {
        List<PieceInfo> activePieces = new List<PieceInfo>(MAX_CAPACITY);
        List<PieceInfo> inactivePieces = new List<PieceInfo>(MAX_CAPACITY);
        PieceInfo flag = null;

        for (int i = 0; i < ActivePieces.Count; i++)
        {
            PieceInfo newPiece = ActivePieces[i].Copy();
            activePieces.Add(newPiece);

            if (newPiece.Rank == PieceRank.Flag)
                flag = newPiece;
        }
        for (int i = 0; i < InactivePieces.Count; i++)
            inactivePieces.Add(InactivePieces[i].Copy());

        return new PieceContainer(activePieces, inactivePieces, flag);
    }

    public PieceInfo this[int i]
    {
        get => i < ActivePieces.Count ? ActivePieces[i] : InactivePieces[i - ActivePieces.Count];
        set => this[i] = value;
    }

    public void Add(PieceInfo pieceInfo)
    {
        InactivePieces.Add(pieceInfo);
        
        if (pieceInfo.Rank == PieceRank.Flag)
            Flag = pieceInfo;

        Debug.Assert(InactivePieces.Count + ActivePieces.Count <= MAX_CAPACITY);
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

    public bool IsValidSpawn()
    {
        if (InactivePieces.Count > 0)
            return false;

        return true;
    }

    public void ToggleVisibility()
    {
        // TODO: Toggle visibility
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
