namespace Commands.Samples
{
    public sealed class HostedCallerContext : ICallerContext
    {
        public string? Input { get; set; }

        public Task Respond(object? response)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("cout");
            Console.ResetColor();
            Console.WriteLine($": Commands.Samples.HostedCallerContext['{Input}']");
            Console.Write("      ");
            Console.WriteLine(response);

            return Task.CompletedTask;
        }
    }
}
