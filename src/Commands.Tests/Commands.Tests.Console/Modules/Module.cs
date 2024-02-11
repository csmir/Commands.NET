using Commands.Core;

namespace Commands.Tests
{
    [Name("command")]
    public class Module : ModuleBase<ConsumerBase>
    {
        [Name("priority")]
        [Priority(1)]
        public void Priority1(bool optional)
        {
            Console.WriteLine($"Success: {Command.Priority} {optional}");
        }

        [Name("priority")]
        [Priority(2)]
        public Task Priority2(bool optional)
        {
            Console.WriteLine($"Success: {Command.Priority} {optional}");
            return Task.CompletedTask;
        }

        [Name("remainder")]
        public void Remainder([Remainder] string values)
        {
            Console.WriteLine($"Success: {values}");
        }

        [Name("time")]
        public void TimeOnly(TimeOnly time)
        {
            Console.WriteLine($"Success: {time}");
        }

        [Name("multiple")]
        public void Test(bool truee, bool falsee)
        {
            Console.WriteLine($"Success: {truee}, {falsee}");
        }

        [Name("multiple")]
        public void Test(int i1, int i2)
        {
            Console.WriteLine($"Success: {i1}, {i2}");
        }

        [Name("optional")]
        public void Test(int i = 0, string str = "")
        {
            Console.WriteLine($"Success: {i}, {str}");
        }

        [Name("nullable")]
        public void Nullable(long? l)
        {
            Console.WriteLine($"Success: {l}");
        }

        [Name("complex")]
        public void Complex([Complex] ComplexType complex)
        {
            Console.WriteLine($"({complex.X}, {complex.Y}, {complex.Z}) {complex.Complexer}: {complex.Complexer.X}, {complex.Complexer.Y}, {complex.Complexer.Z}");
        }

        [Name("complexnullable")]
        public void Complex([Complex] ComplexerType? complex)
        {
            Console.WriteLine($"({complex?.X}, {complex?.Y}, {complex?.Z})");
        }

        [Name("nested")]
        public class NestedModule : ModuleBase<ConsumerBase>
        {
            [Name("multiple")]
            public void Test(bool truee, bool falsee)
            {
                Console.WriteLine($"Success: {truee}, {falsee}");
            }

            [Name("multiple")]
            public void Test(int i1, int i2)
            {
                Console.WriteLine($"Success: {i1}, {i2}");
            }

            [Name("optional")]
            public void Test(int i = 0, string str = "")
            {
                Console.WriteLine($"Success: {i}, {str}");
            }

            [Name("nullable")]
            public void Nullable(long? l)
            {
                Console.WriteLine($"Success: {l}");
            }

            [Name("complex")]
            public void Complex([Complex] ComplexType complex)
            {
                Console.WriteLine($"({complex.X}, {complex.Y}, {complex.Z}) {complex.Complexer}: {complex.Complexer.X}, {complex.Complexer.Y}, {complex.Complexer.Z}");
            }

            [Name("complexnullable")]
            public void Complex([Complex] ComplexerType? complex)
            {
                Console.WriteLine($"({complex?.X}, {complex?.Y}, {complex?.Z})");
            }
        }
    }
}
