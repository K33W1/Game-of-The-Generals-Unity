using System.Collections.Generic;
using UnityEngine;

public class PieceContainer
{
    public PieceContainer()
    {
        ActivePieces = new List<Piece>();
        InactivePieces = new List<Piece>();
    }

    public List<Piece> ActivePieces { get; }
    public List<Piece> InactivePieces { get; }

    public Piece GetPiece(PieceRank rank)
    {
        return ActivePieces.Find(piece => piece.Properties.Rank == rank);
    }

    public void ActivatePiece(Piece piece)
    {
        if (InactivePieces.Remove(piece))
        {
            piece.gameObject.SetActive(true);
            ActivePieces.Add(piece);
        }
    }

    public void KillPiece(Piece piece)
    {
        if (ActivePieces.Remove(piece))
        {
            InactivePieces.Add(piece);
            piece.gameObject.SetActive(false);
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
