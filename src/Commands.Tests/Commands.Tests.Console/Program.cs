using Commands;
using Commands.Parsing;

var builder = new BuildOptions();

builder.AddResultResolver((c, r, s) =>
{
    if (!r.Success)
        Console.WriteLine(r);
});

var framework = new CommandManager(builder);

while (true)
{
    var input = StringParser.Parse(Console.ReadLine());

    await framework.TryExecuteAsync(new ConsumerBase(), input);
}