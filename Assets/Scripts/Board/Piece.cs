using UnityEngine;

public class Piece : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private Sprite hiddenSprite = null;

    [Header("Settings")] [SerializeField] private PieceInfo pieceInfo = null;

    public PieceInfo Info => pieceInfo;

    private new Collider2D collider = null;
    private SpriteRenderer spriteRenderer = null;
    private Sprite visibleSprite = null;
    private bool isVisible = true;

    private void Awake()
    {
        collider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        visibleSprite = spriteRenderer.sprite;
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
        collider.enabled = true;
        spriteRenderer.enabled = true;
    }

    public void Hide()
    {
        collider.enabled = false;
        spriteRenderer.enabled = false;
    }

    public void ToggleVisibility()
    {
        isVisible = !isVisible;

        if (isVisible)
            spriteRenderer.sprite = visibleSprite;
        else
            spriteRenderer.sprite = hiddenSprite;
    }
}
