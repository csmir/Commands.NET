namespace Commands.Tests;

public class NestedConstructibleType(int? innerX = 0, int? innerY = 0, int? innerZ = 0)
{
    public int? InnerX = innerX, InnerY = innerY, InnerZ = innerZ;
}
