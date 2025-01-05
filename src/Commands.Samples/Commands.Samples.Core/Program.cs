
using Commands;
using Commands.Samples;

var math = CommandGroup.Create("math");

math.Add(Command.Create(Sum, "sum", "add"));
math.Add(Command.Create(Subtract, "subtract", "sub"));
math.Add(Command.Create(Multiply, "multiply", "mul"));
math.Add(Command.Create(Divide, "divide", "div"));
math.Add(Command.Create(Exit, "exit"));

var help = CommandGroup.Create<HelpModule>();

var tree = ComponentManager.Create(math, help);

while (true)
    tree.TryExecute(new ConsoleCallerContext(), Console.ReadLine());

static double Sum(double number, int sumBy)
    => number + sumBy;
static double Subtract(double number, int subtractBy)
    => number - subtractBy;
static double Multiply(double number, int multiplyBy)
    => number * multiplyBy;
static double Divide(double number, int divideBy)
    => number / divideBy;
static void Exit()
    => Environment.Exit(0);

class ConsoleCallerContext : ICallerContext
{
    public void Respond(object? message)
        => Console.WriteLine(message);
}