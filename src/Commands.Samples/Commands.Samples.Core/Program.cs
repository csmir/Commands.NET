
using Commands;
using Commands.Builders;

var commands = new List<CommandBuilder>() {
    new("sum", Sum),
    new("subtract", Subtract),
    new("multiply", Multiply),
    new("divide", Divide),
    new("exit", Exit)
};

var tree = new ComponentTree(commands.Select(x => x.Build()), new DelegateResultHandler<ConsoleCallerContext>(HandleResult));

while (true)
    await tree.Execute(new ConsoleCallerContext(), Console.ReadLine()!);

static int Sum(int a, int b) => a + b;
static int Subtract(int a, int b) => a - b;
static int Multiply(int a, int b) => a * b;
static int Divide(int a, int b) => a / b;
static void Exit() => Environment.Exit(0);

static async ValueTask HandleResult(ConsoleCallerContext context, IExecuteResult result, IServiceProvider provider)
    => await context.Respond(result);

class ConsoleCallerContext : ICallerContext
{
    public Task Respond(object? message)
        => new(() => Console.WriteLine(message));
}