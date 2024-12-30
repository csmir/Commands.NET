namespace Commands.Tests
{
    public sealed class EnumerableModule : CommandModule
    {
        [Name("param-array")]
        public static void ParamArray(params string[] _)
        {

        }

        [Name("array")]
        public static void Array(int _, [Remainder] int[] __)
        {

        }
    }
}
