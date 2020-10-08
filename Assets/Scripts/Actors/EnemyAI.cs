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
        Board board = gameManager.Board;
        List<MoveInfo> allPossibleMoves = board.GetAllValidMoves(side);
        return allPossibleMoves[Random.Range(0, allPossibleMoves.Count)];
    }
}
