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
        return AttackWinningSide == Side.None;
    }

    public PieceInfo GetWinningPiece()
    {
        return AttackWinningSide == Side.A ? PieceMoved : PieceAttacked;
    }
}
