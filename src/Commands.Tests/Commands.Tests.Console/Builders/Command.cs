using Commands.Builders;

namespace Commands.Tests;

public class Command
{
    public static void TestBuilds()
    {
        var calculateModule = new CommandGroupBuilder()
            .AddName("calculate");

        var sumCommand = new CommandBuilder()
            .WithHandler((CommandContext<AsyncCustomCaller> ctx, int num1, int op2) => $"{num1} + {op2} = {num1 + op2}!")
            .AddName("sum");

        var averageCommand = new CommandBuilder()
            .WithHandler((CommandContext<AsyncCustomCaller> ctx, params int[] numbers) => $"The average of {string.Join(", ", numbers)} is {numbers.Sum() / (decimal)numbers.Length}!")
            .AddName("average");

        calculateModule.AddCommand(sumCommand);
        calculateModule.AddCommand(averageCommand);

        var component = calculateModule.Build();
    }
}
