using Commands;
using Commands.Tests;

var manager = new ComponentManager([new DelegateResultHandler<Module.CallerContext>((c, r, s) => { c.Respond(r); return ValueTask.CompletedTask; })])
{
    new CommandGroup(["delegate"])
    {
        new Command(() => "Working", ["work"])
    },
    new CommandGroup(typeof(Module), new ComponentConfiguration())
};

while (true)
    manager.TryExecute(new Module.CallerContext(), Console.ReadLine()!);