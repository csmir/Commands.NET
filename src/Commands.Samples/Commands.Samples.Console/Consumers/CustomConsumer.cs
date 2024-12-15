namespace Commands.Samples
{
    public class CustomConsumer(string name) : CallerContext
    {
        public string Name { get; } = name;
    }
}
