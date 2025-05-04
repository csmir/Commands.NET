using Commands.Testing;

namespace Commands.Samples;

[RequireContext<ConsoleCallerContext>]
public sealed class TestModule : CommandModule<ConsoleCallerContext>
{
    [Name("testcommand")]
    [Test]
    public void Test()
    {

    }

    [Name("testcommand")]
    [Test(Arguments = "args")]
    public void TestWithArgs(string _)
    {

    }

    [Name("testcommand")]
    [Test(Arguments = "args", ShouldEvaluateTo = TestResultType.MatchFailure)]
    public void TestWithArgsFail()
    {

    }
}
