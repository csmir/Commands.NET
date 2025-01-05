
using Commands;
using Commands.Samples;

var exit = Command.Create(Exit, "exit");

var mathCommands = CommandGroup.Create("math");

mathCommands.AddRange(
    Command.Create(Sum, "sum", "add"), 
    Command.Create(Subtract, "subtract", "sub"), 
    Command.Create(Multiply, "multiply", "mul"), 
    Command.Create(Divide, "divide", "div")
);

var helpCommands = CommandGroup.Create<HelpModule>();

var manager = ComponentManager.Create(exit, mathCommands, helpCommands);

manager.TryExecute(new ConsoleCallerContext(), args);

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