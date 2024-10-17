namespace Commands.Samples
{
    public class CustomConsumer : ConsumerBase
    {
        public string Name { get; }

        public CustomConsumer(string name)
        {
            Name = name;
        }
    }
}
