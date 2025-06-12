using Commands.Parsing;

namespace Commands.Http;

/// <summary>
///     An attribute that indicates that the parameter should be parsed from the request body as JSON.
/// </summary>
/// <remarks>
///     By marking a parameter with this attribute, the command parser will attempt to deserialize the request body into the specified type using JSON deserialization using the default <see cref="JsonSerializerOptions"/>.
///     <br/>
///     This component is not Native-AOT compatible without implementing <see cref="JsonSerializerContext"/> and providing the target types for deserialization to it.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class JsonBodyAttribute : TypeParserAttribute, IResourceBinding
{
    /// <inheritdoc />
    [UnconditionalSuppressMessage("AOT", "IL3050")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "End user can define custom JsonSerializerContext that has the required TypeInfo for the target type.")]
    public override ValueTask<ParseResult> Parse(IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (argument is not string json)
            return Error("The provided argument is not a valid JSON string.");

        try
        {
            return Success(JsonSerializer.Deserialize(json, parameter.Type));
        }
        catch (JsonException ex)
        {
            return Error(ex.Message);
        }
    }
}