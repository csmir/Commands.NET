using Commands;
using Commands.Tests;

var tree = ComponentTree.CreateBuilder()
    .AddCommand("inline", InlineCommand)
    .AddType<Module>()
    .AddResultHandler(async (c, res, serv) => await c.Respond(res))
    .Build();

while (true)
{
    await tree.Execute(new Module.CallerContext(), Console.ReadLine()!);
}

string InlineCommand(string command)
{
    return "Success";
}