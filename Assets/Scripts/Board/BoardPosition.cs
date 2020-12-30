using System;

[Serializable]
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
    public BoardPosition UpperLeft => new BoardPosition(x - 1, y + 1);
    public BoardPosition UpperRight => new BoardPosition(x + 1, y + 1);
    public BoardPosition DownLeft => new BoardPosition(x - 1, y - 1);
    public BoardPosition DownRight => new BoardPosition(x + 1, y - 1);

    public bool IsPositionAdjacent(BoardPosition otherPos)
    {
        int xDiff = x - otherPos.x;
        int yDiff = y - otherPos.y;
        return (xDiff * xDiff + yDiff * yDiff) == 1;
    }
}
