namespace Commands
{
    /// <summary>
    ///     An attribute to define that a final parameter should use the remainder of the command query, joining together each of the parsed arguments into one string.
    /// </summary>
    /// <remarks>
    ///     This attribute can only be set on the last parameter of a command method.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class RemainderAttribute : Attribute
    {

    }
}
