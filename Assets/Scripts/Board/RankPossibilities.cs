using System.Collections.Generic;

public class RankPossibilities
{
    public int PossibilitiesCount { get; private set; }

    private readonly List<bool> possibilities = null;
    private readonly List<int> piecePool = null;

    public RankPossibilities(List<int> newPiecePool)
    {
        PossibilitiesCount = 15;
        possibilities = new List<bool>(15);
        piecePool = newPiecePool;

        for (int i = 0; i < PossibilitiesCount; i++)
        {
            possibilities.Add(true);
        }
    }

    public float GetConfidence()
    {
        return 1 - (PossibilitiesCount / 15);
    }

    public bool IsPiecePossible(PieceRank rank)
    {
        return possibilities[(int) rank];
    }

    public void WonBattle(PieceRank otherRank)
    {
        if (otherRank == PieceRank.Spy)
        {
            RemovePiecePossibility(PieceRanks.AllRanksWithoutPrivate);
        }
        else
        {
            for (PieceRank thisRank = otherRank; thisRank < PieceRank.Flag; thisRank++)
            {
                RemovePiecePossibility(thisRank);
            }
        }
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
    }

    public void TiedBattle(PieceRank otherRank)
    {
        foreach (PieceRank thisRank in PieceRanks.AllRanks)
        {
            if (thisRank == otherRank)
                continue;

            RemovePiecePossibility(thisRank);
        }
    }

    private void RemovePiecePossibility(PieceRank[] pieceRanks)
    {
        foreach (PieceRank thisRank in pieceRanks)
        {
            RemovePiecePossibility(thisRank);
        }
    }

    private void RemovePiecePossibility(PieceRank rank)
    {
        if (!possibilities[(int)rank])
            return;

        possibilities[(int)rank] = false;
        PossibilitiesCount--;
        piecePool[(int)rank]--;
    }
}
