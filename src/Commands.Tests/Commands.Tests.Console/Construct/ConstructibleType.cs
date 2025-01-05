namespace Commands.Tests;

public class ConstructibleType(int x, int y, int z, [Deconstruct] ConstructibleType.NestedConstructibleType inner)
{
    public int X = x, Y = y, Z = z;

    public NestedConstructibleType Child = inner;

    public class NestedConstructibleType(int? innerX = 0, int? innerY = 0, int? innerZ = 0)
    {
        public int? InnerX = innerX, InnerY = innerY, InnerZ = innerZ;
    }
}

