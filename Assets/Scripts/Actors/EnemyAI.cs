using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyAI : Actor
{
    [SerializeField] private PieceCounterPanel pieceCounterPanel = null;
    [SerializeField] private bool printBestScore = false;
    [SerializeField, Min(0)] float flagAtRiskMultiplier = 1000f;
    [SerializeField, Min(0)] float opennessMultiplier = 2f;
    [SerializeField, Min(0)] float aggressionMultiplier = 2f;
    [SerializeField, Min(0)] float winningBattleBonus = 100f;
    [SerializeField, Min(0)] float losingBattlePenalty = 50f;

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

    private MoveInfo GetMove()
    {
        var allPossibleMoves = board.GetAllValidMoves();
        float bestResult = -1000000f;
        int bestResultIndex = 0;

        // Explore each move
        for (int i = 0; i < allPossibleMoves.Count; i++)
        {
            // Get the current move
            MoveInfo curMove = allPossibleMoves[i];
            float finalResult = 0f;

            // If piece exists in next position, this is an attacking move
            if (board.DoesPieceExistInPosition(curMove.NewPosition))
            {
                // Get needed vars
                PieceInfo myPiece = board.GetPieceFromPosition(curMove.OldPosition);
                PieceInfo otherPiece = board.GetPieceFromPosition(curMove.NewPosition);
                PieceRank myRank = myPiece.Rank;
                RankPossibilities otherRankPossibilities = enemyPieces[otherPiece.ID];

                // Get win chance
                float winChance = CalculateWinChance(otherRankPossibilities, myRank);

                if (winChance >= 0.99f) // Guaranteed to win!
                {
                    Board boardCopy = board.GetCopyWithHiddenPieces(side);
                    boardCopy.ForceMoveWin(curMove);

                    // Calculate heuristic of first move
                    float myWinResult = GetMyHeuristic(boardCopy) + winningBattleBonus;
                    float otherWinResult = PerformCounterMove(boardCopy);
                    float simulatedWinResult = myWinResult - otherWinResult;
                    finalResult += simulatedWinResult;
                }
                else if (winChance <= 0.01f) // Guaranteed to lose!
                {
                    // Optimization: don't explore moves where guaranteed to lose!
                    continue;
                }
                else
                {
                    // Simulate both outcomes and multiply each with it's chance
                    // Simulated win
                    Board boardCopy1 = board.GetCopyWithHiddenPieces(side);
                    boardCopy1.ForceMoveWin(curMove);
                    float myWinResult = GetMyHeuristic(boardCopy1) + winningBattleBonus;
                    float otherWinResult = PerformCounterMove(boardCopy1);
                    float simulatedWinResult = (myWinResult - otherWinResult) * winChance;
                    finalResult += simulatedWinResult;

                    // Simulated loss
                    Board boardCopy2 = board.GetCopyWithHiddenPieces(side);
                    boardCopy2.ForceMoveLose(curMove);
                    float myLossResult = GetMyHeuristic(boardCopy2) - losingBattlePenalty;
                    float otherLossResult = PerformCounterMove(boardCopy2);
                    float simulatedLossResult = (myLossResult - otherLossResult) * (1f - winChance);
                    finalResult += simulatedLossResult;
                }
            }
            else
            {
                Board boardCopy = board.GetCopyWithHiddenPieces(side);
                boardCopy.ForceMove(curMove);
                finalResult += GetMyHeuristic(boardCopy);
                finalResult += PerformCounterMove(boardCopy);
            }

            if (finalResult > bestResult)
            {
                bestResult = finalResult;
                bestResultIndex = i;
            }
        }

        // DEBUG
        if (printBestScore)
            Debug.Log("Best result: " + bestResult);

        return allPossibleMoves[bestResultIndex];
    }

    private float PerformCounterMove(Board boardCopy)
    {
        // Other player's turn
        boardCopy.ForceFlipSides();

        // Explore counter moves
        float result2Sum = 0f;
        List<MoveInfo> counterMoves = boardCopy.GetAllValidMoves();
        for (int j = 0; j < counterMoves.Count; j++)
        {
            MoveInfo otherMove = counterMoves[j];

            // If piece exists in next position, this is an attacking move
            if (boardCopy.DoesPieceExistInPosition(otherMove.NewPosition))
            {
                PieceInfo myPiece = boardCopy.GetPieceFromPosition(otherMove.NewPosition);
                PieceInfo otherPiece = boardCopy.GetPieceFromPosition(otherMove.OldPosition);
                PieceRank myRank = myPiece.Rank;
                RankPossibilities otherRankPossibilities = enemyPieces[otherPiece.ID];

                // Get win chance
                float winChance = 1f - CalculateWinChance(otherRankPossibilities, myRank);

                if (winChance >= 0.99f) // Guaranteed to win!
                {
                    Board boardCopy2 = boardCopy.GetCopyWithHiddenPieces(side);
                    boardCopy2.ForceMoveWin(otherMove);
                    result2Sum += winningBattleBonus;
                    result2Sum += GetOtherHeuristic(boardCopy2);
                }
                else if (winChance <= 0.01f) // Guaranteed to lose!
                {
                    Board boardCopy2 = boardCopy.GetCopyWithHiddenPieces(side);
                    boardCopy2.ForceMoveLose(otherMove);
                    result2Sum -= losingBattlePenalty;
                    result2Sum += GetOtherHeuristic(boardCopy2);
                }
                else
                {
                    // Simulated win
                    Board boardCopyWin = boardCopy.GetCopyWithHiddenPieces(side);
                    boardCopyWin.ForceMoveWin(otherMove);
                    result2Sum += winningBattleBonus * winChance;
                    result2Sum += GetOtherHeuristic(boardCopyWin) * winChance;

                    // Simulated loss
                    Board boardCopyLoss = boardCopy.GetCopyWithHiddenPieces(side);
                    boardCopyLoss.ForceMoveLose(otherMove);
                    result2Sum -= losingBattlePenalty * (1f - winChance);
                    result2Sum += GetOtherHeuristic(boardCopyLoss) * (1f - winChance);
                }
            }
            else
            {
                Board boardCopy2 = boardCopy.GetCopyWithHiddenPieces(side);
                boardCopy2.ForceMove(otherMove);
                result2Sum += GetOtherHeuristic(boardCopy2);
            }
        }

        // return average
        return result2Sum / counterMoves.Count;
    }

    private float GetMyHeuristic(Board boardCopy)
    {
        float score = 0;

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

        return score;
    }

    private float GetOtherHeuristic(Board boardCopy)
    {
        float score = 0;

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
