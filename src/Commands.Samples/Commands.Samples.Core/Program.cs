
using Commands;
using Commands.Builders;

var commands = new List<CommandBuilder>() {
    new("sum", Sum),
    new("subtract", Subtract),
    new("multiply", Multiply),
    new("divide", Divide),
    new("exit", Exit)
};

var tree = new ComponentTree(commands.Select(x => x.Build()), new DelegateResultHandler<ConsoleCallerContext>((c, r, s) => c.Respond(r)));

while (true)
    tree.Execute(new ConsoleCallerContext(), Console.ReadLine()!);

static int Sum(int a, int b) => a + b;
static int Subtract(int a, int b) => a - b;
static int Multiply(int a, int b) => a * b;
static int Divide(int a, int b) => a / b;
static void Exit() => Environment.Exit(0);

class ConsoleCallerContext : ICallerContext
{
    public void Respond(object? message)
        => Console.WriteLine(message);
}