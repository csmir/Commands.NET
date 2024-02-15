namespace Commands
{
    /// <summary>
    ///     An attribute that signifies that a target should <b>not</b> be considered in registration.
    /// </summary>
    /// <remarks>
    ///     This attribute can be marked on modules, commands and module/manager constructors.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class SkipAttribute : Attribute
    {

    }
}
