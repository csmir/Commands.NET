using Commands.Core;

namespace Commands.Tests
{
    [method: PrimaryConstructor]
    public class ComplexerType(int? x = 0, int? y = 0, int? z = 0)
    {
        public int? X = x, Y = y, Z = z;
    }
}
