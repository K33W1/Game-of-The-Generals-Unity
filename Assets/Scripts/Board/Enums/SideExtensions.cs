using System.Diagnostics;

public static class SideExtensions
{
    public static Side Flip(this Side side)
    {
        Debug.Assert(side != Side.None);
        return side == Side.A ? Side.B : Side.A;
    }
}
