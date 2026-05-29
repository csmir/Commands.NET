namespace Commands;

/// <summary>
///     Represents a context that can be used to store a resource associated with the command execution, which can be accessed through method parameters marked with <see cref="IMetadataBinding"/>.
/// </summary>
public interface IMetadataContext : IContext
{
    /// <summary>
    ///     Gets or sets a resource object, which can be injected into command parameters marked with <see cref="IMetadataBinding"/>.
    /// </summary>
    public ValueTask<object?> GetMetadata();
}
