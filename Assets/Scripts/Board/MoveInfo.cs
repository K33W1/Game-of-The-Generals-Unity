public struct MoveInfo
{
    public PieceInfo PieceInfo;
    public BoardPosition OldPosition;
    public BoardPosition NewPosition;

    public MoveInfo(PieceInfo piece, BoardPosition newPosition)
    {
        PieceInfo = piece;
        OldPosition = piece.BoardPosition;
        NewPosition = newPosition;
    }
}
