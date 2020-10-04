public struct MoveInfo
{
    public Piece Piece;
    public BoardPosition TargetPosition;

    public MoveInfo(Piece piece, BoardPosition targetPosition)
    {
        Piece = piece;
        TargetPosition = targetPosition;
    }
}
