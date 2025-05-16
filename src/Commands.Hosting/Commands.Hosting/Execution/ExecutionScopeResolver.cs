namespace Commands.Hosting;

internal sealed class ExecutionScopeResolver : ResultHandler
{
    public override ValueTask HandleResult(IContext context, IResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        services.GetService<IExecutionScope>()?.Dispose();

        return default;
    }
}
