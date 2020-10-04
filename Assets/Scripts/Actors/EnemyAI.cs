using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class EnemyAI : Actor
{
    [Header("References")]
    [SerializeField] private Board board = null;

    [Header("Settings")]
    [SerializeField] private float thinkTime = 1.0f;

    public override void PerformSpawn()
    {
        // TODO: Enemy spawning
    }

    public override void PerformMove()
    {
        StartCoroutine(PerformTurnCoroutine());
    }

    private IEnumerator PerformTurnCoroutine()
    {
        // TODO: Remove wait timer
        yield return new WaitForSeconds(thinkTime);
        board.TryMove(GetMove());
    }

    private MoveInfo GetMove()
    {
        // TODO: Enemy AI
        List<MoveInfo> allPossibleMoves = board.GetAllValidMoves(this);
        return allPossibleMoves[Random.Range(0, allPossibleMoves.Count)];
    }
}
