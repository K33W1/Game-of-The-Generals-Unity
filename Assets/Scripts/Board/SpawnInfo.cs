public struct SpawnInfo
{
    public PieceInfo PieceInfo;
    public BoardPosition NewPosition;

    public SpawnInfo(PieceInfo piece, BoardPosition newPosition)
    {
        PieceInfo = piece;
        NewPosition = newPosition;
    }
}