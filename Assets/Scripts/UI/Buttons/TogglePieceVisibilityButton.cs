using UnityEngine;

public class TogglePieceVisibilityButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Actor enemyPieces = null;

    public void ToggleVisibility()
    {
        enemyPieces.TogglePieceVisibility();
    }
}
