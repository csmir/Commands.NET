namespace Commands.Tests
{
    public class CustomCaller : ICallerContext
    {
        public Task Respond(object? response)
        {
            Console.WriteLine(response);
            return Task.CompletedTask;
        }
    }
}
