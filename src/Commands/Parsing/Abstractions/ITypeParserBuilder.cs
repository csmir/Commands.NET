using System.ComponentModel;

namespace Commands.Parsing;

/// <summary>
///     Represents a set of properties of a <see cref="TypeParser"/>
/// </summary>
public interface ITypeParserBuilder
{
    /// <summary>
    ///     Converts the properties to a new instance of <see cref="TypeParser"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="TypeParser"/>.</returns>
    public TypeParser Build();

    /// <summary>
    ///     Gets the target type of the parser to create.
    /// </summary>
    /// <returns>A type which the parser targets.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Type GetParserType();
}