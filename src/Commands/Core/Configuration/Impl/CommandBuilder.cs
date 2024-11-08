using Commands.Helpers;
using Commands.Reflection;
using Commands.Resolvers;
using Commands.TypeConverters;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A container for options determining the build process for modules and commands.
    /// </summary>
    public class CommandBuilder<T> : ICommandBuilder
        where T : CommandManager
    {
        const string DEFAULT_REGEX = @"^[a-z0-9_-]*$";

        private static readonly Type c_type = typeof(CommandContext<>);
        private static readonly Type t_type = typeof(CommandBuilder<>);

        private readonly List<Action<ICommandBuilder>> _cmdAddChain = [];

        /// <inheritdoc />
        public List<ResultResolverBase> ResultResolvers { get; set; } = [];

        /// <inheritdoc />
        public List<CommandInfo> Commands { get; set; } = [];

        /// <inheritdoc />
        /// <remarks>
        ///     Default: <see cref="Assembly.GetEntryAssembly"/>
        /// </remarks>
        public List<Assembly> Assemblies { get; set; } = [Assembly.GetEntryAssembly()!]; // never null in managed context.

        /// <inheritdoc />
        /// <remarks>
        ///     Default: <see cref="TypeConverterBase.BuildDefaults"/>
        /// </remarks>
        public Dictionary<Type, TypeConverterBase> TypeConverters { get; set; } = TypeConverterBase.BuildDefaults();

        /// <inheritdoc />
        /// <remarks>
        ///     Default: <c>@"^[a-z0-9_-]*$"</c>
        /// </remarks>
        public Regex NamingRegex { get; set; } = new(DEFAULT_REGEX, RegexOptions.Compiled);

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the list of <see cref="CommandBuilder{T}.Commands"/>.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution.</param>
        /// <param name="aliases">The aliases of the command.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public CommandBuilder<T> AddCommand([DisallowNull] string name, [DisallowNull] Delegate commandAction, params string[] aliases)
        {
            if (commandAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(commandAction);
            }

            if (aliases == null)
            {
                ThrowHelpers.ThrowInvalidArgument(aliases);
            }

            aliases = new string[] { name }
                .Concat(aliases)
                .Distinct()
                .ToArray();

            var action = new Action<ICommandBuilder>((ICommandBuilder options) =>
            {
                foreach (var alias in aliases)
                {
                    if (!options.NamingRegex.IsMatch(alias))
                    {
                        ThrowHelpers.ThrowNotMatched(alias);
                    }
                }

                var param = commandAction.Method.GetParameters();

                var hasContext = false;
                if (param.Length > 0 && param[0].ParameterType.IsGenericType && param[0].ParameterType.GetGenericTypeDefinition() == c_type)
                {
                    hasContext = true;
                }

                options.Commands.Add(new CommandInfo(new DelegateInvoker(commandAction.Method, commandAction.Target, hasContext), aliases, hasContext, options));
            });

            _cmdAddChain.Add(action);

            return this;
        }

        /// <summary>
        ///     Builds a new instance of <typeparamref name="T"/> based on the current configuration of this builder.
        /// </summary>
        /// <remarks>
        ///     This method will finalize the configured values within this builder and continue onto constructor matching. 
        ///     Constructor matching will order all found constructors based on their parameter length. It will skip a constructor when:
        ///     <list type="bullet">
        ///         <item>It is marked with <see cref="SkipAttribute"/>.</item>
        ///         <item>It does <b>NOT</b> have <see cref="CommandBuilder{T}"/> as its last parameter.</item>
        ///     </list>
        ///     If none of the above conditions are met for any of the public constructors of <typeparamref name="T"/>, this operation will throw.
        /// </remarks>
        /// <param name="args">Constructor arguments to pass alongside the builder. Can be ignored if the default <see cref="CommandManager"/> is used instead of a custom implementation.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when none of the filter conditions are met.</exception>
        public virtual T Build(params object[] args)
        {
            FinalizeConfiguration();

            var ctors = typeof(T).GetConstructors()
                .Select(x => (target: x, param: x.GetParameters()))
                .OrderByDescending(x => x.param.Length);

            foreach (var (target, param) in ctors)
            {
                // skip if skipattribute is present on ctor.
                if (target.GetCustomAttributes().Any(x => x is SkipAttribute skip))
                {
                    continue;
                }

                var last = param.LastOrDefault();

                // skip if last param is not CommandBuilder<T>.
                if (last != null && !last.ParameterType.IsAssignableFrom(t_type))
                {
                    continue;
                }

                return (T)target.Invoke([.. args, this]);
            }

            ThrowHelpers.ThrowInvalidOperation($"{typeof(T).Name} was attempted to be constructed based on a best matching constructor, but no such match was found.");

            return null!;
        }

        /// <summary>
        ///     Finalizes the configuration. This should not be called more than once, as the last step before passing this builder into a manager.
        /// </summary>
        protected void FinalizeConfiguration()
        {
            foreach (var action in _cmdAddChain)
            {
                action(this);
            }
        }
    }

    /// <summary>
    ///     A container for options determining the build process for modules and commands.
    /// </summary>
    public static class CommandBuilder
    {
        /// <summary>
        ///     Configures an action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ICommandResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="ICommandResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <typeparam name="TBuilder">The <see cref="CommandBuilder{T}"/> type to modify.</typeparam>
        /// <param name="builder">The <see cref="CommandBuilder{T}"/> implementation to modify.</param>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public static TBuilder AddResultResolver<TBuilder>(this TBuilder builder, [DisallowNull] Action<ConsumerBase, ICommandResult, IServiceProvider> resultAction)
            where TBuilder : ICommandBuilder, new()
        {
            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            builder.ResultResolvers.Add(new DelegateResolver(resultAction));

            return builder;
        }

        /// <summary>
        ///     Configures an asynchronous action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ICommandResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="ICommandResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <typeparam name="TBuilder">The <see cref="CommandBuilder{T}"/> type to modify.</typeparam>
        /// <param name="builder">The <see cref="CommandBuilder{T}"/> implementation to modify.</param>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public static TBuilder AddResultResolver<TBuilder>(this TBuilder builder, [DisallowNull] Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> resultAction)
            where TBuilder : ICommandBuilder, new()
        {
            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            builder.ResultResolvers.Add(new AsyncDelegateResolver(resultAction));

            return builder;
        }

        /// <summary>
        ///     Adds an implementation of <see cref="ResultResolverBase"/> to <see cref="CommandBuilder{T}.ResultResolvers"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The <see cref="CommandBuilder{T}"/> type to modify.</typeparam>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add.</typeparam>
        /// <param name="builder">The <see cref="CommandBuilder{T}"/> implementation to modify.</param>
        /// <param name="resolver">The implementation of <see cref="ResultResolverBase"/> to add.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public static TBuilder AddResultResolver<TBuilder, TResolver>(this TBuilder builder, [DisallowNull] TResolver resolver)
            where TBuilder : ICommandBuilder, new()
            where TResolver : ResultResolverBase
        {
            if (resolver == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resolver);
            }

            builder.ResultResolvers.Add(resolver);

            return builder;
        }

        /// <summary>
        ///     Configures an action that will convert a raw argument into the target type, signified by <typeparamref name="TConvertable"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The <see cref="CommandBuilder{T}"/> type to modify.</typeparam>
        /// <typeparam name="TConvertable">The type for this converter to target.</typeparam>
        /// <param name="builder">The <see cref="CommandBuilder{T}"/> implementation to modify.</param>
        /// <param name="convertAction">The action that is responsible for the conversion process.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public static TBuilder AddTypeConverter<TBuilder, TConvertable>(this TBuilder builder, [DisallowNull] Func<ConsumerBase, IArgument, string?, IServiceProvider, ConvertResult> convertAction)
            where TBuilder : ICommandBuilder, new()
        {
            if (convertAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(convertAction);
            }

            var converter = new DelegateConverter<TConvertable>(convertAction);

            builder.TypeConverters[converter.Type] = converter;

            return builder;
        }

        /// <summary>
        ///     Configures an asynchronous action that will convert a raw argument into the target type, signified by <typeparamref name="TConvertable"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The <see cref="CommandBuilder{T}"/> type to modify.</typeparam>
        /// <typeparam name="TConvertable">The type for this converter to target.</typeparam>
        /// <param name="builder">The <see cref="CommandBuilder{T}"/> implementation to modify.</param>
        /// <param name="convertAction">The action that is responsible for the conversion process.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public static TBuilder AddTypeConverter<TBuilder, TConvertable>(this TBuilder builder, [DisallowNull] Func<ConsumerBase, IArgument, string?, IServiceProvider, ValueTask<ConvertResult>> convertAction)
            where TBuilder : ICommandBuilder, new()
        {
            if (convertAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(convertAction);
            }

            var converter = new AsyncDelegateConverter<TConvertable>(convertAction);

            builder.TypeConverters[converter.Type] = converter;

            return builder;
        }

        /// <summary>
        ///     Adds an implementation of <see cref="TypeConverterBase"/> to <see cref="CommandBuilder{T}.TypeConverters"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The <see cref="CommandBuilder{T}"/> type to modify.</typeparam>
        /// <typeparam name="TConverter">The implementation type of <see cref="TypeConverterBase"/> to add.</typeparam>
        /// <param name="builder">The <see cref="CommandBuilder{T}"/> implementation to modify.</param>
        /// <param name="converter">The implementation of <see cref="TypeConverterBase"/> to add.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public static TBuilder AddTypeConverter<TBuilder, TConverter>(this TBuilder builder, [DisallowNull] TConverter converter)
            where TBuilder : ICommandBuilder, new()
            where TConverter : TypeConverterBase
        {
            if (converter == null)
            {
                ThrowHelpers.ThrowInvalidArgument(converter);
            }

            builder.TypeConverters[converter.Type] = converter;

            return builder;
        }

        /// <summary>
        ///     Configures the <see cref="CommandBuilder{T}.Assemblies"/> with an additional assembly.
        /// </summary>
        /// <typeparam name="TBuilder">The <see cref="CommandBuilder{T}"/> type to modify.</typeparam>
        /// <param name="builder">The <see cref="CommandBuilder{T}"/> implementation to modify.</param>
        /// <param name="assembly">An assembly that should be added to <see cref="CommandBuilder{T}.Assemblies"/>.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public static TBuilder AddAssembly<TBuilder>(this TBuilder builder, [DisallowNull] Assembly assembly)
            where TBuilder : ICommandBuilder, new()
        {
            if (assembly == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assembly);
            }

            builder.Assemblies.Add(assembly);

            return builder;
        }

        /// <summary>
        ///     Configures the <see cref="CommandBuilder{T}.Assemblies"/> with an additional set of assemblies.
        /// </summary>
        /// <typeparam name="TBuilder">The <see cref="CommandBuilder{T}"/> type to modify.</typeparam>
        /// <param name="builder">The <see cref="CommandBuilder{T}"/> implementation to modify.</param>
        /// <param name="assemblies">A collection of assemblies that should be added to <see cref="CommandBuilder{T}.Assemblies"/>.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public static TBuilder AddAssemblies<TBuilder>(this TBuilder builder, params Assembly[] assemblies)
            where TBuilder : ICommandBuilder, new()
        {
            if (assemblies == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assemblies);
            }

            builder.Assemblies.AddRange(assemblies);
            return builder;
        }
    }
}
