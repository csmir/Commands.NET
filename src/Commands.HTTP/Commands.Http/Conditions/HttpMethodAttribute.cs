using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to GET requests.
/// </summary>
public class HttpGetAttribute()
    : HttpMethodAttribute("GET")
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to POST requests.
/// </summary>
public class HttpPostAttribute()
    : HttpMethodAttribute("POST")
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to PUT requests.
/// </summary>
public class HttpPutAttribute()
    : HttpMethodAttribute("PUT")
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to DELETE requests.
/// </summary>
public class HttpDeleteAttribute()
    : HttpMethodAttribute("DELETE")
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command, constraining it to PATCH requests.
/// </summary>
public class HttpPatchAttribute()
    : HttpMethodAttribute("PATCH")
{ }

/// <summary>
///     Represents an attribute that can be used to specify the HTTP method for a command.
/// </summary>
/// <remarks>
///     This attribute can be used to constrain a command to a specific HTTP method, such as GET, POST, PUT, DELETE, etc. Consider using the specific method attributes like <see cref="HttpGetAttribute"/>, <see cref="HttpPostAttribute"/>, etc., for better readability and clarity if possible.
/// </remarks>
/// <param name="method">The name of the HTTP method required to execute the provided operation.</param>
[AttributeUsage(AttributeTargets.Method)]
public class HttpMethodAttribute(string method) : HttpConditionAttribute<OREvaluator>
{
    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(HttpCommandContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (context.Request.HttpMethod.Equals(method, StringComparison.InvariantCultureIgnoreCase))
            return Success();

        return Error(new HttpResponse(HttpStatusCode.MethodNotAllowed, Encoding.UTF8.GetBytes($"Provided HTTP method ({context.Request.HttpMethod}) is not allowed for this operation.")));
    }
}