using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class EnemyAI : Actor
{
    private bool isThinking = false;

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
        gameManager.MovePiece(GetMove());
    }

    private MoveInfo GetMove()
    {
        // Debug
        float startingTime = Time.realtimeSinceStartup;

        Board board = gameManager.Board;

        List<MoveInfo> allPossibleMoves = board.GetAllValidMoves();
        List<Fraction> results = new List<Fraction>(allPossibleMoves.Count);

        // Add 0/0 fractional results
        for (int i = 0; i < allPossibleMoves.Count; i++)
            results.Add(new Fraction(0, 0));

        // Explore each move
        for (int i = 0; i < allPossibleMoves.Count; i++)
        {
            Fraction result = results[i];

            // Explore this move a set number of times
            for (int j = 0; j < 100; j++)
            {
                Board boardCopy = board.GetCopyWithHiddenPieces(side);

                // Randomize hidden pieces
                List<PieceRank> ranks = new List<PieceRank>()
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
                    boardCopy.PiecesA[k].Rank = ranks[k];

                boardCopy.MovePiece(boardCopy.GetAllValidMoves()[i]);

                // Play random moves until end game
                while(boardCopy.CurrentGameOutput == GameOutput.None)
                {
                    List<MoveInfo> currentValidMoves = boardCopy.GetAllValidMoves();
                    boardCopy.MovePiece(currentValidMoves[Random.Range(0, currentValidMoves.Count)]);
                }

                // Update result
                // TODO: GameOutput and Side is just the same!
                if (boardCopy.CurrentGameOutput == GameOutput.B)
                    result.Numerator++;
                result.Denominator++;
            }
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

        // Debug
        Debug.Log("AI took " + (Time.realtimeSinceStartup - startingTime).ToString());

        return allPossibleMoves[bestMoveIndex];
    }
}
