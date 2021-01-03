using Extensions;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

        for (int i = 0; i < PieceContainer.MAX_CAPACITY; i++)
        {
            myPieces[i].ID = i;
        }
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
            gameManager.SpawnPiece(new SpawnInfo(myPieces[i], spawnPos[i]));
    }

    public void GenerateSmartSpawns()
    {
        // Get all my pieces
        List<PieceInfo> piecesToSpawn = new List<PieceInfo>(myPieces.AllPieces.Count);
        foreach (PieceInfo piece in myPieces.AllPieces)
        {
            piecesToSpawn.Add(piece);
        }

        // Get rows
        int backRow = side == Side.A ? 0 : 7;
        int midRow = side == Side.A ? 1 : 6;

        // 75% to spawn flag on back row
        // 25% to spawn flag on mid row
        PieceInfo flag = ExtractPieceFromList(piecesToSpawn, PieceRank.Flag);
        BoardPosition flagPos = new BoardPosition(
            Random.Range(0, Board.WIDTH),
            Random.value > 0.25f ? backRow : midRow);

        gameManager.SpawnPiece(new SpawnInfo(flag, flagPos));

        // Get guard pieces for the flag
        PieceInfo guard1 = ExtractPieceFromList(piecesToSpawn, PieceRank.Spy);
        PieceInfo guard2 = ExtractPieceFromList(piecesToSpawn, PieceRank.Private);

        // 3rd guard can be 3, 4, or 5 star general, equally likely
        PieceRank guard3Rank = PieceRank.General5;
        float guard3Chance = Random.value;
        if (guard3Chance > 0.3333f)
            guard3Rank = PieceRank.General3;
        else if (guard3Chance > 0.6666f)
            guard3Rank = PieceRank.General4;
        PieceInfo guard3 = ExtractPieceFromList(piecesToSpawn, guard3Rank);

        // Get guard spawns
        List<BoardPosition> guardSpawns = new List<BoardPosition>();

        // Add sides
        AddBoardPositionIfValid(guardSpawns, flagPos.Left);
        AddBoardPositionIfValid(guardSpawns, flagPos.Right);

        // Add next row
        if (side == Side.A)
        {
            AddBoardPositionIfValid(guardSpawns, flagPos.UpperLeft);
            AddBoardPositionIfValid(guardSpawns, flagPos.Up);
            AddBoardPositionIfValid(guardSpawns, flagPos.UpperRight);
        }
        else
        {
            AddBoardPositionIfValid(guardSpawns, flagPos.DownLeft);
            AddBoardPositionIfValid(guardSpawns, flagPos.Down);
            AddBoardPositionIfValid(guardSpawns, flagPos.DownRight);
        }

        // Randomize spawns
        guardSpawns.Shuffle();

        // Spawn guard pieces
        gameManager.SpawnPiece(new SpawnInfo(guard1, guardSpawns[0]));
        gameManager.SpawnPiece(new SpawnInfo(guard2, guardSpawns[1]));
        gameManager.SpawnPiece(new SpawnInfo(guard3, guardSpawns[2]));

        // List all possible spawns
        float leftValue = 0f;
        float midValue = 0f;
        float rightValue = 0f;
        List<BoardPosition> leftFlank = new List<BoardPosition>();
        List<BoardPosition> midFlank = new List<BoardPosition>();
        List<BoardPosition> rightFlank = new List<BoardPosition>();

        // Left flank
        for (int i = 0; i < Board.WIDTH / 3; i++)
        {
            for (int j = minSpawnHeight; j <= maxSpawnHeight; j++)
            {
                PieceInfo pieceInfo = board.GetPieceFromPosition(i, j);
                if (pieceInfo != null)
                {
                    leftValue += pieceInfo.Rank.GetValue();
                }
                else
                {
                    leftFlank.Add(new BoardPosition(i, j));
                }
            }
        }

        // Middle flank
        for (int i = Board.WIDTH / 3; i < Board.WIDTH / 3 * 2; i++)
        {
            for (int j = minSpawnHeight; j <= maxSpawnHeight; j++)
            {
                PieceInfo pieceInfo = board.GetPieceFromPosition(i, j);
                if (pieceInfo != null)
                {
                    midValue += pieceInfo.Rank.GetValue();
                }
                else
                {
                    midFlank.Add(new BoardPosition(i, j));
                }
            }
        }

        // Right flank
        for (int i = Board.WIDTH / 3 * 2; i < Board.WIDTH; i++)
        {
            for (int j = minSpawnHeight; j <= maxSpawnHeight; j++)
            {
                PieceInfo pieceInfo = board.GetPieceFromPosition(i, j);
                if (pieceInfo != null)
                {
                    rightValue += pieceInfo.Rank.GetValue();
                }
                else
                {
                    rightFlank.Add(new BoardPosition(i, j));
                }
            }
        }

        // Types of spawns. Both equally likely
        // 1. Conservative - Equally distribute to all 3 flanks
        // 2. Blitz - Concentrate power in 1 flank
        if (Random.value > 0.5f)
        {
            // Conservative - Equally distribute
            Debug.Log("Conservative spawning");
            
            // Shuffle first
            piecesToSpawn.Shuffle();
            leftFlank.Shuffle();
            midFlank.Shuffle();
            rightFlank.Shuffle();

            while (piecesToSpawn.Count > 0)
            {
                PieceInfo pieceToSpawn = ExtractPieceFromList(piecesToSpawn, piecesToSpawn[0].Rank);

                // Find smallest flank in terms of value
                if (leftValue <= midValue && leftValue <= rightValue && leftFlank.Count > 0)
                {
                    SpawnPieceFromPositionList(leftFlank, pieceToSpawn);
                    leftValue += pieceToSpawn.Rank.GetValue();
                }
                else if(midValue <= leftValue && midValue <= rightValue && midFlank.Count > 0)
                {
                    SpawnPieceFromPositionList(midFlank, pieceToSpawn);
                    midValue += pieceToSpawn.Rank.GetValue();
                }
                else
                {
                    SpawnPieceFromPositionList(rightFlank, pieceToSpawn);
                    rightValue += pieceToSpawn.Rank.GetValue();
                }
            }
        }
        else
        {
            // Blitz
            Debug.Log("Blitz spawning");

            piecesToSpawn.Sort((pieceInfo1, pieceInfo2) => pieceInfo2.Rank.GetValue().CompareTo(pieceInfo1.Rank.GetValue()));

            // Choose 1 flank
            List<BoardPosition> chosenFlank;
            List<BoardPosition> otherFlanks;

            float flankChance = Random.value;
            if (flankChance < 0.3333f)
            {
                chosenFlank = leftFlank;
                otherFlanks = midFlank;
                otherFlanks.AddRange(rightFlank);
            }
            else if (flankChance < 0.6666f)
            {
                chosenFlank = midFlank;
                otherFlanks = leftFlank;
                otherFlanks.AddRange(rightFlank);
            }
            else
            {
                chosenFlank = rightFlank;
                otherFlanks = leftFlank;
                otherFlanks.AddRange(midFlank);
            }

            chosenFlank.Shuffle();
            otherFlanks.Shuffle();

            foreach (PieceInfo piece in piecesToSpawn)
            {
                // If chosen flank is not empty,
                if (chosenFlank.Count > 0)
                {
                    // 80% chosen blitz flank
                    // 20% for other flanks
                    flankChance = Random.value;
                    if (flankChance < 0.8f)
                    {
                        SpawnPieceFromPositionList(chosenFlank, piece);
                    }
                    else
                    {
                        SpawnPieceFromPositionList(otherFlanks, piece);
                    }
                }
                else
                {
                    SpawnPieceFromPositionList(otherFlanks, piece);
                }
            }
        }
    }

    private void AddBoardPositionIfValid(List<BoardPosition> guardSpawns, BoardPosition left)
    {
        if (board.IsValidPosition(left))
            guardSpawns.Add(left);
    }

    private static PieceInfo ExtractPieceFromList(
        List<PieceInfo> toSpawn,
        PieceRank rank)
    {
        PieceInfo flag = toSpawn.Find(info => info.Rank == rank);
        toSpawn.Remove(flag);
        return flag;
    }

    private void SpawnPieceFromPositionList(
        List<BoardPosition> positions,
        PieceInfo pieceToSpawn)
    {
        BoardPosition spawnPos = positions[positions.Count - 1];
        positions.RemoveAt(positions.Count - 1);
        gameManager.SpawnPiece(new SpawnInfo(pieceToSpawn, spawnPos));
    }

    public void TogglePieceVisibility()
    {
        foreach (Piece piece in myMonoPieces)
        {
            piece.ToggleVisibility();
        }
    }
}
