public static class GameRules
{
    public static Side GetWinningSide(PieceInfo pieceA, PieceInfo pieceB)
    {
        PieceRank thisRank = pieceA.Rank;
        PieceRank otherRank = pieceB.Rank;

        if (thisRank == PieceRank.Flag && otherRank == PieceRank.Flag) // Flag to Flag
            return Side.A;
        else if (thisRank == PieceRank.Spy && otherRank == PieceRank.Private)
            return Side.B;
        else if (thisRank == PieceRank.Private && otherRank == PieceRank.Spy)
            return Side.A;
        else if (thisRank == otherRank) // Same ranks
            return Side.Invalid;
        else if (thisRank < otherRank)
            return Side.A;
        else
            return Side.B;
    }
}
