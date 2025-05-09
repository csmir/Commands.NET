namespace Commands.Testing;

/// <summary>
///     
/// </summary>
/// <param name="command"></param>
public class TestContext(Command command) : ITestContext
{
    /// <summary>
    ///     
    /// </summary>
    public Command Command { get; set; } = command;

    /// <summary>
    ///     
    /// </summary>
    public IList<ITest> Tests { get; set; } = null!;
    
    /// <summary>
    ///     
    /// </summary>
    public CancellationTokenSource CancellationSource { get; set; } = null!;

    /// <summary>
    ///     
    /// </summary>
    public virtual void Dispose()
    {

    }
}
