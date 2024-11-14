namespace Commands.Samples
{
    public class CustomConsumer(string name) : ConsumerBase
    {
        public string Name { get; } = name;
    }
}
