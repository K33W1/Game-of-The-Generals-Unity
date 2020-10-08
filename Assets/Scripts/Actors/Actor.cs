using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected GameManager gameManager = null;

    [Header("Settings")]
    [SerializeField] protected Side side = Side.Invalid;
    [SerializeField] private int minSpawnHeight = 0;
    [SerializeField] private int maxSpawnHeight = 2;

    public abstract void PerformSpawn();
    public abstract void PerformMove();

    public void RandomizeSpawns()
    {
        // Get all pieces
        PieceContainer pieceContainer = gameManager.Board.GetPieceContainer(side);
        List<PieceInfo> allPieces = new List<PieceInfo>();
        foreach (PieceInfo piece in pieceContainer.InactivePieces)
            allPieces.Add(piece);
        foreach (PieceInfo piece in pieceContainer.ActivePieces)
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
            gameManager.SpawnPiece(new MoveInfo(allPieces[i], spawnPos[i]));
    }
}
