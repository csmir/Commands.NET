using Commands.Conversion;
using System.Diagnostics;
using System.Reflection;

namespace Commands
{
    /// <inheritdoc />
    [DebuggerDisplay("{ToString()}")]
    public sealed class ArgumentInfo : IArgument
    {
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
        public TypeParser? Converter { get; } = null;

        /// <inheritdoc />
        public bool IsCollection
            => Converter is ICollectionConverter;

        internal ArgumentInfo(
            ParameterInfo parameterInfo, string? name, ComponentConfiguration options)
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

            if (attributes.ContainsAttribute<RemainderAttribute>(false) || attributes.ContainsAttribute<ParamArrayAttribute>(false))
                IsRemainder = true;
            else
                IsRemainder = false;

            var converter = ComponentUtilities.GetTypeConverter(Type, options);

            Converter = converter;
            ExposedType = parameterInfo.ParameterType;
            Attributes = attributes.ToArray();

            if (!string.IsNullOrEmpty(name))
                Name = name;
            else
                Name = parameterInfo.Name ?? "";
        }

        /// <inheritdoc />
        public float GetScore()
        {
            var score = 1.0f;

            if (IsOptional)
                score -= 0.5f;

            if (IsRemainder)
                score -= 0.25f;

            if (IsNullable)
                score -= 0.25f;

            if (Converter != null)
                score += 0.5f;

            return score;
        }

        /// <inheritdoc />
        public int CompareTo(object obj)
            => obj is IScoreable scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

        /// <inheritdoc />
        public override string ToString()
            => ToString(false);

        /// <inheritdoc cref="ToString()"/>
        /// <param name="includeArgumentNames">Defines whether the argument signatures should be named or not.</param>
        public string ToString(bool includeArgumentNames)
            => $"{Type.Name}{(includeArgumentNames ? Name : "")}";
    }
}
