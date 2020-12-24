public enum PieceRank
{
    Invalid = -1,
    Spy,
    General5,
    General4,
    General3,
    General2,
    General1,
    Colonel,
    LtColonel,
    Major,
    Captain,
    Lieutenant1,
    Lieutenant2,
    Sergeant,
    Private,
    Flag
}

public static class PieceRanks
{
    public static readonly PieceRank[] AllRanks =
    {
        PieceRank.Spy,
        PieceRank.General5,
        PieceRank.General4,
        PieceRank.General3,
        PieceRank.General2,
        PieceRank.General1,
        PieceRank.Colonel,
        PieceRank.LtColonel,
        PieceRank.Major,
        PieceRank.Captain,
        PieceRank.Lieutenant1,
        PieceRank.Lieutenant2,
        PieceRank.Sergeant,
        PieceRank.Private,
        PieceRank.Flag
    };

    public static readonly PieceRank[] AllRanksWithoutPrivate =
    {
        PieceRank.Spy,
        PieceRank.General5,
        PieceRank.General4,
        PieceRank.General3,
        PieceRank.General2,
        PieceRank.General1,
        PieceRank.Colonel,
        PieceRank.LtColonel,
        PieceRank.Major,
        PieceRank.Captain,
        PieceRank.Lieutenant1,
        PieceRank.Lieutenant2,
        PieceRank.Sergeant,
        PieceRank.Flag
    };

    public static readonly PieceRank[] AllRanksWithoutSpy =
    {
        PieceRank.General5,
        PieceRank.General4,
        PieceRank.General3,
        PieceRank.General2,
        PieceRank.General1,
        PieceRank.Colonel,
        PieceRank.LtColonel,
        PieceRank.Major,
        PieceRank.Captain,
        PieceRank.Lieutenant1,
        PieceRank.Lieutenant2,
        PieceRank.Sergeant,
        PieceRank.Private,
        PieceRank.Flag
    };
}