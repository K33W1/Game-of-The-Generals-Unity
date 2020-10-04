using UnityEngine;

public class ConfirmSpawnsButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput player = null;

    public void ConfirmSpawns()
    {
        player.ConfirmSpawns();
    }
}
