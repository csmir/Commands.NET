using Commands;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;

var manager = ComponentManager.CreateBuilder()
    .WithTypes(typeof(Program).Assembly.GetExportedTypes())
    .AddResultHandler<AutomatedContext>((c, r, s) =>
    {
        if (!c.ShouldFail)
        {
            Console.WriteLine($"Received {r.Exception}; Failed but should have succeeded. Input: {c.Input}");
            return;
        }
    })
    .AddResultHandler<ConsoleContext>((c, r, s) => c.Respond(r))
    .Build();

var commands = manager.GetCommands();

foreach (var command in commands.OfType<Command>())
{
    foreach (var input in command.Attributes.OfType<TryInputAttribute>())
    {
        var ctx = new AutomatedContext(input.Input, input.ShouldFail);

        var commandName = command.GetFullName(false);

        if (!string.IsNullOrEmpty(input.Input))
            commandName += " " + input.Input;

        manager.TryExecute(ctx, commandName);
    }
}

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
