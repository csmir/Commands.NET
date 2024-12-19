
namespace Commands.Samples
{
    public class CustomCaller(string name) : ICallerContext
    {
        public string Name { get; } = name;

        public Task Respond(object? response)
        {
            System.Console.WriteLine($"[{Name}] {response}");

            return Task.CompletedTask;
        }
    }
}
