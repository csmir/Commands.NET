using Commands.Conditions;
using Commands.Hosting;
using Commands.Parsing;

namespace Commands.Http;

/// <summary>
///     Represents a handler for HTTP command execution results, allowing for custom handling of different result types and exceptions.
/// </summary>
/// <param name="hostEnvironment">The environment in which the current pipeline is executing.</param>
public class HttpResultHandler(IHostEnvironment hostEnvironment) : ResultHandler
{
    /// <inheritdoc />
    protected override ValueTask ConditionUnmet(IContext context, Exception exception, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (exception is HttpConditionException httpException)
            context.Respond(new HttpResponse(httpException.StatusCode, Encoding.UTF8.GetBytes(httpException.Message), httpException.ContentType));

        else if (hostEnvironment.IsDevelopment())
            context.Respond(new HttpResponse(HttpStatusCode.Forbidden, exception.Message));

        else
            context.Respond(new HttpResponse(HttpStatusCode.Forbidden));

        return default;
    }

    /// <inheritdoc />
    protected override ValueTask InvokeFailed(IContext context, Exception exception, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (hostEnvironment.IsDevelopment())
            context.Respond(new HttpResponse(HttpStatusCode.InternalServerError, exception.Message));
        else
            context.Respond(new HttpResponse(HttpStatusCode.InternalServerError));

        return default;
    }

    /// <inheritdoc />
    protected override ValueTask CommandNotFound(IContext context, CommandNotFoundException exception, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (hostEnvironment.IsDevelopment())
            context.Respond(new HttpResponse(HttpStatusCode.NotFound, exception.Message));
        else
            context.Respond(new HttpResponse(HttpStatusCode.NotFound));

        return default;
    }

    /// <inheritdoc />
    protected override ValueTask RouteIncomplete(IContext context, CommandRouteIncompleteException exception, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (hostEnvironment.IsDevelopment())
            context.Respond(new HttpResponse(HttpStatusCode.NotFound, exception.Message));
        else
            context.Respond(new HttpResponse(HttpStatusCode.NotFound));

        return default;
    }

    /// <inheritdoc />
    protected override ValueTask ParamsOutOfRange(IContext context, CommandOutOfRangeException exception, ParseResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (hostEnvironment.IsDevelopment())
            context.Respond(new HttpResponse(HttpStatusCode.BadRequest, exception.Message));
        else
            context.Respond(new HttpResponse(HttpStatusCode.BadRequest));

        return default;
    }

    /// <inheritdoc />
    protected override ValueTask ParseFailed(IContext context, Exception exception, ParseResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (hostEnvironment.IsDevelopment())
            context.Respond(new HttpResponse(HttpStatusCode.BadRequest, exception.Message));
        else
            context.Respond(new HttpResponse(HttpStatusCode.BadRequest));

        return default;
    }

    /// <inheritdoc />
    protected override ValueTask Unhandled(IContext context, Exception? exception, IResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (hostEnvironment.IsDevelopment() && exception != null)
            context.Respond(new HttpResponse(HttpStatusCode.InternalServerError, exception.Message));
        else
            context.Respond(new HttpResponse(HttpStatusCode.InternalServerError));

        return default;
    }
}
