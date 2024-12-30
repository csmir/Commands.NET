namespace Commands.Tests
{
    [Name("command")]
    public class Module : CommandModule
    {
        [Name("nested")]
        public class NestedModule : CommandModule
        {
            [Name("complex")]
            public void Complex([Complex] ComplexType complex)
            {
                Respond($"({complex.X}, {complex.Y}, {complex.Z}) {complex.Complexer}: {complex.Complexer.X}, {complex.Complexer.Y}, {complex.Complexer.Z}");
            }

            [Name("complexnullable")]
            public void Complex([Complex] ComplexerType? complex)
            {
                Respond($"({complex?.X}, {complex?.Y}, {complex?.Z})");
            }

            [Name("multiple")]
            public void Test(bool truee, bool falsee)
            {
                Respond($"Success: {truee}, {falsee}");
            }

            [Name("multiple")]
            public void Test(int i1, int i2)
            {
                Respond($"Success: {i1}, {i2}");
            }

            [Name("optional")]
            public void Test(int i = 0, string str = "")
            {
                Respond($"Success: {i}, {str}");
            }

            [Name("nullable")]
            public void Nullable(long? l)
            {
                Respond($"Success: {l}");
            }
        }

        [Name("remainder")]
        public void Remainder([Remainder] string values)
        {
            Respond($"Success: {values}");
        }

        [Name("multiple")]
        public void Test(bool truee, bool falsee)
        {
            Respond($"Success: {truee}, {falsee}");
        }

        [Name("multiple")]
        public void Test(int i1, int i2)
        {
            Respond($"Success: {i1}, {i2}");
        }

        [Name("optional")]
        public void Test(int i = 0, string str = "")
        {
            Respond($"Success: {i}, {str}");
        }

        [Name("nullable")]
        public void Nullable(long? l)
        {
            Respond($"Success: {l}");
        }

        [Name("complex")]
        public void Complex([Complex] ComplexType complex)
        {
            Respond($"({complex.X}, {complex.Y}, {complex.Z}) {complex.Complexer}: {complex.Complexer.X}, {complex.Complexer.Y}, {complex.Complexer.Z}");
        }

        [Name("complexnullable")]
        public void Complex([Complex] ComplexerType? complex)
        {
            Respond($"({complex?.X}, {complex?.Y}, {complex?.Z})");
        }
    }
}
