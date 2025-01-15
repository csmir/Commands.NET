using Commands;
using Commands.Tests;

var manager = ComponentManager.From()
    .Type<Module>()
    .Handler(ResultHandler.From<ICallerContext>((c, res, serv) => c.Respond(res)))
    .ToManager();

while (true)
    manager.TryExecute(new Module.CallerContext(), Console.ReadLine()!);