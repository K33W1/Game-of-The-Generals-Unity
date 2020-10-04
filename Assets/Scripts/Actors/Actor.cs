using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Board board = null;
    [SerializeField] protected PieceContainer pieces = null;

    [Header("Settings")]
    [SerializeField] private int minSpawnHeight = 0;
    [SerializeField] private int maxSpawnHeight = 2;

    public abstract void PerformSpawn();
    public abstract void PerformMove();

    public void RandomizeSpawns()
    {
        // Get all pieces
        List<Piece> allPieces = new List<Piece>();
        List<BoardPosition> spawnPos = new List<BoardPosition>();
        foreach (Piece piece in pieces.InactivePieces)
            allPieces.Add(piece);
        foreach (Piece piece in pieces.ActivePieces)
            allPieces.Add(piece);

        // List all valid spawn positions
        for (int i = 0; i < board.Width; i++)
            for (int j = minSpawnHeight; j <= maxSpawnHeight; j++)
                spawnPos.Add(new BoardPosition(i, j));

        // Shuffle
        System.Random random = new System.Random();
        spawnPos = spawnPos.OrderBy(x => random.Next()).ToList();

        // Spawn pieces
        for (int i = 0; i < allPieces.Count; i++)
            board.SpawnPiece(new MoveInfo(allPieces[i], spawnPos[i]));
    }
}
