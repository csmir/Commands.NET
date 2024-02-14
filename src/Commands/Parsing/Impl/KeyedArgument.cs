namespace Commands.Parsing
{
    public readonly struct KeyedArgument(string key, string? value) : IParseArgument
    {
        public object? Value { get; } = value;

        public string Key { get; } = key;

        public static implicit operator KeyedArgument((string key, string? value) pair)
        {
            return new KeyedArgument(pair.key, pair.value);
        }
    }
}
