using Commands.Core;
using Commands.Helpers;
using Commands.TypeConverters;
using System.Reflection;

namespace Commands.Reflection
{
    /// <inheritdoc />
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
        public TypeConverterBase? Converter { get; } = null;

        internal ArgumentInfo(
            ParameterInfo parameterInfo, BuildOptions options)
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
            {
                IsOptional = true;
            }
            else
            {
                IsOptional = false;
            }

            if (attributes.Contains<RemainderAttribute>(false))
            {
                IsRemainder = true;
            }
            else
            {
                IsRemainder = false;
            }

            if (Type.IsEnum)
            {
                Converter = EnumTypeReader.GetOrCreate(Type);
            }

            else if (Type != typeof(string) && Type != typeof(object))
            {
                Converter = options.KeyedConverters[Type];
            }

            Attributes = attributes;
            ExposedType = parameterInfo.ParameterType;
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
        public override string ToString()
            => ToString(false);

        /// <inheritdoc cref="ToString()"/>
        /// <param name="includeArgumentNames">Defines whether the argument signatures should be named or not.</param>
        public string ToString(bool includeArgumentNames)
        {
            var str = Type.Name;

            if (includeArgumentNames)
            {
                str += $" {Name}";
            }

            return str;
        }
    }
}
