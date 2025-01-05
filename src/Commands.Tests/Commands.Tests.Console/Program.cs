using Commands;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;

var manager = ComponentManager.CreateBuilder()
    .ConfigureComponents(configure =>
    {
        configure.AddParser(new CSharpScriptParser());
    })
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

    manager.TryExecute(new DefaultCallerContext(), values, new()
    {
        Services = scope.ServiceProvider
    });
}
