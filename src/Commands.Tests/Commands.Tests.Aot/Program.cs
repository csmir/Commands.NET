using Commands;
using Commands.Tests;

var tree = ComponentTree.CreateBuilder()
    .AddType<Module>()
    .AddResultHandler((c, res, serv) => c.Respond(res))
    .Build();

while (true)
    tree.Execute(new Module.CallerContext(), Console.ReadLine()!);