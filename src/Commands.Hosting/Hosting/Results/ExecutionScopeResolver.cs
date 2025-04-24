namespace Commands.Hosting;

internal sealed class ExecutionScopeResolver : ResultHandler
{
    public override ValueTask HandleResult(ICallerContext caller, IResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        services.GetRequiredService<IExecutionContext>().Dispose();

        return default;
    }
}
