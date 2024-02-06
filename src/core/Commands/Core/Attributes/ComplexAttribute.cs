namespace Commands.Core
{
    /// <summary>
    ///     An attribute to mark a parameter as complex, which will attempt to fetch the primary constructor values and use those as command parameters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ComplexAttribute : Attribute
    {

    }
}
