using Commands.Builders;
using Commands.Conditions;
using Commands.Parsing;
using Commands.Testing;
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
        // Parsers:
        TypeParser.Create<Version>(Version.TryParse);

        TypeParser.Create<Version>((c, p, o, s) => ParseResult.FromSuccess(o));

        // Results:
        ResultHandler.Create<ICallerContext>((c, r, s) => { });

        // Conditions:
        ExecuteCondition.Create<ANDEvaluator>((c, m, s) => ConditionResult.FromSuccess());

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

        // Configuration:
        ComponentConfiguration.Create();

        ComponentConfiguration.Create(TypeParser.Create<Version>(Version.TryParse), TypeParser.Create<Guid>(Guid.TryParse));

        ComponentConfiguration.Create([], null);

        // Manager:
        ComponentManager.Create();

        ComponentManager.Create([]);

        ComponentManager.Create([], []);

        // Tests:
        TestRunner.Create([]);
    }
}
