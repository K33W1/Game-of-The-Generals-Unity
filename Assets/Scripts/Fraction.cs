public struct Fraction
{
    public int Numerator;
    public int Denominator;

    public Fraction(int numerator, int denominator)
    {
        Numerator = numerator;
        Denominator = denominator;
    }

    public float GetDecimal()
    {
        return (float)Numerator / Denominator;
    }
}
