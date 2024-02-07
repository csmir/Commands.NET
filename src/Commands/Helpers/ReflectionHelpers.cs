using Commands.Conditions;
using Commands.Core;
using Commands.Reflection;
using Commands.TypeConverters;
using System.Reflection;

namespace Commands.Helpers
{
    internal static class ReflectionHelpers
    {
        public static IEnumerable<ModuleInfo> BuildComponents(IEnumerable<TypeConverterBase> converters, BuildingContext context)
        {
            var unionConverters = TypeConverterBase.BuildDefaults().UnionBy(converters, x => x.Type).ToDictionary(x => x.Type, x => x);

            var rootType = typeof(ModuleBase);
            foreach (var assembly in context.Assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (rootType.IsAssignableFrom(type)
                        && !type.IsAbstract
                        && !type.ContainsGenericParameters)
                    {
                        yield return new ModuleInfo(type, unionConverters);
                    }
                }
            }
        }

        public static IEnumerable<ModuleInfo> GetModules(ModuleInfo module, IDictionary<Type, TypeConverterBase> converters)
        {
            foreach (var group in module.Type.GetNestedTypes())
            {
                foreach (var attribute in group.GetCustomAttributes(true))
                {
                    if (attribute is GroupAttribute gattribute)
                    {
                        yield return new ModuleInfo(group, converters, module, gattribute.Name, gattribute.Aliases);
                    }
                }
            }
        }

        public static IEnumerable<CommandInfo> GetCommands(ModuleInfo module, IDictionary<Type, TypeConverterBase> typeReaders)
        {
            foreach (var method in module.Type.GetMethods())
            {
                var attributes = method.GetCustomAttributes(true);

                string[] aliases = [];
                foreach (var attribute in attributes)
                {
                    if (attribute is CommandAttribute cmd)
                    {
                        aliases = cmd.Aliases;
                    }
                }

                if (aliases.Length == 0)
                {
                    continue;
                }

                yield return new CommandInfo(module, method, aliases, typeReaders);
            }
        }

        public static IConditional[] GetComponents(this ModuleInfo module, IDictionary<Type, TypeConverterBase> typeReaders)
        {
            var commands = (IEnumerable<IConditional>)GetCommands(module, typeReaders)
                .OrderBy(x => x.Arguments.Length);

            var modules = (IEnumerable<IConditional>)GetModules(module, typeReaders)
                .OrderBy(x => x.Components.Length);

            return commands.Concat(modules)
                .ToArray();
        }

        public static IArgument[] GetParameters(this MethodBase method, IDictionary<Type, TypeConverterBase> typeReaders)
        {
            var parameters = method.GetParameters();

            var arr = new IArgument[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].GetCustomAttributes().Any(x => x is ComplexAttribute))
                {
                    arr[i] = new ComplexArgumentInfo(parameters[i], typeReaders);
                }
                else
                {
                    arr[i] = new ArgumentInfo(parameters[i], typeReaders);
                }
            }

            return arr;
        }

        public static PreconditionAttribute[] GetPreconditions(this Attribute[] attributes)
            => attributes.CastWhere<PreconditionAttribute>().ToArray();

        public static PostconditionAttribute[] GetPostconditions(this Attribute[] attributes)
            => attributes.CastWhere<PostconditionAttribute>().ToArray();

        public static Attribute[] GetAttributes(this ICustomAttributeProvider provider, bool inherit)
            => provider.GetCustomAttributes(inherit).CastWhere<Attribute>().ToArray();

        public static Tuple<int, int> GetLength(this IArgument[] parameters)
        {
            var minLength = 0;
            var maxLength = 0;

            foreach (var parameter in parameters)
            {
                if (parameter is ComplexArgumentInfo complexParam)
                {
                    maxLength += complexParam.MaxLength;
                    minLength += complexParam.MinLength;
                }

                if (parameter is ArgumentInfo defaultParam)
                {
                    maxLength++;
                    if (!defaultParam.IsOptional)
                        minLength++;
                }
            }

            return new(minLength, maxLength);
        }
    }
}
