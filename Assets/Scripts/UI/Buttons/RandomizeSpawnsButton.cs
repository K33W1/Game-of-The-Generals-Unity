using UnityEngine;

public class RandomizeSpawnsButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Actor actor = null;

    public void RandomizeSpawns()
    {
        actor.RandomizeSpawns();
    }
}
