namespace Commands.Tests
{
    public class CustomCaller : ICallerContext
    {
        public int ArgumentCount = 0;

        public Task Respond(object? response)
        {
            Console.WriteLine(response);
            return Task.CompletedTask;
        }
    }
}
