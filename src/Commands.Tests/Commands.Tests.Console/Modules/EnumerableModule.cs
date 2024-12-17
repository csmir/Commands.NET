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

        [Name("list")]
        public static void List(string _, [Remainder] List<int> __)
        {

        }

        [Name("enumerable")]
        public static void Enumerable([Remainder] IEnumerable<bool> _)
        {

        }

        [Name("collection")]
        public static void Collection(string _, bool __, [Remainder] ICollection<string> input)
        {
            foreach (var item in input)
            {
                Console.WriteLine(item);
            }
        }

        [Name("readonly-collection")]
        public static void ReadOnlyCollection([Remainder] IReadOnlyCollection<bool> _)
        {

        }

        [Name("set")]
        public static void Set([Remainder] ISet<int> _)
        {

        }

        [Name("hash-set")]
        public static void HashSet([Remainder] HashSet<long> _)
        {

        }
    }
}
