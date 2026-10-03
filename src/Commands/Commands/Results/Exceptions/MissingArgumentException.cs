namespace Commands;

/// <summary>
///     Represents an exception that is created when no value is provided for a required parameter. This exception is the inner exception of a <see cref="CommandOutOfRangeException"/>.
/// </summary>
/// <param name="parameterName">The name of the parameter that did not receive a value.</param>
public sealed class MissingArgumentException(string? parameterName)
    : Exception($"No value was provided for the required parameter '{parameterName}'.")
{
    /// <summary>
    ///     Gets the name of the parameter that did not receive a value.
    /// </summary>
    public string? ParameterName { get; } = parameterName;
}
