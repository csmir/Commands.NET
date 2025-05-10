
using Commands;
using Commands.Samples;

var exitCommand = new Command(() => Environment.Exit(0), "exit");

var mathGroup = new CommandGroup("math")
{
    new Command(Sum, "sum", "add"),
    new Command(Subtract, "subtract", "sub"),
    new Command(Multiply, "multiply", "mul"),
    new Command(Divide, "divide", "div")
};

var provider = ComponentProvider.CreateBuilder()
    .AddComponentType<HelpModule>()
    .AddComponent(mathGroup)
    .AddComponent(exitCommand)
    .Build();

await provider.Execute(new ConsoleCallerContext(args));

static double Sum(double number, int sumBy)
    => number + sumBy;
static double Subtract(double number, int subtractBy)
    => number - subtractBy;
static double Multiply(double number, int multiplyBy)
    => number * multiplyBy;
static double Divide(double number, int divideBy)
    => number / divideBy;
