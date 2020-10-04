using UnityEngine;

public class ToggleVisiblePiecesUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PieceContainer enemyPieces = null;

    public void ToggleVisibility()
    {
        enemyPieces.ToggleVisibility();
    }
}
