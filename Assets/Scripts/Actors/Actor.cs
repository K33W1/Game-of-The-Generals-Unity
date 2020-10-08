using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Board board = null;

    [Header("Settings")]
    [SerializeField] private Side side = Side.Invalid;
    [SerializeField] private int minSpawnHeight = 0;
    [SerializeField] private int maxSpawnHeight = 2;

    public abstract void PerformSpawn();
    public abstract void PerformMove();

    public void RandomizeSpawns()
    {
        // Get all pieces
        PieceContainer pieceContainer = board.GetPieceContainer(side);
        List<Piece> allPieces = new List<Piece>();
        foreach (Piece piece in pieceContainer.InactivePieces)
            allPieces.Add(piece);
        foreach (Piece piece in pieceContainer.ActivePieces)
            allPieces.Add(piece);

        // List all valid spawn positions
        List<BoardPosition> spawnPos = new List<BoardPosition>();
        for (int i = 0; i < Board.Width; i++)
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
