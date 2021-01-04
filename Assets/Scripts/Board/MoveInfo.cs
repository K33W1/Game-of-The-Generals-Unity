public struct MoveInfo
{
    public BoardPosition OldPosition;
    public BoardPosition NewPosition;

    public MoveInfo(PieceInfo piece, BoardPosition newPosition)
    {
        OldPosition = piece.BoardPosition;
        NewPosition = newPosition;
    }

    public BoardPosition GetDifference()
    {
        return new BoardPosition(
            NewPosition.x - OldPosition.x, 
            NewPosition.y - OldPosition.y);
    }
}
