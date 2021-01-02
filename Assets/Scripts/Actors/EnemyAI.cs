using Extensions;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class EnemyAI : Actor
{
    [SerializeField] private PieceCounterPanel pieceCounterPanel = null;
    [SerializeField] private bool printBestScore = false;
    [SerializeField, Min(0)] private int minIterations = 1;
    [SerializeField, Min(0)] private int maxIterations = 200;

    private readonly Dictionary<PieceInfo, RankPossibilities> enemyPieces =
        new Dictionary<PieceInfo, RankPossibilities>();
    private bool isThinking = false;
    private readonly List<int> piecePool = new List<int>
    {
        2,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        6,
        1
    };

    private readonly List<PieceRank> ranks = new List<PieceRank>
    {
        PieceRank.Spy,
        PieceRank.Spy,
        PieceRank.General5,
        PieceRank.General4,
        PieceRank.General3,
        PieceRank.General2,
        PieceRank.General1,
        PieceRank.Colonel,
        PieceRank.LtColonel,
        PieceRank.Major,
        PieceRank.Captain,
        PieceRank.Lieutenant1,
        PieceRank.Lieutenant2,
        PieceRank.Sergeant,
        PieceRank.Private,
        PieceRank.Private,
        PieceRank.Private,
        PieceRank.Private,
        PieceRank.Private,
        PieceRank.Private,
        PieceRank.Flag
    };

    public override void Initialize(
        GameManager gameManager,
        Board board,
        PieceContainer myPieces,
        PieceContainer otherPieces)
    {
        base.Initialize(gameManager, board, myPieces, otherPieces);

        foreach (PieceInfo pieceInfo in otherPieces)
        {
            RankPossibilities rankPossibilities = new RankPossibilities();
            enemyPieces.Add(pieceInfo, rankPossibilities);
        }

        pieceCounterPanel.UpdateText(piecePool);

        TogglePieceVisibility();
    }

    public override void PerformSpawn()
    {
        if (isThinking)
            return;
        
        isThinking = true;

        GenerateSmartSpawns();
        gameManager.ConfirmSpawn();

        isThinking = false;
    }

    public override void PerformMove()
    {
        // Keep time
        float startingTime = Time.realtimeSinceStartup;

        // Update info about enemy's pieces
        UpdateEnemyInfo(board.BoardChange.Value);
        gameManager.MovePiece(GetMove());
        UpdateEnemyInfo(board.BoardChange.Value);

        // DEBUG: Print time it took
        float endingTime = Time.realtimeSinceStartup;
        Debug.Log("AI took " + (endingTime - startingTime));
    }

    private void UpdateEnemyInfo(BoardChange boardChange)
    {
        if (!boardChange.WasThereAnAttack())
            return;

        PieceInfo thisPiece = boardChange.GetPieceFromAction(side);
        PieceInfo otherPiece = boardChange.GetPieceFromAction(side.Flip());
        RankPossibilities otherPieceRank = enemyPieces[otherPiece];

        if (boardChange.GetWinningPiece() == null)
        {
            otherPieceRank.TiedBattle(thisPiece.Rank);
        }
        else if (otherPiece == boardChange.GetWinningPiece())
        {
            otherPieceRank.WonBattle(thisPiece.Rank);
        }
        else // This piece is the winner
        {
            otherPieceRank.LostBattle(thisPiece.Rank);
        }

        // Remove from piece pool once piece has been discovered
        PieceRank guaranteedRank = otherPieceRank.GuaranteedRank;
        if (guaranteedRank == PieceRank.Invalid)
            return;

        piecePool[(int) guaranteedRank]--;
        pieceCounterPanel.UpdateText(piecePool);

        // If piece pool reaches 0, all other pieces can't be this piece
        if (piecePool[(int) guaranteedRank] > 0)
            return;

        foreach (RankPossibilities rankPossibilities in enemyPieces.Values)
        {
            if (rankPossibilities.GuaranteedRank == guaranteedRank)
                continue;
                
            rankPossibilities.RemovePiecePossibility(guaranteedRank);
        }
    }

    private MoveInfo GetMove()
    {
        List<MoveInfo> allPossibleMoves = board.GetAllValidMoves();
        List<Fraction> results = new List<Fraction>(allPossibleMoves.Count);
        int iterations = GetIterations();

        // Explore each move
        for (int i = 0; i < allPossibleMoves.Count; i++)
        {
            // Start with 0/0 fractional results
            Fraction result = new Fraction(0, 0);

            // Explore this move a set number of times
            for (int j = 0; j < iterations; j++)
            {
                Board boardCopy = board.GetCopyWithHiddenPieces(side);
                GuessEnemyPieces(boardCopy);
                boardCopy.MovePiece(boardCopy.GetAllValidMoves()[i]);

                // Play random moves until end game
                Rollout(boardCopy);

                // If won, increase numerator
                if (boardCopy.CurrentGameOutput == side)
                {
                    result.Numerator++;
                }

                result.Denominator++;
            }

            results.Add(result);
        }

        // Find best move
        int bestMoveIndex = 0;
        float bestScore = 0;
        for (int i = 0; i < results.Count; i++)
        {
            float result = results[i].GetDecimal();

            if (result > bestScore)
            {
                bestScore = result;
                bestMoveIndex = i;
            }
        }

        if (printBestScore)
        {
            Debug.Log("Best score: " + bestScore);
        }

        return allPossibleMoves[bestMoveIndex];
    }

    private void Rollout(Board boardCopy)
    {
        while (boardCopy.CurrentGameOutput == Side.None)
        {
            List<MoveInfo> currentValidMoves = boardCopy.GetAllValidMoves();
            int randomIndex = Random.Range(0, currentValidMoves.Count);
            boardCopy.MovePiece(currentValidMoves[randomIndex]);
        }
    }

    private int GetIterations()
    {
        // Get sum of confidences
        float sum = 0f;
        foreach (RankPossibilities rankPossibilities in enemyPieces.Values)
        {
            sum += rankPossibilities.GetConfidence();
        }

        // Average confidence
        float confidence = sum / enemyPieces.Count;
        return (int) (minIterations + (maxIterations - minIterations) * confidence);
    }

    private void GuessEnemyPieces(Board boardCopy)
    {
        // Shuffle
        ranks.Shuffle();

        // Check if all pieces have possible ranks
        for (int i = 0; i < PieceContainer.MAX_CAPACITY; i++)
        {
            PieceInfo piece1 = otherPieces[i];
            RankPossibilities piece1RankPossibilities = enemyPieces[piece1];
            PieceRank piece1Rank = ranks[i];

            // Check if piece is possible
            if (piece1RankPossibilities.IsPiecePossible(piece1Rank))
                continue;
            
            // Attempt to switch piece rank
            bool didSwitch = false;
            for (int j = 0; j < PieceContainer.MAX_CAPACITY; j++)
            {
                PieceInfo piece2 = otherPieces[j];
                RankPossibilities piece2RankPossibilities = enemyPieces[piece2];
                PieceRank piece2Rank = ranks[j];

                // Check if switching works
                if (piece2RankPossibilities.IsPiecePossible(piece1Rank) &&
                    piece1RankPossibilities.IsPiecePossible(piece2Rank))
                {
                    // Switch ranks
                    ranks[i] = piece2Rank;
                    ranks[j] = piece1Rank;
                    didSwitch = true;
                    break;
                }
            }

            if (!didSwitch)
            {
                Debug.LogWarning("Failed to switch!");
            }
        }

        // Copy ranks to board
        PieceContainer otherPiecesCopy = boardCopy.GetPieceContainer(side.Flip());
        for (int i = 0; i < PieceContainer.MAX_CAPACITY; i++)
        {
            otherPiecesCopy[i].Rank = ranks[i];
        }
    }
}
