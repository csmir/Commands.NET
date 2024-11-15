namespace Commands.Tests
{
    public sealed class EnumerableModule : ModuleBase
    {
        [Name("param-array")]
        public void ParamArray(params string[] input)
        {

        }

        [Name("array")]
        public void Array(int firstInput, [Remainder] int[] input)
        {

        }

        [Name("list")]
        public void List(string firstInput, [Remainder] List<int> input)
        {

        }

        [Name("enumerable")]
        public void Enumerable([Remainder] IEnumerable<bool> input)
        {

        }

        [Name("collection")]
        public void Collection(string firstInput, bool secondInput, [Remainder] ICollection<string> input)
        {
            foreach (var item in input)
            {
                Console.WriteLine(item);
            }
        }

        [Name("readonly-collection")]
        public void ReadOnlyCollection([Remainder] IReadOnlyCollection<bool> input)
        {

        }

        [Name("set")]
        public void Set([Remainder] ISet<int> input)
        {

        }

        [Name("hash-set")]
        public void HashSet([Remainder] HashSet<long> input)
        {

        }
    }
}
