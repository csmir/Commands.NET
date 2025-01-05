using Commands;
using Commands.Tests;

var manager = ComponentManager.CreateBuilder()
    .AddType<Module>()
    .AddResultHandler((c, res, serv) => c.Respond(res))
    .Build();

while (true)
    manager.TryExecute(new Module.CallerContext(), Console.ReadLine()!);