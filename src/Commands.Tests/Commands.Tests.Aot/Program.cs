using Commands;
using Commands.Tests;

var manager = ComponentManager.With
    .Type<Module>()
    .Handler(ResultHandler.From<ICallerContext>((c, res, serv) => c.Respond(res)))
    .Create();

while (true)
    manager.TryExecute(new Module.CallerContext(), Console.ReadLine()!);