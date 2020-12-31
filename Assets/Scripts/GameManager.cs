using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Grid))]
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Graveyard graveyardA = null;
    [SerializeField] private Graveyard graveyardB = null;
    [SerializeField] private Transform leftPieceTransform = null;
    [SerializeField] private Transform midPieceTransform = null;
    [SerializeField] private Transform rightPieceTransform = null;
    [SerializeField] private Transform leftPieceDeathTransform = null;
    [SerializeField] private Transform rightPieceDeathTransform = null;
    [SerializeField] private SpriteRenderer blackSprite = null;
    [SerializeField] private Actor actorA = null;
    [SerializeField] private Actor actorB = null;

    [Header("Animation Settings")]
    [SerializeField] float start = 1.0f;
    [SerializeField] float wait1 = 1.0f;
    [SerializeField] float death = 0.5f;
    [SerializeField] float wait2 = 0.5f;
    [SerializeField] float end = 1.0f;

    public event Action<GamePhase> GamePhaseChanged;
    public event Action<Side> GameEnded;

    private Board Board = null;
    private Grid grid = null;

    private Dictionary<PieceInfo, Piece> pieceInfoMap = new Dictionary<PieceInfo, Piece>();

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Start()
    {
        // PieceContainer dependencies
        List<PieceInfo> piecesA = new List<PieceInfo>(PieceContainer.MAX_CAPACITY);
        List<PieceInfo> piecesB = new List<PieceInfo>(PieceContainer.MAX_CAPACITY);

        // Get the piece gameobjects
        foreach (Piece piece in FindObjectsOfType<Piece>())
        {
            pieceInfoMap.Add(piece.Info, piece);

            if (piece.Info.Side == Side.A)
            {
                piecesA.Add(piece.Info);
            }
            else if (piece.Info.Side == Side.B)
            {
                piecesB.Add(piece.Info);
            }
            else
            {
                Debug.LogError("Piece has no side!");
                return;
            }
        }

        // Board dependencies
        PieceInfo[,] pieceGrid = new PieceInfo[Board.WIDTH, Board.HEIGHT];
        PieceContainer pieceContainerA = new PieceContainer(piecesA);
        PieceContainer pieceContainerB = new PieceContainer(piecesB);
        GamePhase startingGamePhase = GamePhase.Spawn;
        Side startingGameOutput = Side.None;
        Side startingSide = Side.A;

        // Initialize board
        Board = new Board(
            pieceGrid,
            pieceContainerA,
            pieceContainerB,
            null,
            startingGamePhase,
            startingGameOutput,
            startingSide);

        // Initialize actors
        actorA.Initialize(this, Board, pieceContainerA, pieceContainerB);
        actorB.Initialize(this, Board, pieceContainerB, pieceContainerA);

        // TODO: Configure which can spawn first
        actorA.PerformSpawn();
    }

    public void SpawnPiece(in MoveInfo move)
    {
        if (Board.SpawnPiece(move))
            UpdatePieceWorldPosition(pieceInfoMap[move.PieceInfo]);

        if (Board.CurrentSide == Side.A)
            actorA.PerformSpawn();
        else
            actorB.PerformSpawn();
    }

    public void ConfirmSpawn()
    {
        if (Board.ConfirmSpawn())
        {
            if (Board.CurrentSide == Side.B)
            {
                actorB.PerformSpawn();
            }
            else
            {
                // Actor A first
                actorA.PerformMove();

                GamePhaseChanged.Invoke(GamePhase.Move);
            }
        }
        else
        {
            Debug.Log("Could not confirm spawn!");
        }
    }

    public void MovePiece(in MoveInfo move)
    {
        StartCoroutine(MovePieceCoroutine(move));
    }

    private IEnumerator MovePieceCoroutine(MoveInfo move)
    {
        if (Board.MovePiece(move))
        {
            BoardChange boardChange = Board.BoardChange.Value;
            
            if (boardChange.WasThereAnAttack())
            {
                PieceInfo pieceInfoA = boardChange.GetPieceFromSide(Side.A);
                PieceInfo pieceInfoB = boardChange.GetPieceFromSide(Side.B);
                Piece pieceA = pieceInfoMap[pieceInfoA];
                Piece pieceB = pieceInfoMap[pieceInfoB];
                Transform pieceATransform = pieceA.transform;
                Transform pieceBTransform = pieceB.transform;

                Vector3 originalPiecePosA = pieceA.transform.position;
                Vector3 originalPiecePosB = pieceB.transform.position;

                pieceA.BringToFront();
                pieceB.BringToFront();

                Sequence sequence = DOTween.Sequence();
                sequence.Append(pieceATransform.DOMove(leftPieceTransform.position, start))
                    .Join(pieceBTransform.DOMove(rightPieceTransform.position, start))
                    .Join(pieceATransform.DOScale(new Vector3(4f, 4f, 4f), start))
                    .Join(pieceBTransform.DOScale(new Vector3(4f, 4f, 4f), start))
                    .Join(blackSprite.DOColor(Color.white, start))
                    .AppendInterval(wait1);

                if (boardChange.GetWinningPiece() == pieceInfoA)
                {
                    sequence.Append(pieceBTransform.DOMove(rightPieceDeathTransform.position, death))
                        .Join(pieceATransform.DOMove(midPieceTransform.position, death));
                }
                else if (boardChange.GetWinningPiece() == pieceInfoB)
                {
                    sequence.Append(pieceATransform.DOMove(leftPieceDeathTransform.position, death))
                        .Join(pieceBTransform.DOMove(midPieceTransform.position, death));
                }
                else
                {
                    sequence.Append(pieceATransform.DOMove(leftPieceDeathTransform.position, death))
                        .Join(pieceBTransform.DOMove(rightPieceDeathTransform.position, death));
                }

                sequence.AppendInterval(wait2);

                if (boardChange.GetWinningPiece() != null)
                {
                    sequence.Append(boardChange.GetWinningPiece() == pieceInfoA
                        ? pieceATransform.DOMove(originalPiecePosA, end)
                        : pieceBTransform.DOMove(originalPiecePosB, end))
                        .Join(pieceATransform.DOScale(Vector3.one, start))
                        .Join(pieceBTransform.DOScale(Vector3.one, start))
                        .Join(blackSprite.DOColor(Color.clear, start));
                }
                else
                {
                    sequence.Append(blackSprite.DOColor(Color.clear, start));
                }
                
                sequence.Play();

                yield return sequence.WaitForCompletion();

                pieceATransform.localScale = Vector3.one;
                pieceBTransform.localScale = Vector3.one;

                if (!pieceInfoA.IsAlive)
                    graveyardA.AddPiece(pieceInfoMap[pieceInfoA]);
                if (!pieceInfoB.IsAlive)
                    graveyardB.AddPiece(pieceInfoMap[pieceInfoB]);

                pieceA.BringToMiddle();
                pieceB.BringToMiddle();
            }

            UpdatePieceWorldPosition(pieceInfoMap[move.PieceInfo]);

            if (Board.CurrentGameOutput != Side.None)
                EndGame();
            else if (Board.CurrentSide == Side.A)
                actorA.PerformMove();
            else if (Board.CurrentSide == Side.B)
                actorB.PerformMove();
            else
                Debug.LogError("Board's current side is invalid!");
        }
        else
        {
            // Try again
            if (Board.CurrentSide == Side.A)
                actorA.PerformMove();
            else if (Board.CurrentSide == Side.B)
                actorB.PerformMove();
            else
                Debug.LogError("Board's current side is invalid!");
        }
    }

    private void EndGame()
    {
        GamePhaseChanged.Invoke(GamePhase.End);
        GameEnded.Invoke(Board.CurrentGameOutput);
    }

    private void UpdatePieceWorldPosition(Piece piece)
    {
        if (!piece.Info.IsAlive)
            return;

        piece.transform.position = GetCellToWorld(piece.Info.BoardPosition);
    }

    #region Helpers
    public BoardPosition GetWorldToCell(in Vector3 worldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        return new BoardPosition(cellPos.x, cellPos.y);
    }

    private Vector3 GetCellToWorld(in BoardPosition pos)
    {
        return grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
    }
    #endregion
}
