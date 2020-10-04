﻿using System.Collections.Generic;
using UnityEngine;

public class PieceContainer : MonoBehaviour
{
    public List<Piece> ActivePieces { get; private set; } = new List<Piece>();
    public List<Piece> InactivePieces { get; private set; } = new List<Piece>();

    private void Start()
    {
        foreach (Piece piece in transform.GetComponentsInChildren<Piece>())
            InactivePieces.Add(piece);
    }

    public Piece GetPiece(PieceRank rank)
    {
        return ActivePieces.Find(piece => piece.Properties.Rank == rank);
    }

    public void ActivatePiece(Piece piece)
    {
        if (InactivePieces.Remove(piece))
            ActivePieces.Add(piece);
    }

    public void KillPiece(Piece piece)
    {
        if (ActivePieces.Remove(piece))
        {
            InactivePieces.Add(piece);
            piece.Disable();
        }
        else
        {
            Debug.LogError("Piece can't be removed!");
        }
    }

    public bool IsValidSpawn()
    {
        if (InactivePieces.Count == 0)
            return true;

        return false;
    }

    public void ToggleVisibility()
    {
        // TODO: Toggle visibility
    }
}