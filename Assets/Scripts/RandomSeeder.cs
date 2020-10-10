using UnityEngine;

public class RandomSeeder : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int seed = 0;

    private void Awake()
    {
        Random.InitState(seed);
    }
}
