namespace Commands;

/// <summary>
///     An attribute to mark a command parameter with input continuation, deconstructing its first available constructor to become part of the command signature. This class cannot be inherited.
/// </summary>
/// <remarks>
///     Constructible parameter constructors are picked based on Priority, or top to bottom if none, skipping constructors marked with <see cref="IgnoreAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class DeconstructAttribute : Attribute
{
}
