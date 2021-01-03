using System.Collections.Generic;

public class RankPossibilities
{
    public List<PieceRank> PossibleRanks { get; }
    public PieceRank GuaranteedRank = PieceRank.Invalid;

    private readonly List<bool> possibilities = null;

    public RankPossibilities()
    {
        possibilities = new List<bool>(15);
        PossibleRanks = new List<PieceRank>(15);

        for (int i = 0; i < 15; i++)
        {
            possibilities.Add(true);
            PossibleRanks.Add((PieceRank)i);
        }
    }

    public float GetConfidence()
    {
        return 1 - (PossibleRanks.Count / 15);
    }

    public bool IsPiecePossible(PieceRank rank)
    {
        return possibilities[(int)rank];
    }

    public void WonBattle(PieceRank otherRank)
    {
        if (otherRank == PieceRank.Spy)
        {
            RemovePiecePossibility(PieceRanks.AllRanksWithoutPrivate);
        }
        else if (otherRank == PieceRank.Private)
        {
            RemovePiecePossibility(PieceRank.Spy);
            RemovePiecePossibility(PieceRank.Private);
            RemovePiecePossibility(PieceRank.Flag);
        }
        else
        {
            for (PieceRank thisRank = otherRank; thisRank <= PieceRank.Flag; thisRank++)
            {
                RemovePiecePossibility(thisRank);
            }
        }

        DiscoverRank();
    }

    public void LostBattle(PieceRank otherRank)
    {
        if (otherRank == PieceRank.Spy)
        {
            RemovePiecePossibility(PieceRank.Spy);
            RemovePiecePossibility(PieceRank.Private);
        }
        else if (otherRank == PieceRank.Private)
        {
            RemovePiecePossibility(PieceRanks.AllRanksWithoutSpy);
        }
        else
        {
            for (PieceRank thisRank = otherRank; thisRank >= 0; thisRank--)
            {
                RemovePiecePossibility(thisRank);
            }
        }

        DiscoverRank();
    }

    public void TiedBattle(PieceRank otherRank)
    {
        foreach (PieceRank thisRank in PieceRanks.AllRanks)
        {
            if (thisRank == otherRank)
                continue;

            RemovePiecePossibility(thisRank);
        }

        DiscoverRank();
    }

    public void RemovePiecePossibility(PieceRank[] pieceRanks)
    {
        foreach (PieceRank thisRank in pieceRanks)
        {
            RemovePiecePossibility(thisRank);
        }
    }

    public void RemovePiecePossibility(PieceRank rank)
    {
        if (!possibilities[(int)rank])
            return;

        possibilities[(int)rank] = false;
        PossibleRanks.Remove(rank);
    }


    private void DiscoverRank()
    {
        if (PossibleRanks.Count == 1)
        {
            GuaranteedRank = PossibleRanks[0];
        }
    }

}
