using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Commands
{
    /// <summary>
    ///     Represents an activator that can create an instance of a complex type, being a parameter marked with <see cref="ComplexAttribute"/>.
    /// </summary>
    public sealed class ComplexActivator : IActivator
    {
        /// <inheritdoc />
        public MethodBase Target { get; }

        internal ComplexActivator(Type type)
        {
            var ctor = type.GetInvokableConstructor();

            Target = ctor;
        }

        /// <inheritdoc />
        public object? Invoke<T>(T caller, CommandInfo? command, object?[] args, IComponentTree? tree, CommandOptions options) where T : ICallerContext
            => Target.Invoke(null, args);

        /// <inheritdoc />
#if NET8_0_OR_GREATER
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
        public Type? GetReturnType()
            => default;
    }
}
