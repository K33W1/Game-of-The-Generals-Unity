public static class GameRules
{
    public static Side GetWinningSide(PieceRank rankA, PieceRank rankB)
    {
        if (rankA == PieceRank.Flag && rankB == PieceRank.Flag) // Flag to Flag
            return Side.A;
        if (rankA == PieceRank.Spy && rankB == PieceRank.Private)
            return Side.B;
        if (rankA == PieceRank.Private && rankB == PieceRank.Spy)
            return Side.A;
        if (rankA == rankB) // Same ranks
            return Side.None;
        if (rankA < rankB)
            return Side.A;

        return Side.B;
    }
}
