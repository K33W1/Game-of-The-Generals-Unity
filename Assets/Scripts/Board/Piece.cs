using System;
using UnityEngine;

[Serializable]
public class Piece : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Sprite hiddenSprite = null;

    [Header("Settings")]
    [SerializeField] private PieceProps pieceProps = new PieceProps();

    public PieceProps Properties => pieceProps;
    public BoardPosition BoardPosition { get; set; } = new BoardPosition(-1, -1);

    private SpriteRenderer spriteRenderer = null;
    private Sprite visibleSprite = null;
    private bool isVisible = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        visibleSprite = spriteRenderer.sprite;
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
