namespace Commands;

/// <summary>
///     Represents a set of properties to create a new <see cref="ResultHandler"/>
/// </summary>
public interface IResultHandlerBuilder
{
    /// <summary>
    ///     Converts the properties to a new instance of <see cref="ResultHandler"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="ResultHandler"/>.</returns>
    public ResultHandler Build();
}