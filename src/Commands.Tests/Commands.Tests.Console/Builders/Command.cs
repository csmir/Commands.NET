using Commands.Builders;
using Commands.Conditions;

namespace Commands.Tests
{
    public class Command
    {
        public static void TestBuilds()
        {
            ComponentConfigurationBuilder.Default.Properties[ConfigurationPropertyDefinitions.NameValidationExpression] = "^[a-zA-Z0-9_]+$";
            ComponentConfigurationBuilder.Default.Properties[ConfigurationPropertyDefinitions.MakeModulesReadonly] = false;

            var calculateModule = new ModuleBuilder()
                .WithAliases("calculate");

            var lengthCondition = new ConditionBuilder<ANDEvaluator, CustomCaller>()
                .WithHandler((ctx, cmd, trigger, services) => ctx.ArgumentCount <= 10
                    ? ConditionResult.FromSuccess()
                    : ConditionResult.FromError("The input is too long."))
                .WithTriggers(ConditionTrigger.Parsing);

            var sumCommand = new CommandBuilder()
                .WithHandler((CommandContext<CustomCaller> ctx, int num1, int op2) => $"{num1} + {op2} = {num1 + op2}!")
                .WithAliases("sum");

            var averageCommand = new CommandBuilder()
                .WithHandler((CommandContext<CustomCaller> ctx, params int[] numbers) => $"The average of {string.Join(", ", numbers)} is {numbers.Sum() / (decimal)numbers.Length}!")
                .AddCondition(lengthCondition)
                .WithAliases("average");

            calculateModule.AddCommand(sumCommand);
            calculateModule.AddCommand(averageCommand);

            var component = calculateModule.Build();
        }
    }
}
