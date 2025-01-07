using Commands;
using Microsoft.Extensions.DependencyInjection;

var manager = ComponentManager.CreateBuilder()
    .WithTypes(typeof(Program).Assembly.GetExportedTypes())
    .AddResultHandler((c, r, s) => c.Respond(r))
    .Build();

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
