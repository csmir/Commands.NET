using Commands;

var components = new ComponentTree();

components.AddRange(typeof(Program).Assembly.GetExportedTypes());
components.Add(new Command(([Dependency] IComponentProvider provider, ConsoleContext context) =>
{
    foreach (var command in provider.Components.GetCommands())
        context.Respond(command);

}, "help"));

var commands = components.GetCommands();

foreach (var command in commands)
{
    var testResult = await command.Test((input) => new ConsoleContext(input));

    if (!testResult.All(x => x.Success))
        Console.WriteLine($"Test failed for command: {command}");
}

var provider = new ComponentProvider(components);

while (true)
    await provider.Execute(new ConsoleContext(Console.ReadLine()));