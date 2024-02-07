using Commands.Core;
using Commands.Helpers;
using Commands.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var collection = new ServiceCollection()
    .ConfigureCommands()
    .TryAddResolver((context, result, services) =>
    {
        if (!result.Success())
            Console.WriteLine(result);
    })
    .AddLogging(configure =>
    {
        configure.AddSimpleConsole();
        configure.SetMinimumLevel(LogLevel.Information);
    });

var services = collection.BuildServiceProvider();

var framework = services.GetRequiredService<CommandManager>();

bool swap = true;
while (true)
{
    var input = Array.Empty<object>();
    if (swap)
    {
        input = StringParser.Parse("async true");
    }
    else
    {
        input = StringParser.Parse("async false");
    }

    await framework.TryExecuteAsync(new ConsumerBase(), input, new()
    {
        AsyncApproach = AsyncApproach.Discard
    });
}