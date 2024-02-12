using Commands;
using Commands.Parsing;

var manager = CommandManager.CreateBuilder()
    .AddResultResolver((c, r, s) =>
    {
        if (!r.Success)
            Console.WriteLine(r);
    })
    .Build();

while (true)
{
    var input = StringParser.Parse(Console.ReadLine());

    await manager.TryExecuteAsync(new ConsumerBase(), input);
}