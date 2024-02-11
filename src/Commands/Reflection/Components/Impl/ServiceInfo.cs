using Commands.Helpers;
using Commands.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a service parameter.
    /// </summary>
    public sealed class ServiceInfo : IParameter
    {
        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public Type ExposedType { get; }

        /// <inheritdoc />
        public bool IsNullable { get; }

        /// <inheritdoc />
        public bool IsOptional { get; }

        internal ServiceInfo(
            ParameterInfo parameterInfo)
        {
            var underlying = Nullable.GetUnderlyingType(parameterInfo.ParameterType);

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

            ExposedType = parameterInfo.ParameterType;
        }
    }
}
