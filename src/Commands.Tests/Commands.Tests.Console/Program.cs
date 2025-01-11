using Commands;
using Commands.Testing;
using Microsoft.Extensions.DependencyInjection;

var manager = ComponentManager.CreateBuilder()
    .WithTypes(typeof(Program).Assembly.GetExportedTypes())
    .AddResultHandler<ConsoleContext>((c, r, s) => c.Respond(r.Exception?.InnerException != null ? r.Exception.InnerException : r.Exception))
    .Build();

var testRunner = TestRunner.Create<TestCallerContext>(manager.GetCommands());

testRunner.TestFailed += (result) =>
{
    throw new InvalidOperationException($"Failed to evaluate command {result.Command} with expected outcome {result.ExpectedResult}. Received {result.ActualResult}.", result.Exception);
};

await testRunner.Run();

if (testRunner.CountCompleted == testRunner.Count)
    Console.WriteLine("All tests ran succesfully.");

var services = new ServiceCollection()
    .AddSingleton(manager)
    .BuildServiceProvider();

while (true)
{
    var input = Console.ReadLine();

    var values = ArgumentArray.Read(input);

    using var scope = services.CreateScope();

    manager.TryExecute(new ConsoleContext(), values, new()
    {
        Services = scope.ServiceProvider
    });
}
