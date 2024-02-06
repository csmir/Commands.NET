using Commands.Core;

namespace Commands.Tests
{
    public class ComplexType
    {
        public int X, Y, Z;

        public ComplexerType Complexer;

        [PrimaryConstructor]
        public ComplexType(int x, int y, int z, [Complex] ComplexerType complexer)
        {
            X = x; Y = y; Z = z;
            Complexer = complexer;
        }
    }
}
