using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to GET requests.
/// </summary>
/// <param name="routeNames">Optional route names that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpGetAttribute(params string[] routeNames)
    : HttpMethodAttribute(GET, routeNames)
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to POST requests.
/// </summary>
/// <param name="routeNames">Optional route names that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpPostAttribute(params string[] routeNames)
    : HttpMethodAttribute(POST, routeNames)
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to PUT requests.
/// </summary>
/// <param name="routeNames">Optional route names that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpPutAttribute(params string[] routeNames)
    : HttpMethodAttribute(PUT, routeNames)
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to DELETE requests.
/// </summary>
/// <param name="routeNames">Optional route names that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpDeleteAttribute(params string[] routeNames)
    : HttpMethodAttribute(DELETE, routeNames)
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to PATCH requests.
/// </summary>
/// <param name="routeNames">Optional route names that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpPatchAttribute(params string[] routeNames)
    : HttpMethodAttribute(PATCH, routeNames)
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command.
/// </summary>
/// <remarks>
///     This attribute can be used to constrain a command to a specific HTTP method, such as GET, POST, PUT, DELETE, etc. Consider using the specific method attributes like <see cref="HttpGetAttribute"/>, <see cref="HttpPostAttribute"/>, etc., for better readability and clarity if possible.
/// </remarks>
/// <param name="method">The name of the HTTP method required to execute the provided operation.</param>
/// <param name="routeNames">Optional route names that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpMethodAttribute(string method, params string[] routeNames) : HttpConditionAttribute<HttpMethodEvaluator>, INameBinding
{
    /// <summary>
    ///     Gets the GET http method name.
    /// </summary>
    public const string GET = "GET";

    /// <summary>
    ///     Gets the POST http method name.
    /// </summary>
    public const string POST = "POST";

    /// <summary>
    ///     Gets the PUT http method name.
    /// </summary>
    public const string PUT = "PUT";

    /// <summary>
    ///     Gets the DELETE http method name.
    /// </summary>
    public const string DELETE = "DELETE";

    /// <summary>
    ///     Gets the PATCH http method name.
    /// </summary>
    public const string PATCH = "PATCH";

    /// <inheritdoc />
    public string Name
        => Names.Length > 0 ? Names[0] : string.Empty;

    /// <inheritdoc />
    public string[] Names { get; } = routeNames;

    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(HttpCommandContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (context.Request.HttpMethod.Equals(method, StringComparison.InvariantCultureIgnoreCase))
            return ConditionResult.FromSuccess();

        return ConditionResult.FromError(new HttpConditionException(this, new HttpResult(HttpStatusCode.MethodNotAllowed, Encoding.UTF8.GetBytes($"Provided HTTP method ({context.Request.HttpMethod}) is not allowed for this operation."))));
    }
}