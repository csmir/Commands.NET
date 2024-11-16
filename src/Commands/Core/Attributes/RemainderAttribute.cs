using Commands.Converters;

namespace Commands
{
    /// <summary>
    ///     An attribute to define that a final parameter should use the remaining entries in the command query, if any.
    /// </summary>
    /// <remarks>
    ///     <b>This attribute has different behavior depending on what parameter type it is applied to:</b>
    ///     <list type="bullet">
    ///         <item>When set on collection types such as <see cref="List{T}"/> or <see cref="Array"/>, the remaining entries in the query will be concatenated, converted, and inserted into a new collection of that type. Non-generic collection types are not supported.</item>
    ///         <item>When set on a type that implements a <see cref="TypeConverterBase"/>, either custom or default, the remaining entries in the query will be joined into a single string, converted, and passed to the argument.</item>
    ///     </list>
    ///     This implementation is unanimously supported with <see cref="ParamArrayAttribute"/>, which is set on all collections prefixed by <see langword="params"/> by default.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class RemainderAttribute : Attribute
    {

    }
}
