namespace Commands
{
    /// <summary>
    ///     An attribute that signifies that a target should <b>not</b> be used in execution pattern.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class SkipAttribute : Attribute
    {

    }
}
