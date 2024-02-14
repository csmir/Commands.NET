using Commands.Helpers;
using Commands.Reflection;
using Commands.Resolvers;
using Commands.TypeConverters;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A set of options determining the build process for modules and commands.
    /// </summary>
    public class CommandBuilder<T>() : ICommandBuilder
        where T : CommandManager
    {
        const string DEFAULT_REGEX = @"^[a-z0-9_-]*$";

        private static readonly Type c_type = typeof(T);

        private Dictionary<Type, TypeConverterBase>? keyedConverters;

        private readonly List<Action<CommandBuilder<T>>> _commandAdders = [];

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
        public List<ResultResolverBase> ResultResolvers { get; set; } = [];

        /// <inheritdoc />
        public List<CommandInfo> Commands { get; set; } = [];

        /// <inheritdoc />
        /// <remarks>
        ///     Default: <c>@"^[a-z0-9_-]*$"</c>
        /// </remarks>
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'. We don't do this because it can be changed in source.
        public Regex NamingRegex { get; set; } = new(DEFAULT_REGEX, RegexOptions.Compiled);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

        /// <summary>
        ///     Configures an action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ICommandResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="ICommandResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public virtual CommandBuilder<T> AddResultResolver(Action<ConsumerBase, ICommandResult, IServiceProvider> resultAction)
        {
            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            ResultResolvers.Add(new DelegateResolver(resultAction));

            return this;
        }

        /// <summary>
        ///     Configures an asynchronous action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="ICommandResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="ICommandResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public virtual CommandBuilder<T> AddResultResolver(Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> resultAction)
        {
            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            ResultResolvers.Add(new AsyncDelegateResolver(resultAction));

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="ResultResolverBase"/> to the <see cref="ResultResolvers"/>.
        /// </summary>
        /// <typeparam name="TResolver">The implementation type of <see cref="ResultResolverBase"/> to add.</typeparam>
        /// <param name="resolver">The implementation of <see cref="ResultResolverBase"/> to add.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public virtual CommandBuilder<T> AddResultResolver<TResolver>(TResolver resolver)
            where TResolver : ResultResolverBase
        {
            if (resolver == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resolver);
            }

            ResultResolvers.Add(resolver);

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="TypeConverterBase"/> to the <see cref="TypeConverters"/>.
        /// </summary>
        /// <typeparam name="TConverter">The implementation type of <see cref="TypeConverterBase"/> to add.</typeparam>
        /// <param name="converter">The implementation of <see cref="TypeConverterBase"/> to add.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public virtual CommandBuilder<T> AddTypeConverter<TConverter>(TConverter converter)
            where TConverter : TypeConverterBase
        {
            if (converter == null)
            {
                ThrowHelpers.ThrowInvalidArgument(converter);
            }

            TypeConverters[converter.Type] = converter;

            return this;
        }

        public virtual CommandBuilder<T> AddTypeConverter<TConvertable>(Func<ConsumerBase, IArgument, string?, IServiceProvider, ConvertResult> convertAction)
        {
            if (convertAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(convertAction);
            }

            var converter = new DelegateConverter<TConvertable>(convertAction);

            TypeConverters[converter.Type] = converter;

            return this;
        }

        public virtual CommandBuilder<T> AddTypeConverter<TConvertable>(Func<ConsumerBase, IArgument, string?, IServiceProvider, ValueTask<ConvertResult>> convertAction)
        {
            if (convertAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(convertAction);
            }

            var converter = new AsyncDelegateConverter<TConvertable>(convertAction);

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="Assemblies"/> with an additional assembly.
        /// </summary>
        /// <param name="assembly">An assembly that should be added to <see cref="CommandBuilder{T}.Assemblies"/>.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public virtual CommandBuilder<T> AddAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assembly);
            }

            Assemblies.Add(assembly);

            return this;
        }

        /// <summary>
        ///     Configures the <see cref="Assemblies"/> with an additional set of assemblies.
        /// </summary>
        /// <param name="assemblies">A collection of assemblies that should be added to <see cref="CommandBuilder{T}.Assemblies"/>.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public virtual CommandBuilder<T> AddAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assemblies);
            }

            Assemblies.AddRange(assemblies);
            return this;
        }

        /// <summary>
        ///     Adds a new <see cref="Delegate"/> based command to the list of <see cref="Commands"/>.
        /// </summary>
        /// <remarks>
        ///     Delegate commands <b>require</b> the first parameter to be <see cref="CommandContext"/>, which holds scope and execution information of the created command during its execution.
        /// </remarks>
        /// <param name="name">The command name.</param>
        /// <param name="commandAction">The action of the command.</param>
        /// <param name="aliases">The aliases of the command.</param>
        /// <returns>The same <see cref="CommandBuilder{T}"/> for call-chaining.</returns>
        public virtual CommandBuilder<T> AddCommand(string name, Delegate commandAction, params string[] aliases)
        {
            if (commandAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(commandAction);
            }

            if (aliases == null)
            {
                ThrowHelpers.ThrowInvalidArgument(aliases);
            }

            aliases = [name, .. aliases];

            var action = new Action<CommandBuilder<T>>((CommandBuilder<T> options) =>
            {
                foreach (var alias in aliases)
                {
                    if (aliases.Contains(alias))
                    {
                        ThrowHelpers.ThrowNotDistinct(alias);
                    }

                    if (!options.NamingRegex.IsMatch(alias))
                    {
                        ThrowHelpers.ThrowNotMatched(alias);
                    }
                }

                options.Commands.Add(new CommandInfo(new DelegateInvoker(commandAction.Method, commandAction.Target), aliases, options));
            });

            _commandAdders.Add(action);

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
                if (last != null && !last.ParameterType.IsAssignableFrom(typeof(CommandBuilder<>)))
                {
                    continue;
                }

                return (T)target.Invoke([.. args, this]);
            }

            ThrowHelpers.ThrowInvalidOperation($"{c_type.Name} was attempted to be constructed based on a best matching constructor, but no such match was found.");

            return null!;
        }

        /// <summary>
        ///     Finalizes the configuration. This should be called no more than once, as the last step before passing this builder into a manager.
        /// </summary>
        protected void FinalizeConfiguration()
        {
            foreach (var action in _commandAdders)
            {
                action(this);
            }
        }
    }
}
