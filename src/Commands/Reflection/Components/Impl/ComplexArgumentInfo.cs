using Commands.Helpers;
using Commands.TypeConverters;
using System.Diagnostics;
using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a type with a defined complex constructor.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public class ComplexArgumentInfo : IArgument, IArgumentBucket
    {
        /// <summary>
        ///     Gets the invocation target of this complex argument.
        /// </summary>
        public ConstructorInfo Constructor { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public Type ExposedType { get; }

        /// <inheritdoc />
        public bool IsNullable { get; }

        /// <inheritdoc />
        public bool IsOptional { get; }

        /// <inheritdoc />
        public bool IsRemainder { get; }

        /// <inheritdoc />
        public Attribute[] Attributes { get; }

        /// <inheritdoc />
        public IArgument[] Arguments { get; }

        /// <inheritdoc />
        public bool HasArguments { get; }

        /// <inheritdoc />
        public int MinLength { get; }

        /// <inheritdoc />
        public int MaxLength { get; }

        /// <inheritdoc />
        public TypeConverterBase? Converter { get; } = null;

        internal ComplexArgumentInfo(
            ParameterInfo parameterInfo, string? name, CommandBuilder options)
        {
            var underlying = Nullable.GetUnderlyingType(parameterInfo.ParameterType);
            var attributes = parameterInfo.GetAttributes(false);

            if (underlying != null)
            {
                IsNullable = true;
                Type = underlying;
            }
            else
            {
                IsNullable = false;
                Type = parameterInfo.ParameterType;
            }

            if (parameterInfo.IsOptional)
                IsOptional = true;
            else
                IsOptional = false;

            var constructor = Type.GetConstructors()[0];
            var parameters = constructor.GetArguments(false, options);

            if (parameters.Length == 0)
            {
                ThrowHelpers.ThrowInvalidOperation("Complex types are expected to have at least 1 constructor parameter.");
            }

            var (minLength, maxLength) = parameters.GetLength();

            IsRemainder = false;

            MinLength = minLength;
            MaxLength = maxLength;

            Constructor = constructor;
            Arguments = parameters;
            HasArguments = parameters.Length > 0;

            Attributes = attributes.ToArray();

            ExposedType = parameterInfo.ParameterType;

            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }
            else
            {
                Name = parameterInfo.Name ?? "";
            }
        }

        /// <inheritdoc />
        public float GetScore()
        {
            var score = 1.0f;

            if (IsOptional)
                score -= 0.5f;

            if (IsNullable)
                score -= 0.25f;

            foreach (var arg in Arguments)
            {
                score += arg.GetScore();
            }

            return score;
        }

        /// <inheritdoc />
        public override string ToString()
            => ToString(false);

        /// <inheritdoc cref="ToString()"/>
        /// <param name="includeArgumentNames">Defines whether the argument signatures should be named or not.</param>
        public string ToString(bool includeArgumentNames)
        {
            return $"{Type.Name}{(includeArgumentNames ? $" {Name} " : "")}({string.Join<IArgument>(", ", Arguments)})";
        }
    }
}
