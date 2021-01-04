using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyAI : Actor
{
    [SerializeField] private PieceCounterPanel pieceCounterPanel = null;
    [SerializeField] private LoadingScreen loadingScreen = null;
    [SerializeField] private bool printBestScore = false;
    [SerializeField, Min(0)] private float flagAtRiskMultiplier = 1000f;
    [SerializeField, Min(0)] private float opennessMultiplier = 2f;
    [SerializeField, Min(0)] private float aggressionMultiplier = 2f;
    [SerializeField, Min(0)] private float forwardBonus = 10f;
    [SerializeField, Min(0)] private float winningBattleBonus = 100f;
    [SerializeField, Min(0)] private float losingBattlePenalty = 50f;
    [SerializeField, Min(1)] private int maxDepth = 3;

    private readonly Dictionary<int, RankPossibilities> enemyPieces =
        new Dictionary<int, RankPossibilities>();
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

    private MoveInfo chosenMove = new MoveInfo();

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
            enemyPieces.Add(pieceInfo.ID, rankPossibilities);
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
        StartCoroutine(PerformMoveCoroutine());
    }

    private IEnumerator PerformMoveCoroutine()
    {
        // Keep time
        float startingTime = Time.realtimeSinceStartup;

        // Update info about enemy's pieces
        UpdateEnemyInfo(board.BoardChange.Value);
        yield return StartCoroutine(GetMove());
        gameManager.MovePiece(chosenMove);
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
        RankPossibilities otherPieceRank = enemyPieces[otherPiece.ID];

        if (boardChange.GetWinningPiece() == null)
            otherPieceRank.TiedBattle(thisPiece.Rank);
        else if (otherPiece == boardChange.GetWinningPiece())
            otherPieceRank.WonBattle(thisPiece.Rank);
        else // This piece is the winner
            otherPieceRank.LostBattle(thisPiece.Rank);

        // Remove from piece pool once piece has been discovered
        PieceRank guaranteedRank = otherPieceRank.GuaranteedRank;
        if (guaranteedRank == PieceRank.Invalid)
            return;

        // DEBUG
        piecePool[(int)guaranteedRank]--;
        pieceCounterPanel.UpdateText(piecePool);

        // If piece pool reaches 0, all other pieces can't be this piece
        if (piecePool[(int)guaranteedRank] > 0)
            return;

        foreach (RankPossibilities rankPossibilities in enemyPieces.Values)
        {
            if (rankPossibilities.GuaranteedRank == guaranteedRank)
                continue;

            rankPossibilities.RemovePiecePossibility(guaranteedRank);
        }
    }

    private IEnumerator GetMove()
    {
        var allPossibleMoves = board.GetAllValidMoves();
        float bestResult = float.MinValue;
        int bestResultIndex = 0;

        // UI
        loadingScreen.Show();

        // Explore each move
        for (int i = 0; i < allPossibleMoves.Count; i++)
        {
            MoveInfo move = allPossibleMoves[i];
            
            float finalResult = EvaluateMove(
                board.GetCopyWithHiddenPieces(side),
                move,
                maxDepth - 1);

            if (finalResult > bestResult)
            {
                bestResult = finalResult;
                bestResultIndex = i;
            }

            // Update loading screen
            loadingScreen.SetScrollbarValue((float)i / allPossibleMoves.Count);

            yield return 0;
        }

        loadingScreen.Hide();

        // DEBUG
        if (printBestScore)
            Debug.Log("Best result: " + bestResult);

        chosenMove = allPossibleMoves[bestResultIndex];
    }

    private float EvaluateMove(Board boardCopy, MoveInfo move, int depth)
    {
        float resultSum = 0f;

        if (side == Side.A)
        {
            if (move.GetDifference().y > 0)
                resultSum += forwardBonus;
        }
        else
        {
            if (move.GetDifference().y < 0)
                resultSum += forwardBonus;
        }

        // If piece exists in next position, this is an attacking move
        if (boardCopy.DoesPieceExistInPosition(move.NewPosition))
        {
            // Get needed vars
            PieceInfo oldPosPiece = boardCopy.GetPieceFromPosition(move.OldPosition);
            PieceInfo newPosPiece = boardCopy.GetPieceFromPosition(move.NewPosition);
            PieceInfo myPiece = boardCopy.CurrentSide == side ? oldPosPiece : newPosPiece;
            PieceInfo otherPiece = boardCopy.CurrentSide == side ? newPosPiece : oldPosPiece;
            PieceRank myRank = myPiece.Rank;
            RankPossibilities otherRankPossibilities = enemyPieces[otherPiece.ID];

            // Get win chance
            float winChance = CalculateWinChance(otherRankPossibilities, myRank);
            if (boardCopy.CurrentSide != side)
                winChance = 1f - winChance; 

            if (winChance >= 0.99f) // Guaranteed to win!
            {
                resultSum += SimulateWin(boardCopy, move, depth);
            }
            else if (winChance <= 0.01f) // Guaranteed to lose!
            {
                // Optimization: don't explore moves where guaranteed to lose!
                return -losingBattlePenalty * 10f;
            }
            else // Simulate both outcomes and multiply each with it's chance
            {
                // Simulated win
                resultSum += SimulateWin(boardCopy, move, depth) * winChance;

                // Simulated loss
                float lossChance = 1f - winChance;
                resultSum += SimulateLoss(boardCopy, move, depth) * lossChance;
            }
        }
        else
        {
            float myResult = SimulateNormal(boardCopy, move, depth);
            resultSum += myResult;
        }

        return resultSum;
    }

    private float SimulateWin(Board boardCopy, in MoveInfo move, int depth)
    {
        Board newBoardCopy = boardCopy.GetCopyWithHiddenPieces(side);

        // First turn
        newBoardCopy.ForceMoveWin(move);
        float myResult = GetHeuristic(newBoardCopy) + winningBattleBonus;

        // Second turn
        newBoardCopy.ForceFlipSides(); // Flip turns
        float otherResult = MonteCarloSearch(newBoardCopy, depth - 1);

        return myResult - otherResult;
    }

    private float SimulateLoss(Board boardCopy, MoveInfo move, int depth)
    {
        Board newBoardCopy = boardCopy.GetCopyWithHiddenPieces(side);

        // First turn
        newBoardCopy.ForceMoveLose(move);
        float myResult = GetHeuristic(newBoardCopy) - losingBattlePenalty;

        // Second turn
        newBoardCopy.ForceFlipSides();
        float otherResult = MonteCarloSearch(newBoardCopy, depth - 1);

        return myResult - otherResult;
    }

    private float SimulateNormal(Board boardCopy, MoveInfo move, int depth)
    {
        Board newBoardCopy = boardCopy.GetCopyWithHiddenPieces(side);

        // First turn
        newBoardCopy.ForceMove(move);
        float myResult = GetHeuristic(boardCopy);

        // Second turn
        newBoardCopy.ForceFlipSides();
        float otherResult = MonteCarloSearch(newBoardCopy, depth - 1);

        return myResult - otherResult;
    }

    private float MonteCarloSearch(Board boardCopy, int depth)
    {
        if (depth < 0)
            return GetHeuristic(boardCopy);

        float resultSum = 0f;
        List<MoveInfo> moves = boardCopy.GetAllValidMoves();

        // Explore counter moves
        for (int j = 0; j < moves.Count; j++)
        {
            resultSum += EvaluateMove(boardCopy, moves[j], depth);
        }

        // return average
        return resultSum / moves.Count;
    }

    private float GetHeuristic(Board boardCopy)
    {
        float score = 0;

        if (boardCopy.CurrentSide == side)
        {
            // Openness - Calculate score for how many moves you can do
            List<MoveInfo> allValidMoves = boardCopy.GetAllValidMoves();
            int allValidMovesCount = allValidMoves.Count;
            score += allValidMovesCount * opennessMultiplier;

            // Check if flag is at risk
            PieceInfo myFlag = myPieces.Flag;
            BoardPosition myFlagPos = myFlag.BoardPosition;
            PieceInfo upPiece = boardCopy.GetPieceFromPosition(myFlagPos.Up);
            PieceInfo downPiece = boardCopy.GetPieceFromPosition(myFlagPos.Down);
            PieceInfo leftPiece = boardCopy.GetPieceFromPosition(myFlagPos.Left);
            PieceInfo rightPiece = boardCopy.GetPieceFromPosition(myFlagPos.Right);
            if (upPiece != null && upPiece.Side != side)
                score -= flagAtRiskMultiplier;
            if (downPiece != null && downPiece.Side != side)
                score -= flagAtRiskMultiplier;
            if (leftPiece != null && leftPiece.Side != side)
                score -= flagAtRiskMultiplier;
            if (rightPiece != null && rightPiece.Side != side)
                score -= flagAtRiskMultiplier;

            // Aggressiveness - Calculate score of each battle
            for (int i = 0; i < allValidMovesCount; i++)
            {
                MoveInfo move = allValidMoves[i];

                // If piece exists in new position, it is an attack
                if (!boardCopy.DoesPieceExistInPosition(move.NewPosition))
                    continue;

                PieceInfo myPiece = boardCopy.GetPieceFromPosition(move.OldPosition);
                PieceInfo otherPiece = boardCopy.GetPieceFromPosition(move.NewPosition);
                PieceRank myRank = myPiece.Rank;
                RankPossibilities otherRankPossibilities = enemyPieces[otherPiece.ID];

                float winChance = CalculateWinChance(otherRankPossibilities, myRank);
                winChance = (winChance * 2f) - 1; // 0..1 to -1..1

                score += winChance * aggressionMultiplier;
            }
        }
        else
        {
            // Openness - Calculate score for how many moves you can do
            List<MoveInfo> allValidMoves = boardCopy.GetAllValidMoves();
            int allValidMovesCount = allValidMoves.Count;
            score += allValidMovesCount * opennessMultiplier;

            // Aggressiveness - Calculate score of each battle
            for (int i = 0; i < allValidMovesCount; i++)
            {
                MoveInfo move = allValidMoves[i];

                // If piece exists in new position, it is an attack
                if (!boardCopy.DoesPieceExistInPosition(move.NewPosition))
                    continue;

                PieceInfo myPiece = boardCopy.GetPieceFromPosition(move.NewPosition);
                PieceInfo otherPiece = boardCopy.GetPieceFromPosition(move.OldPosition);
                PieceRank myRank = myPiece.Rank;
                RankPossibilities otherRankPossibilities = enemyPieces[otherPiece.ID];

                float winChance = CalculateWinChance(otherRankPossibilities, myRank);
                winChance = (winChance * 2f) - 1; // 0..1 to -1..1

                score += winChance * aggressionMultiplier;
            }
        }
        
        return score;
    }

    private float CalculateWinChance(RankPossibilities otherRankPossibilities, PieceRank myRank)
    {
        float wins = 0;
        List<PieceRank> otherPossibleRanks = otherRankPossibilities.PossibleRanks;

        for (var i = 0; i < otherPossibleRanks.Count; i++)
        {
            PieceRank otherRank = otherPossibleRanks[i];
            if (GameRules.GetWinningSide(myRank, otherRank) == Side.A)
                wins++;
        }

        return wins / otherPossibleRanks.Count;
    }
}
