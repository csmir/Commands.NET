namespace Commands;

/// <summary>
///     An attribute to mark a command parameter as a resource, which will inject the resource from the available <see cref="IResourceContext"/>, if possible, into the parameter during command execution.
/// </summary>
/// <remarks>
///     Ensure to use this attribute only on parameters inside signatures that have access to an implementation of <see cref="IResourceContext"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class ResourceAttribute : Attribute, IResourceBinding
{ 
}