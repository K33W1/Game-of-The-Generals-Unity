using UnityEngine;

public class TogglePieceVisibilityButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PieceContainer enemyPieces = null;

    public void ToggleVisibility()
    {
        enemyPieces.ToggleVisibility();
    }
}
