namespace Commands.Samples
{
    public sealed class HostedCallerContext : ICallerContext
    {
        public Task Respond(object? response)
        {
            Console.WriteLine(response);

            return Task.CompletedTask;
        }
    }
}
