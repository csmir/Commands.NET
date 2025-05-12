using Commands;

var components = new ComponentTree();

components.AddRange(typeof(Program).Assembly.GetExportedTypes());
components.Add(new Command((CommandContext<ConsoleCallerContext> c) =>
{
    foreach (var command in c.Provider!.Components.GetCommands())
        c.Respond(command);

}, "help"));

var provider = new ComponentProvider(components);

while (true)
    await provider.Execute(new ConsoleCallerContext(Console.ReadLine()));