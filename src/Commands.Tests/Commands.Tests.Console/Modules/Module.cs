using Commands.Core;

namespace Commands.Tests
{
    public class Module : ModuleBase<CommandContext>
    {
        [Command("priority")]
        [Priority(1)]
        public void Priority1(bool optional = true)
        {
            Console.WriteLine($"Success: {Command.Priority}");
        }

        [Command("priority")]
        [Priority(2)]
        public Task Priority2(bool optional = false)
        {
            Console.WriteLine($"Success: {Command.Priority}");
            return Task.CompletedTask;
        }

        [Command("remainder")]
        public void Remainder([Remainder] string values)
        {
            Console.WriteLine($"Success: {values}");
        }

        [Command("time")]
        public void TimeOnly(TimeOnly time)
        {
            Console.WriteLine($"Success: {time}");
        }

        [Command("multiple")]
        public void Test(bool truee, bool falsee)
        {
            System.Console.WriteLine($"Success: {truee}, {falsee}");
        }

        [Command("multiple")]
        public void Test(int i1, int i2)
        {
            Console.WriteLine($"Success: {i1}, {i2}");
        }

        [Command("optional")]
        public void Test(int i = 0, string str = "")
        {
            Console.WriteLine($"Success: {i}, {str}");
        }

        [Command("nullable")]
        public void Nullable(long? l)
        {
            Console.WriteLine($"Success: {l}");
        }

        [Command("complex")]
        public void Complex([Complex] ComplexType complex)
        {
            Console.WriteLine($"({complex.X}, {complex.Y}, {complex.Z}) {complex.Complexer}: {complex.Complexer.X}, {complex.Complexer.Y}, {complex.Complexer.Z}");
        }

        [Command("complexnullable")]
        public void Complex([Complex] ComplexerType? complex)
        {
            Console.WriteLine($"({complex?.X}, {complex?.Y}, {complex?.Z})");
        }

        [Group("nested")]
        public class NestedModule : ModuleBase<CommandContext>
        {
            [Command("multiple")]
            public void Test(bool truee, bool falsee)
            {
                Console.WriteLine($"Success: {truee}, {falsee}");
            }

            [Command("multiple")]
            public void Test(int i1, int i2)
            {
                Console.WriteLine($"Success: {i1}, {i2}");
            }

            [Command("optional")]
            public void Test(int i = 0, string str = "")
            {
                Console.WriteLine($"Success: {i}, {str}");
            }

            [Command("nullable")]
            public void Nullable(long? l)
            {
                Console.WriteLine($"Success: {l}");
            }

            [Command("complex")]
            public void Complex([Complex] ComplexType complex)
            {
                Console.WriteLine($"({complex.X}, {complex.Y}, {complex.Z}) {complex.Complexer}: {complex.Complexer.X}, {complex.Complexer.Y}, {complex.Complexer.Z}");
            }

            [Command("complexnullable")]
            public void Complex([Complex] ComplexerType? complex)
            {
                Console.WriteLine($"({complex?.X}, {complex?.Y}, {complex?.Z})");
            }
        }
    }
}
