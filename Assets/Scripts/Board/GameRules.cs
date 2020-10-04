public static class GameRules
{
    public static Piece GetWinningPiece(Piece pieceA, Piece pieceB)
    {
        PieceRank thisRank = pieceA.Properties.Rank;
        PieceRank otherRank = pieceB.Properties.Rank;

        if (thisRank == PieceRank.Flag && otherRank == PieceRank.Flag) // Flag to Flag
            return pieceA;
        else if (thisRank == PieceRank.Spy && otherRank == PieceRank.Private)
            return pieceB;
        else if (thisRank == PieceRank.Private && otherRank == PieceRank.Spy)
            return pieceA;
        else if (thisRank == otherRank) // Same ranks
            return null;
        else if (thisRank < otherRank)
            return pieceA;
        else
            return pieceB;
    }
}
