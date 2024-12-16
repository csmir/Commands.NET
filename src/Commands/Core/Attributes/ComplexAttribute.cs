namespace Commands
{
    /// <summary>
    ///     An attribute to mark a command parameter as complex. 
    ///     A complex parameter replaces the parameter with the best matching constructor of the parameter type this attribute marks.
    /// </summary>
    /// <remarks>
    ///     Complex parameter constructors are picked based on length, skipping constructors marked with <see cref="NoBuildAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ComplexAttribute : Attribute
    {

    }
}
