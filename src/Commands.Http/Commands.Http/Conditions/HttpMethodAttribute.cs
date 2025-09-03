using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to GET requests.
/// </summary>
/// <param name="routeName">Optional route name that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
public class HttpGetAttribute([StringSyntax("Route")] string? routeName = null) : HttpMethodAttribute(GET, routeName)
{
}

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to POST requests.
/// </summary>
/// <param name="routeName">Optional route name that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
public class HttpPostAttribute([StringSyntax("Route")] string? routeName = null) : HttpMethodAttribute(POST, routeName)
{
}

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to PUT requests.
/// </summary>
/// <param name="routeName">Optional route name that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
public class HttpPutAttribute([StringSyntax("Route")] string? routeName = null) : HttpMethodAttribute(PUT, routeName)
{
}

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to DELETE requests.
/// </summary>
/// <param name="routeName">Optional route name that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
public class HttpDeleteAttribute([StringSyntax("Route")] string? routeName = null) : HttpMethodAttribute(DELETE, routeName)
{
}

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to PATCH requests.
/// </summary>
/// <param name="routeName">Optional route name that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
public class HttpPatchAttribute([StringSyntax("Route")] string? routeName = null) : HttpMethodAttribute(PATCH, routeName)
{
}

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command.
/// </summary>
/// <remarks>
///     This attribute can be used to constrain a command to a specific HTTP method, such as GET, POST, PUT, DELETE, etc. Consider using the specific method attributes like <see cref="HttpGetAttribute"/>, <see cref="HttpPostAttribute"/>, etc., for better readability and clarity if possible.
/// </remarks>
/// <param name="method">The name of the HTTP method required to execute the provided operation.</param>
/// <param name="routeName">Optional route name that can be used to match the command to specific routes.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
public class HttpMethodAttribute(string method, [StringSyntax("Route")] string? routeName = null) : HttpConditionAttribute<HttpMethodEvaluator>, INameBinding
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
    public string[] Names { get; } = !string.IsNullOrEmpty(routeName) ? [routeName] : [];

    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(HttpCommandContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (context.Request.HttpMethod.Equals(method, StringComparison.InvariantCultureIgnoreCase))
            return ConditionResult.FromSuccess();

        return ConditionResult.FromError(new HttpConditionException(new HttpResult(HttpStatusCode.MethodNotAllowed, Encoding.UTF8.GetBytes($"Provided HTTP method ({context.Request.HttpMethod}) is not allowed for this operation."))));
    }
}