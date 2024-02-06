namespace Commands.Core
{
    /// <summary>
    ///     An attribute that sets a specified constructor as the dependency injection constructor.
    /// </summary>
    /// <remarks>
    ///     It is not intended to use this attribute on multiple constructors. If it is, it will pick the highest constructor specified with this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class PrimaryConstructorAttribute : Attribute
    {

    }
}
