namespace Commands.Tests;

public class ConstructibleInnerType(int? xx = 0, int? yy = 0, int? zz = 0)
{
    public int? X = xx, Y = yy, Z = zz;
}
