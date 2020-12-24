using Extensions;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class EnemyAI : Actor
{
    [SerializeField] private PieceCounterPanel pieceCounterPanel = null;

    private readonly List<int> piecePool = new List<int>();
    private readonly Dictionary<PieceInfo, RankPossibilities> enemyPiecesPossibilities =
        new Dictionary<PieceInfo, RankPossibilities>();
    private bool isThinking = false;

    public override void Initialize(
        GameManager gameManager,
        Board board,
        PieceContainer myPieces,
        PieceContainer otherPieces)
    {
        base.Initialize(gameManager, board, myPieces, otherPieces);

        foreach (PieceInfo pieceInfo in otherPieces)
        {
            RankPossibilities rankPossibilities = new RankPossibilities(piecePool);
            enemyPiecesPossibilities.Add(pieceInfo, rankPossibilities);
        }

        for (int i = 0; i < 15; i++)
        {
            piecePool.Add(21);
        }
    }

    public override void PerformSpawn()
    {
        if (isThinking)
            return;

        isThinking = true;

        // TODO: Smart spawning
        RandomizeSpawns();
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

        // DEBUG: Print counts for each pieces
        if (pieceCounterPanel != null)
        {
            pieceCounterPanel.UpdateText(piecePool);
        }
    }

    private void UpdateEnemyInfo(BoardChange boardChange)
    {
        if (!boardChange.WasThereAnAttack())
            return;

        PieceInfo thisPiece = boardChange.GetPiece(side);
        PieceInfo otherPiece = boardChange.GetPiece(side.Flip());
        RankPossibilities enemyRankPossibilities = enemyPiecesPossibilities[otherPiece];

        if (boardChange.GetWinningPiece() == null)
        {
            enemyRankPossibilities.TiedBattle(thisPiece.Rank);
        }
        else if (otherPiece == boardChange.GetWinningPiece())
        {
            enemyRankPossibilities.WonBattle(thisPiece.Rank);
        }
        else // This piece is the winner
        {
            enemyRankPossibilities.LostBattle(thisPiece.Rank);
        }
    }

    private MoveInfo GetMove()
    {
        List<MoveInfo> allPossibleMoves = board.GetAllValidMoves();
        List<Fraction> results = new List<Fraction>(allPossibleMoves.Count);

        // Explore each move
        for (int i = 0; i < allPossibleMoves.Count; i++)
        {
            // Start with 0/0 fractional results
            Fraction result = new Fraction(0, 0);

            // Explore this move a set number of times
            for (int j = 0; j < 2; j++)
            {
                Board boardCopy = board.GetCopyWithHiddenPieces(side);

                GuessEnemyPieces(boardCopy);

                boardCopy.MovePiece(boardCopy.GetAllValidMoves()[i]);

                // Play random moves until end game
                while (boardCopy.CurrentGameOutput == Side.None)
                {
                    List<MoveInfo> currentValidMoves = boardCopy.GetAllValidMoves();
                    int randomIndex = Random.Range(0, currentValidMoves.Count);
                    boardCopy.MovePiece(currentValidMoves[randomIndex]);
                }

                // Update result
                if (boardCopy.CurrentGameOutput == Side.B)
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

        return allPossibleMoves[bestMoveIndex];
    }

    private void GuessEnemyPieces(Board boardCopy)
    {
        // TODO: Smart guessing
        List<PieceRank> ranks = new List<PieceRank>
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

        

        // Shuffle
        ranks.Shuffle();
        for (int k = 0; k < PieceContainer.MAX_CAPACITY; k++)
        {
            boardCopy.PiecesA[k].Rank = ranks[k];
        }
    }
}
