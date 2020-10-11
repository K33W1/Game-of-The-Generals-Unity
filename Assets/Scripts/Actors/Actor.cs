using Extensions;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected Side side = Side.None;
    [SerializeField] private int minSpawnHeight = 0;
    [SerializeField] private int maxSpawnHeight = 2;

    protected GameManager gameManager = null;
    protected Board board = null;

    public void Initialize(GameManager gameManager, Board board)
    {
        this.gameManager = gameManager;
        this.board = board;
    }

    public abstract void PerformSpawn();
    public abstract void InitializeEnemyInfo(PieceContainer otherPieces);
    public abstract void PerformMove();

    public void RandomizeSpawns()
    {
        // Get all pieces
        PieceContainer pieceContainer = board.GetPieceContainer(side);
        List<PieceInfo> allPieces = new List<PieceInfo>();
        foreach (PieceInfo piece in pieceContainer.InactivePieces)
            allPieces.Add(piece);
        foreach (PieceInfo piece in pieceContainer.ActivePieces)
            allPieces.Add(piece);

        // List all valid spawn positions
        List<BoardPosition> spawnPos = new List<BoardPosition>();
        for (int i = 0; i < Board.WIDTH; i++)
            for (int j = minSpawnHeight; j <= maxSpawnHeight; j++)
                spawnPos.Add(new BoardPosition(i, j));

        // Shuffle
        spawnPos.Shuffle();

        // Spawn pieces
        for (int i = 0; i < allPieces.Count; i++)
            gameManager.SpawnPiece(new MoveInfo(allPieces[i], spawnPos[i]));
    }
}
