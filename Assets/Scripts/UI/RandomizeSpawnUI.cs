using UnityEngine;

public class RandomizeSpawnUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board board = null;

    public void RandomizeSpawn()
    {
        board.RandomizeSpawn();
    }
}
