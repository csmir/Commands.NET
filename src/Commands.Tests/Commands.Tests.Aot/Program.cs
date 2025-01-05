using Commands;
using Commands.Tests;

var tree = ComponentManager.CreateBuilder()
    .AddType<Module>()
    .AddResultHandler((c, res, serv) => c.Respond(res))
    .Build();

while (true)
    tree.TryExecute(new Module.CallerContext(), Console.ReadLine()!);