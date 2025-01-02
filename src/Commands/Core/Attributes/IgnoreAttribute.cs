namespace Commands;

/// <summary>
///     An attribute that signifies that a target should <b>not</b> be considered in registration. This class cannot be inherited.
/// </summary>
/// <remarks>
///     This attribute can be marked on implementations of <see cref="CommandModule"/>, declared commands and <see cref="CommandModule"/> constructors. 
///     When marked, the target will not be considered in the registration process.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
public sealed class IgnoreAttribute() : Attribute
{

}
