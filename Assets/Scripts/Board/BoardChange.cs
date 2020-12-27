public struct BoardChange
{
    public readonly PieceInfo PieceMoved;
    public readonly PieceInfo PieceAttacked;
    public readonly Side AttackWinningSide;
    public readonly BoardPosition OldPosition;
    public readonly BoardPosition NewPosition;

    public BoardChange(MoveInfo moveInfo, PieceInfo pieceAttacked)
    {
        PieceMoved = moveInfo.PieceInfo;
        PieceAttacked = pieceAttacked;
        AttackWinningSide = Side.None;
        OldPosition = moveInfo.OldPosition;
        NewPosition = moveInfo.NewPosition;
    }

    public BoardChange(MoveInfo moveInfo, PieceInfo pieceAttacked, Side attackWinningSide)
        : this(moveInfo, pieceAttacked)
    {
        AttackWinningSide = attackWinningSide;
    }

    public bool WasThereAnAttack()
    {
        return PieceAttacked != null;
    }

    public PieceInfo GetWinningPiece()
    {
        if (AttackWinningSide == Side.None)
            return null;
        
        return AttackWinningSide == Side.A ? PieceMoved : PieceAttacked;
    }

    public PieceInfo GetPieceFromAction(Side side)
    {
        return PieceMoved.Side == side ? PieceMoved : PieceAttacked;
    }

    public PieceInfo GetPieceFromSide(Side side)
    {
        return PieceMoved.Side == side ? PieceMoved : PieceAttacked;
    }
}
