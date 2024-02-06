using Commands.Core;

namespace Commands.Tests
{
    public class ComplexerType
    {
        public int? X, Y, Z;

        [PrimaryConstructor]
        public ComplexerType(int? x = 0, int? y = 0, int? z = 0)
        {
            X = x; Y = y; Z = z;
        }
    }
}
