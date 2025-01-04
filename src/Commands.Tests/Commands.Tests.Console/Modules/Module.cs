namespace Commands.Tests;

[Name("command")]
public class Module : CommandModule
{
    [Name("nested")]
    public class NestedModule : CommandModule
    {
        [Name("deconstruct")]
        public static string Deconstruct([Deconstruct] ConstructibleType complex)
            => $"({complex.X}, {complex.Y}, {complex.Z}) {complex.Complexer}: {complex.Complexer.X}, {complex.Complexer.Y}, {complex.Complexer.Z}";

        [Name("deconstruct")]
        public static string Deconstruct([Deconstruct] ConstructibleInnerType? complex)
            => $"({complex?.X}, {complex?.Y}, {complex?.Z})";

        [Name("multiple")]
        public static string Test(bool @true, bool @false)
            => $"Success: {@true}, {@false}";

        [Name("multiple")]
        public static string Test(int i1, int i2)
            => $"Success: {i1}, {i2}";

        [Name("optional")]
        public static string Test(int i = 0, string str = "")
            => $"Success: {i}, {str}";

        [Name("nullable")]
        public static string Nullable(long? l)
            => $"Success: {l}";
    }

    [Name("remainder")]
    public void Remainder([Remainder] string values)
        => Respond($"Success: {values}");

    [Name("multiple")]
    public void Test(bool @true, bool @false)
        => Respond($"Success: {@true}, {@false}");

    [Name("multiple")]
    public void Test(int i1, int i2)
        => Respond($"Success: {i1}, {i2}");

    [Name("optional")]
    public void Test(int i = 0, string str = "")
        => Respond($"Success: {i}, {str}");

    [Name("nullable")]
    public void Nullable(long? l)
        => Respond($"Success: {l}");

    [Name("deconstruct")]
    public void Deconstruct([Deconstruct] ConstructibleType complex)
        => Respond($"({complex.X}, {complex.Y}, {complex.Z}) {complex.Complexer}: {complex.Complexer.X}, {complex.Complexer.Y}, {complex.Complexer.Z}");

    [Name("deconstruct")]
    public void Deconstruct([Deconstruct] ConstructibleInnerType? complex)
        => Respond($"({complex?.X}, {complex?.Y}, {complex?.Z})");

    [Name("param-array")]
    public void ParamArray(params string[] range)
        => Respond($"Success: {string.Join(' ', range)}");

    [Name("array")]
    public void Array(int num, [Remainder] int[] range)
        => Respond($"Success: {num}, {string.Join(' ', range)}");
}
