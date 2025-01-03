
using Commands;

var group = CommandGroup.Create(["math"]);

group.Add(Command.Create(Sum, ["sum", "add"]));
group.Add(Command.Create(Subtract, ["subtract", "sub"]));
group.Add(Command.Create(Multiply, ["multiply", "mul"]));
group.Add(Command.Create(Divide, ["divide", "div"]));
group.Add(Command.Create(Exit, ["exit"]));

var tree = new ComponentTree([group]);

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