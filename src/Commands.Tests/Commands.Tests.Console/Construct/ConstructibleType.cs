namespace Commands.Tests;

public class ConstructibleType(int x, int y, int z, [Deconstruct] NestedConstructibleType inner)
{
    public int X = x, Y = y, Z = z;

    public NestedConstructibleType Child = inner;
}