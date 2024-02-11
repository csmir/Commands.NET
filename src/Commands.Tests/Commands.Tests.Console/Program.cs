using Commands;
using Commands.Helpers;
using Commands.Parsing;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .ConfigureCommands(configure =>
    {
        configure.AddResultResolver((c, r, s) =>
        {
            if (!r.Success)
                Console.WriteLine(r);
        });
    })
    .BuildServiceProvider();

var framework = services.GetRequiredService<CommandManager>();

while (true)
{
    var input = StringParser.Parse(Console.ReadLine());

    await framework.TryExecuteAsync(new ConsumerBase(), input);
}