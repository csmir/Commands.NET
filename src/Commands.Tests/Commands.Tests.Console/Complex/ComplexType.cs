using Commands.Core;

namespace Commands.Tests
{
    public class ComplexType(int x, int y, int z, [Complex] ComplexerType complexer)
    {
        public int X = x, Y = y, Z = z;

        public ComplexerType Complexer = complexer;
    }
}
