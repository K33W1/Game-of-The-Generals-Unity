using UnityEngine;

public class Piece : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private Sprite hiddenSprite = null;

    [Header("Settings")] [SerializeField] private PieceInfo pieceInfo = null;

    public PieceInfo Info => pieceInfo;

    private Collider2D spriteCollider = null;
    private SpriteRenderer spriteRenderer = null;
    private Sprite visibleSprite = null;
    private bool isVisible = true;

    private void Awake()
    {
        spriteCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void BringToFront()
    {
        spriteRenderer.sortingOrder = 10;
    }

    public void BringToMiddle()
    {
        spriteRenderer.sortingOrder = 0;
    }

    public void Show()
    {
        spriteCollider.enabled = true;
        spriteRenderer.enabled = true;
    }

    public void Hide()
    {
        spriteCollider.enabled = false;
        spriteRenderer.enabled = false;
    }

    public void ToggleVisibility()
    {
        isVisible = !isVisible;

        if (isVisible)
        {
            spriteRenderer.sprite = visibleSprite;
        }
        else
        {
            visibleSprite = spriteRenderer.sprite;
            spriteRenderer.sprite = hiddenSprite;
        }
    }
}
