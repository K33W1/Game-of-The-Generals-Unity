using System.Collections.Generic;
using UnityEngine;

public class Graveyard : MonoBehaviour
{
    [SerializeField] private Transform[] graveyardPositions = null;
    
    private List<Piece> deadPieces = null;

    private void Awake()
    {
        deadPieces = new List<Piece>(21);
    }

    public void AddPiece(Piece piece)
    {
        piece.transform.position =
            graveyardPositions[deadPieces.Count].position;
        deadPieces.Add(piece);
    }
}
