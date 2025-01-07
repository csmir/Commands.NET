using Commands.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Tests;

public class FeatureRules
{
    /// <summary>
    ///     Rule: Every builder type must have a static accessor and a parameterless constructor.
    /// </summary>
    [SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "Tests")]
    public static void Builders()
    {
        // Command:
        new CommandBuilder();

        Command.CreateBuilder();

        // Group:
        new CommandGroupBuilder();

        CommandGroup.CreateBuilder();

        // Condition:
        new ConditionBuilder<ANDEvaluator, ICallerContext>();

        CommandCondition.CreateBuilder<ANDEvaluator, ICallerContext>();

        // Configuration:
        new ComponentConfigurationBuilder();

        ComponentConfiguration.CreateBuilder();

        // Manager:
        new ComponentManagerBuilder();

        ComponentManager.CreateBuilder();
    }

    /// <summary>
    ///     Rule: Every component type and handler must have a fluent static creation action for multiple configurable assets.
    /// </summary>
    public static void Immediate()
    {
        // Commands:
        Command.Create(() => { }, "name");

        Command.Create(() => { }, "name", "anothername");

        Command.Create(() => { }, ["name"], null);

        Command.Create(() => { }, ["name"], [], null);

        // Groups:
        CommandGroup.Create("name");

        CommandGroup.Create("name", "anothername");

        CommandGroup.Create(["name"], null);

        CommandGroup.Create(typeof(Program));

        // Conditions:
        CommandCondition.Create<ANDEvaluator>((ctx, cmd, services) => ConditionResult.FromSuccess());

        CommandCondition.Create<ANDEvaluator, ICallerContext>((ctx, cmd, services) => ConditionResult.FromSuccess());

        // Configuration:
        ComponentConfiguration.Create();

        ComponentConfiguration.Create(new TryParseParser<Version>(Version.TryParse), new TryParseParser<Guid>(Guid.TryParse));

        ComponentConfiguration.Create([], null);

        // Manager:
        ComponentManager.Create();

        ComponentManager.Create([]);

        ComponentManager.Create([], []);
    }
}
