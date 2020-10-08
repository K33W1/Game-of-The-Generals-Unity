using System.Collections.Generic;
using UnityEngine;

public class PieceContainer
{
    public PieceContainer()
    {
        ActivePieces = new List<PieceInfo>();
        InactivePieces = new List<PieceInfo>();
    }

    public List<PieceInfo> ActivePieces { get; }
    public List<PieceInfo> InactivePieces { get; }

    public PieceInfo GetPiece(PieceRank rank)
    {
        return ActivePieces.Find(piece => piece.Rank == rank);
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
