namespace Commands.Tests;

[Name("command")]
public class BasicModule : CommandModule
{
    [Name("nested")]
    public class NestedModule : CommandModule
    {
        [Name("deconstruct")]
        [TryInput("1 2 3", true)]
        [TryInput("1 2 3 4")]
        [TryInput("1 2 3 4 5 6")]
        [TryInput("1 2 3 4 5 6 7 8", true)]
        public static string Deconstruct([Deconstruct] ConstructibleType constructedType)
            => $"({constructedType.X}, {constructedType.Y}, {constructedType.Z}) {constructedType.Child}: {constructedType.Child.InnerX}, {constructedType.Child.InnerY}, {constructedType.Child.InnerZ}";

        [Name("multiple")]
        [TryInput("true false")]
        [TryInput("1 2", true)]
        public static string Test(bool @true, bool @false)
            => $"Success: {@true}, {@false}";

        [Name("multiple")]
        [TryInput("1 2")]
        [TryInput("true false", true)]
        public static string Test(int i1, int i2)
            => $"Success: {i1}, {i2}";

        [Name("optional")]
        [TryInput("")]
        [TryInput("1")]
        [TryInput("1 test")]
        [TryInput("test test", true)]
        public static string Test(int i = 0, string str = "")
            => $"Success: {i}, {str}";

        [Name("nullable")]
        [TryInput("", true)]
        [TryInput("1")]
        [TryInput("null")]
        public static string Nullable(long? l)
            => $"Success: {l}";
    }

    [Name("remainder")]
    [TryInput("1 2 3 4 5")]
    [TryInput("1 2 3 4 5 6 7 8 9 10")]
    public void Remainder([Remainder] string values)
        => Respond($"Success: {values}");

    [Name("param-array")]
    [TryInput("one two three four five")]
    public void ParamArray(params string[] range)
        => Respond($"Success: {string.Join(' ', range)}");

    [Name("array")]
    [TryInput("1 2 3 4 5")]
    [TryInput("1 2 3 4 blab", true)]
    public void Array(int num, [Remainder] int[] range)
        => Respond($"Success: {num}, {string.Join(' ', range)}");

    [Name("async")]
    [TryInput("true")]
    [TryInput("false")]
    public async Task Async(bool delay)
    {
        if (delay)
        {
            await Task.Delay(Random.Shared.Next(5000, 10000));

            await Respond("Success");
        }
    }
}
