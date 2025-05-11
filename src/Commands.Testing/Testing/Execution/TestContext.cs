namespace Commands.Testing;

public class TestContext(Command command) : ITestContext
{
    public Command Command { get; set; } = command;

    public IList<ITest> Tests { get; set; } = null!;
    
    public CancellationTokenSource CancellationSource { get; set; } = null!;

    public virtual void Dispose()
    {

    }
}
