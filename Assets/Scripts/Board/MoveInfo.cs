public struct MoveInfo
{
    public BoardPosition OldPosition;
    public BoardPosition NewPosition;

    public MoveInfo(PieceInfo piece, BoardPosition newPosition)
    {
        OldPosition = piece.BoardPosition;
        NewPosition = newPosition;
    }
}
