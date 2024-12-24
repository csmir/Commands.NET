using Commands;
using Commands.Conversion;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

//var tree = ComponentTree.CreateBuilder()
//    .ConfigureComponents(c =>
//    {
//        c.AddParser(new CSharpScriptParser());
//    })
//    .AddResultHandler((c, r, s) =>
//    {
//        c.Respond(r);
//    })
//    .AddModule(m =>
//    {
//        m.WithAliases("level1");
//        m.AddCommand("a", () => Console.WriteLine("Test"));
//        m.AddModule(s =>
//        {
//            s.WithAliases("level2");
//            s.AddCommand("b", () => Console.WriteLine("Test"));
//            s.AddCommand(() => Console.WriteLine("Test"));
//        });
//    })
//    .AddCommand("j", () => Console.WriteLine("Test"))
//    .Build();

var configuration = new ComponentConfiguration();

var handler = new DelegateResultHandler((c, r, s) => c.Respond(r));
var asyncHandler = new AsyncDelegateResultHandler(async (c, r, s) => await c.Respond(r));
var handlerOnlyWhenContextIs = new DelegateResultHandler<CustomCaller>((c, r, s) => c.Respond(r));

var tree = new ComponentTree(components: configuration.GetComponents(typeof(Program).Assembly), handler, asyncHandler, handlerOnlyWhenContextIs);

var services = new ServiceCollection()
    .AddSingleton(tree)
    .BuildServiceProvider();

while (true)
{
    var input = AnsiConsole.Prompt(new TextPrompt<string>("Enter a command").PromptStyle("info"));

    using var scope = services.CreateAsyncScope();

    await tree.Execute(new CustomCaller(), input, new()
    {
        Services = scope.ServiceProvider
    });

    await scope.DisposeAsync();
}
