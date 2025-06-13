namespace Commands;

/// <summary>
///     An interface that marks a command parameter as a remainder binding, which will consume the remaining entries in the command query, if any.
/// </summary>
/// <remarks>
///     To use this binding, implement <see cref="RemainderAttribute"/> directly, or consume another custom attribute that implements this interface.
/// </remarks>
public interface IRemainderBinding
{
}
