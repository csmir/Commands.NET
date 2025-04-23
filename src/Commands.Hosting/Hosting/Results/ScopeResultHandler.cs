namespace Commands.Hosting;

internal sealed class ScopeResultHandler : ResultHandler
{
    public override ValueTask HandleResult(ICallerContext caller, IResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        services.GetRequiredService<IExecutionContext>().Dispose();

        return default;
    }
}
