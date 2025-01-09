using Commands.Testing;

namespace Commands.Tests;

[Name("command")]
public class BasicModule : CommandModule
{
    [Name("nested")]
    public class NestedModule : CommandModule
    {
        [Name("deconstruct")]
        [Test(Arguments = "1 2 3 4 5 6")]
        [Test(ShouldEvaluateTo = TestResultType.MatchFailure, Arguments = "1 2 3 4 5 6 7")]
        public static string Deconstruct([Deconstruct] ConstructibleType constructedType)
            => $"({constructedType.X}, {constructedType.Y}, {constructedType.Z}) {constructedType.Child}: {constructedType.Child.InnerX}, {constructedType.Child.InnerY}, {constructedType.Child.InnerZ}";

        [Name("multiple")]
        [Test(Arguments = "true false")]
        [Test(ShouldEvaluateTo = TestResultType.ParseFailure, Arguments = "not bools")]
        public static string Test(bool @true, bool @false)
            => $"Success: {@true}, {@false}";

        [Name("multiple")]
        [Test(Arguments = "1 2")]
        [Test(ShouldEvaluateTo = TestResultType.ParseFailure, Arguments = "1 not-int")]
        public static string Test(int i1, int i2)
            => $"Success: {i1}, {i2}";

        [Name("optional")]
        [Test(Arguments = "1")]
        [Test(Arguments = "1 test")]
        [Test]
        public static string Test(int i = 0, string str = "")
            => $"Success: {i}, {str}";

        [Name("nullable")]
        [Test(Arguments = "1")]
        [Test(Arguments = "null")]
        [Test(ShouldEvaluateTo = TestResultType.ParseFailure, Arguments = "not-int")]
        public static string Nullable(long? l)
            => $"Success: {l}";
    }

    [Name("remainder")]
    [Test(Arguments = "one two three four")]
    public void Remainder([Remainder] string values)
        => Respond($"Success: {values}");

    [Name("param-array")]
    [Test(Arguments = "one two three four five")]
    public void ParamArray(params string[] range)
        => Respond($"Success: {string.Join(' ', range)}");

    [Name("array")]
    [Test(Arguments = "1 2 3 4 5")]
    [Test(ShouldEvaluateTo = TestResultType.ParseFailure, Arguments = "1 2 3 4 blab")]
    public void Array(int num, [Remainder] int[] range)
        => Respond($"Success: {num}, {string.Join(' ', range)}");

    [Name("async")]
    [Test(Arguments = "true")]
    [Test(Arguments = "false")]
    public async Task Async(bool delay)
    {
        if (delay)
        {
            await Task.Delay(Random.Shared.Next(5000, 10000));

            await Respond("Success");
        }
    }
}
