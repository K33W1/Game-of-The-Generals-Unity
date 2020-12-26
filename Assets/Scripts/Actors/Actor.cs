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
    protected PieceContainer myPieces = null;
    protected PieceContainer otherPieces = null;

    private Piece[] myMonoPieces = null;

    public virtual void Initialize(GameManager gameManager, Board board, PieceContainer myPieces, PieceContainer otherPieces)
    {
        this.gameManager = gameManager;
        this.board = board;
        this.myPieces = myPieces;
        this.otherPieces = otherPieces;
    }

    private void Awake()
    {
        myMonoPieces = GetComponentsInChildren<Piece>();
    }

    public abstract void PerformSpawn();
    public abstract void PerformMove();

    public void RandomizeSpawns()
    {
        // List all valid spawn positions
        List<BoardPosition> spawnPos = new List<BoardPosition>();
        for (int i = 0; i < Board.WIDTH; i++)
            for (int j = minSpawnHeight; j <= maxSpawnHeight; j++)
                spawnPos.Add(new BoardPosition(i, j));

        // Shuffle
        spawnPos.Shuffle();

        // Spawn pieces
        for (int i = 0; i < PieceContainer.MAX_CAPACITY; i++)
            gameManager.SpawnPiece(new MoveInfo(myPieces[i], spawnPos[i]));
    }

    public void TogglePieceVisibility()
    {
        foreach (Piece piece in myMonoPieces)
        {
            piece.ToggleVisibility();
        }
    }
}
