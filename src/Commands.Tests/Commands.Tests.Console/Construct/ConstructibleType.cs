namespace Commands.Tests;

public class ConstructibleType(int x, int y, int z, [Deconstruct] ConstructibleInnerType complexer)
{
    public int X = x, Y = y, Z = z;

    public ConstructibleInnerType Complexer = complexer;
}
