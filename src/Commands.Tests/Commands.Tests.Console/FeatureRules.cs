using Commands.Builders;
using Commands.Conditions;
using Commands.Conversion;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Tests;

public class FeatureRules
{
    /// <summary>
    ///     Rule: Every builder type must have a static accessor and a parameterless constructor.
    /// </summary>
    [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Tests")]
    public static void Builders()
    {
        // Command:
        var commandBuilder = new CommandBuilder();

        var staticCommandBuilder = Command.CreateBuilder();

        // Group:
        var commandGroupBuilder = new CommandGroupBuilder();

        var staticCommandGroupBuilder = CommandGroup.CreateBuilder();

        // Condition:
        var conditionBuilder = new ConditionBuilder<ANDEvaluator, ICallerContext>();

        var staticConditionBuilder = CommandCondition.CreateBuilder<ANDEvaluator, ICallerContext>();

        // Configuration:
        var configurationBuilder = new ComponentConfigurationBuilder();

        var staticConfigurationBuilder = ComponentConfiguration.CreateBuilder();

        // Manager:
        var managerBuilder = new ComponentManagerBuilder();

        var staticManagerBuilder = ComponentManager.CreateBuilder();
    }

    /// <summary>
    ///     Rule: Every component type and handler must have a fluent static creation action for multiple configurable assets.
    /// </summary>
    public static void Immediate()
    {
        // Commands:
        var command = Command.Create(() => { }, "name");

        var commandWithNames = Command.Create(() => { }, "name", "anothername");

        var commandWithConfiguration = Command.Create(() => { }, ["name"], null);

        var commandWithConditionsAndConfiguration = Command.Create(() => { }, ["name"], [], null);

        // Groups:
        var group = CommandGroup.Create("name");

        var groupWithNames = CommandGroup.Create("name", "anothername");

        var groupWithConfiguration = CommandGroup.Create(["name"], null);

        var groupWithTypeImplementation = CommandGroup.Create(typeof(Program));

        // Conditions:
        var condition = CommandCondition.Create<ANDEvaluator>((ctx, cmd, services) => ConditionResult.FromSuccess());

        var conditionWithContext = CommandCondition.Create<ANDEvaluator, ICallerContext>((ctx, cmd, services) => ConditionResult.FromSuccess());

        // Configuration:
        var configuration = ComponentConfiguration.Create();

        var configurationWithParsers = ComponentConfiguration.Create(new TryParseParser<Version>(Version.TryParse), new TryParseParser<Guid>(Guid.TryParse));

        var configurationWithParsersAndProperties = ComponentConfiguration.Create([], null);

        // Manager:
        var manager = ComponentManager.Create();

        var managerWithComponents = ComponentManager.Create([]);

        var managerWithComponentsAndHandlers = ComponentManager.Create([], []);
    }
}
