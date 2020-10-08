public struct MoveInfo
{
    public PieceInfo Piece;
    public BoardPosition TargetPosition;

    public MoveInfo(PieceInfo piece, BoardPosition targetPosition)
    {
        Piece = piece;
        TargetPosition = targetPosition;
    }
}
