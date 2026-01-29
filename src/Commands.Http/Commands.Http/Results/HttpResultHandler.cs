using Commands.Conditions;
using Commands.Hosting;
using Commands.Parsing;
using System.Net.Http.Headers;

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
        {
            var response = new HttpResult(HttpStatusCode.Forbidden);

            BuildErrorHeaders(response, exception, nameof(ConditionUnmet));

            context.Respond(response);
        }

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> InvokeFailed(IContext context, Exception exception, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (exception is HttpConditionException httpException)
            context.Respond(httpException.Response);
        else
        {
            var response = new HttpResult(HttpStatusCode.InternalServerError);

            BuildErrorHeaders(response, exception, nameof(InvokeFailed));

            context.Respond(response);
        }

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> CommandNotFound(IContext context, CommandNotFoundException exception, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        var response = new HttpResult(HttpStatusCode.InternalServerError);

        BuildErrorHeaders(response, exception, nameof(CommandNotFound));

        context.Respond(response);

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> RouteIncomplete(IContext context, CommandRouteIncompleteException exception, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        var response = new HttpResult(HttpStatusCode.NotFound);

        BuildErrorHeaders(response, exception, nameof(RouteIncomplete));

        context.Respond(response);

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> ParamsOutOfRange(IContext context, CommandOutOfRangeException exception, ParseResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        var response = new HttpResult(HttpStatusCode.BadRequest);

        BuildErrorHeaders(response, exception, nameof(ParamsOutOfRange));

        context.Respond(response);

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> ParseFailed(IContext context, Exception exception, ParseResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (exception is HttpConditionException httpException)
            context.Respond(httpException.Response);
        else
        {
            var response = new HttpResult(HttpStatusCode.BadRequest);

            BuildErrorHeaders(response, exception, nameof(ParseFailed));

            context.Respond(response);
        }

        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    protected override ValueTask<bool> Unhandled(IContext context, Exception? exception, IResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        var response = new HttpResult(HttpStatusCode.InternalServerError);

        BuildErrorHeaders(response, exception, nameof(Unhandled));

        context.Respond(response);

        return ValueTask.FromResult(true);
    }

    private void BuildErrorHeaders(HttpResult result, Exception? exception, string source)
    {
        if (_isDevelopment)
        {
            result.Headers[HttpHeaderNames.XLibErrDescription] = exception?.Message ?? "An unhandled error occurred.";
            result.Headers[HttpHeaderNames.XLibErrOrigin] = source;
        }
    }
}
