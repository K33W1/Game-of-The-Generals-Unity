using System.Collections.Generic;
using UnityEngine;

public class PieceContainer
{
    private const int MAX_CAPACITY = 21;

    public List<PieceInfo> ActivePieces { get; }
    public List<PieceInfo> InactivePieces { get; }
    public PieceInfo Flag { get; private set; };

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
    }

    public PieceContainer Copy()
    {
        List<PieceInfo> activePieces = new List<PieceInfo>(MAX_CAPACITY);
        List<PieceInfo> inactivePieces = new List<PieceInfo>(MAX_CAPACITY);
        PieceInfo flag = Flag;

        for (int i = 0; i < ActivePieces.Count; i++)
            activePieces.Add(ActivePieces[i].Copy());
        for (int i = 0; i < InactivePieces.Count; i++)
            inactivePieces.Add(InactivePieces[i].Copy());

        return new PieceContainer(activePieces, inactivePieces, flag);
    }

    public void Add(PieceInfo pieceInfo)
    {
        InactivePieces.Add(pieceInfo);
        
        if (pieceInfo.Rank == PieceRank.Flag)
            Flag = pieceInfo;

        Debug.Assert(InactivePieces.Count + ActivePieces.Count <= 21);
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
}
