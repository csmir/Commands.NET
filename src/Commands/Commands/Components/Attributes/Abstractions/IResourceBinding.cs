namespace Commands;

/// <summary>
///     An interface that notifies that a parameter is a resource, and that it should be accessed through a <see cref="IResourceContext"/> rather than the context arguments.
/// </summary>
/// <remarks>
///     To use this binding, implement <see cref="ResourceAttribute"/> directly, or consume another custom attribute that implements this interface.
/// </remarks>
public interface IResourceBinding
{
}
