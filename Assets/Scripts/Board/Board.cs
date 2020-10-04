﻿using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Grid))]
public class Board : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Actor actorA = null;
    [SerializeField] private Actor actorB = null;
    [SerializeField] private PieceContainer actorAPieces = null;
    [SerializeField] private PieceContainer actorBPieces = null;

    [Header("Settings")]
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 8;

    public int Width => width;
    public int Height => height;

    private Grid grid = null;
    private Piece[,] pieceGrid = null;
    private GamePhase currentGamePhase = GamePhase.SpawnA;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        pieceGrid = new Piece[width, height];
    }

    private void Start()
    {
        actorA.PerformSpawn();
    }

    public void ConfirmSpawn()
    {
        if (currentGamePhase == GamePhase.SpawnA)
        {
            if (actorAPieces.IsValidSpawn())
            {
                currentGamePhase = GamePhase.SpawnB;
                actorB.PerformSpawn();
            }
            else
            {
                Debug.Log("Invalid spawn state! " + "Game State: " + currentGamePhase.ToString());
                actorA.PerformSpawn();
            }
        }
        else if (currentGamePhase == GamePhase.SpawnB)
        {
            if (actorBPieces.IsValidSpawn())
            {
                currentGamePhase = GamePhase.MoveA;
                actorA.PerformMove();
            }
            else
            {
                Debug.Log("Invalid spawn state! " + "Game State: " + currentGamePhase.ToString());
                actorB.PerformSpawn();
            }
        }
    }

    public void SpawnPiece(MoveInfo move)
    {
        BoardPosition pos = move.TargetPosition;

        if (currentGamePhase == GamePhase.SpawnA)
        {
            if (pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y <= 2)
            {
                PlacePiece(move);
                actorAPieces.ActivatePiece(move.Piece);
            }

            actorA.PerformSpawn();
        }
        else if (currentGamePhase == GamePhase.SpawnB)
        {
            if (pos.x >= 0 && pos.x < Width && pos.y >= 5 && pos.y <= 7)
            {
                PlacePiece(move);
                actorBPieces.ActivatePiece(move.Piece);
            }

            actorB.PerformSpawn();
        }
        else
        {
            Debug.LogError("Tried to spawn a piece in wrong game phase!");
        }
    }

    public void PlacePiece(MoveInfo move)
    {
        UpdatePieceWorldPosition(move);
        UpdateGridArray(move);
    }

    public bool TryMove(MoveInfo move)
    {
        Piece thisPiece = move.Piece;
        BoardPosition nextPos = move.TargetPosition;

        if (!IsValidMove(move))
            return false;

        Piece otherPiece = pieceGrid[nextPos.x, nextPos.y];
        
        if (otherPiece != null)
        {
            if (otherPiece.Properties.IsPlayerPiece == thisPiece.Properties.IsPlayerPiece)
                return false;

            MovePieceOnPiece(thisPiece, otherPiece);
        }

        PlacePiece(move);

        return true;
    }

    private void MovePieceOnPiece(Piece pieceA, Piece pieceB)
    {
        Piece winningPiece = GameRules.GetWinningPiece(pieceA, pieceB);
        // TODO: Battle animation
        // TODO: Remove losing piece
    }

    private void UpdatePieceWorldPosition(MoveInfo move)
    {
        if (!move.Piece.gameObject.activeInHierarchy)
            return;

        move.Piece.transform.position = GetCellToWorld(move.TargetPosition);
    }

    private void UpdateGridArray(MoveInfo move)
    {
        move.Piece.BoardPosition = move.TargetPosition;
    }

    public List<MoveInfo> GetAllValidMoves(Actor actor)
    {
        List <MoveInfo> allPossibleMoves = new List<MoveInfo>();
        PieceContainer pieces = actor == actorA ? actorAPieces : actorBPieces;

        foreach (Piece piece in pieces.ActivePieces)
            foreach (MoveInfo move in GetPieceValidMoves(piece))
                allPossibleMoves.Add(move);

        return allPossibleMoves;
    }

    #region Helpers
    public Vector3 GetCellToWorld(BoardPosition pos)
    {
        return grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
    }

    public BoardPosition GetWorldToCell(Vector3 worldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        return new BoardPosition(cellPos.x, cellPos.y);
    }

    public List<MoveInfo> GetPieceValidMoves(Piece piece)
    {
        List<MoveInfo> moves = new List<MoveInfo>();

        BoardPosition origin = piece.BoardPosition;
        MoveInfo moveUp = new MoveInfo(piece, origin.Up);
        MoveInfo moveDown = new MoveInfo(piece, origin.Down);
        MoveInfo moveLeft = new MoveInfo(piece, origin.Left);
        MoveInfo moveRight = new MoveInfo(piece, origin.Right);

        if (IsValidMove(moveUp)) moves.Add(moveUp);
        if (IsValidMove(moveDown)) moves.Add(moveDown);
        if (IsValidMove(moveLeft)) moves.Add(moveLeft);
        if (IsValidMove(moveRight)) moves.Add(moveRight);

        return moves;
    }

    public bool IsPositionInsideGrid(BoardPosition pos)
    {
        return !(pos.x < 0 || pos.y < 0 || pos.x >= Width || pos.y >= Height);
    }

    public bool IsValidMove(MoveInfo move)
    {
        Piece piece = move.Piece;
        BoardPosition targetPos = move.TargetPosition;

        if (!IsPositionInsideGrid(targetPos))
            return false;

        if (!piece.BoardPosition.IsPositionAdjacent(targetPos))
            return false;

        Piece otherPiece = pieceGrid[targetPos.x, targetPos.y];

        if (otherPiece == null)
            return true;

        if (otherPiece.Properties.IsPlayerPiece != piece.Properties.IsPlayerPiece)
            return true;

        return false;
    }
    #endregion
}
