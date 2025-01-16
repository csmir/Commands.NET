using Commands.Conditions;
using Commands.Parsing;
using Commands.Testing;

namespace Commands.Tests;

public static class FunctionalAPILayout
{
    public static void CreateCommand()
    {
        Command.With
            .Delegate(() => { })
            .Name("command")
            .Create();

        Command.From("command")
            .Delegate(() => { })
            .Create();

        Command.From(() => { }, "command")
            .Create();
    }

    public static void CreateCommandGroup()
    {
        CommandGroup.With
            .Name("group")
            .Create();

        CommandGroup.From("group")
            .Create();
    }

    public static void CreateTypeParser()
    {
        TypeParser.For<TimeSpan>()
            .Delegate(TimeSpan.TryParse)
            .Create();

        TypeParser.From<Version>(Version.TryParse)
            .Create();

        TypeParser.From<Guid>((c, p, o, s) => ParseResult.FromSuccess(Guid.NewGuid()))
            .Create();
    }

    public static void CreateResultHandler()
    {
        ResultHandler.For<ConsoleContext>()
            .Delegate((c, r, s) => c.Respond(r))
            .Create();

        ResultHandler.From<ConsoleContext>((c, r, s) => c.Respond(r))
            .Create();
    }

    public static void CreateExecuteCondition()
    {
        ExecuteCondition.For<ANDEvaluator>()
            .Delegate((c, m, s) => ConditionResult.FromSuccess())
            .Create();

        ExecuteCondition.From<ANDEvaluator>((c, m, s) => ConditionResult.FromSuccess())
            .Create();
    }

    public static void CreateManager()
    {
        ComponentManager.With
            .Type<BasicModule>()
            .Create();

        ComponentManager.From(CommandGroup.From("group"))
            .Create();
    }

    public static void CreateConfiguration()
    {
        ComponentConfiguration.With
            .Parsers(TypeParser.From<Version>(Version.TryParse))
            .Create();

        ComponentConfiguration.From(new CSharpScriptParser()
            ).Create();
    }

    public static void CreateTestProvider()
    {
        TestProvider.With
            .Command(null!)
            .Create();

        TestProvider.From(null!)
            .Create();
    }

    public static void CreateTestRunner()
    {
        TestRunner.For<TestCallerContext>()
            .Tests()
            .Create();
    }
}
