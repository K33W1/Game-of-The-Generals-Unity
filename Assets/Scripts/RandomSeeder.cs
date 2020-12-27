using UnityEngine;

public class RandomSeeder : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int seed = 0;
    [SerializeField] private bool willInit = false;

    private void Awake()
    {
        if (willInit)
            Random.InitState(seed);
    }
}
