namespace Commands;

/// <summary>
///     An attribute to mark a command parameter as a dependency, which will be resolved from the service provider when the command is executed.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class DependencyAttribute : Attribute
{

}