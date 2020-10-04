public struct BoardPosition
{
    public int x;
    public int y;

    public BoardPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public BoardPosition Up => new BoardPosition(x, y + 1);
    public BoardPosition Down => new BoardPosition(x, y - 1);
    public BoardPosition Left => new BoardPosition(x - 1, y);
    public BoardPosition Right => new BoardPosition(x + 1, y);

    public bool IsPositionAdjacent(BoardPosition otherPos)
    {
        return (x * otherPos.x + y * otherPos.y) == 1;
    }
}
