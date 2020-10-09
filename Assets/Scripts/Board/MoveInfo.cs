public struct MoveInfo
{
    public PieceInfo PieceInfo;
    public BoardPosition NewPosition;

    public MoveInfo(PieceInfo piece, BoardPosition newPosition)
    {
        PieceInfo = piece;
        NewPosition = newPosition;
    }
}
