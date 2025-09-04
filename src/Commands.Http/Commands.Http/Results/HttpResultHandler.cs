using Commands.Conditions;
using Commands.Hosting;
using Commands.Parsing;

namespace Commands.Http;

/// <summary>
///     Represents a handler for HTTP command execution results, allowing for custom handling of different result types and exceptions.
/// </summary>
public class HttpResultHandler(IServiceProvider services) : ResultHandler
{
    private readonly bool _isDevelopment = services.GetService<IHostEnvironment>()?.IsDevelopment() ?? false;

    /// <inheritdoc />
    public override int Order => ExecuteLast;

    /// <inheritdoc />
    protected override ValueTask<bool> ConditionUnmet(IContext context, Exception exception, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (exception is HttpConditionException httpException)
            context.Respond(httpException.Response);
        else
            context.Respond(_isDevelopment
                ? HttpResult.Forbidden(exception.Message)
                : HttpResult.Forbidden());

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> InvokeFailed(IContext context, Exception exception, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (exception is HttpConditionException httpException)
            context.Respond(httpException.Response);
        else
            context.Respond(_isDevelopment
                ? HttpResult.InternalServerError(exception.Message)
                : HttpResult.InternalServerError());

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> CommandNotFound(IContext context, CommandNotFoundException exception, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        context.Respond(_isDevelopment
            ? HttpResult.NotFound(exception.Message)
            : HttpResult.NotFound());

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> RouteIncomplete(IContext context, CommandRouteIncompleteException exception, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        context.Respond(_isDevelopment
            ? HttpResult.NotFound(exception.Message)
            : HttpResult.NotFound());

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> ParamsOutOfRange(IContext context, CommandOutOfRangeException exception, ParseResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        context.Respond(_isDevelopment
            ? HttpResult.BadRequest(exception.Message)
            : HttpResult.BadRequest());

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> ParseFailed(IContext context, Exception exception, ParseResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (exception is HttpConditionException httpException)
            context.Respond(httpException.Response);
        else
            context.Respond(_isDevelopment
                ? HttpResult.BadRequest(exception.Message)
                : HttpResult.BadRequest());

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> Unhandled(IContext context, Exception? exception, IResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        context.Respond(_isDevelopment
            ? HttpResult.InternalServerError(exception?.Message ?? "An unhandled error occurred.")
            : HttpResult.InternalServerError());

        return ValueTask.FromResult(true);
    }
}
