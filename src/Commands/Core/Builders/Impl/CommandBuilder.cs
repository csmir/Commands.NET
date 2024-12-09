﻿using Commands.Conditions;
using Commands.Reflection;

namespace Commands
{
    /// <summary>
    ///     A base class that represents a delegate based command, before it is built into a reflection-based executable object. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This class is used to configure a command before it is built into a <see cref="CommandInfo"/> object. By calling the <see cref="Build"/> method, the command is built into an object that can be executed by the <see cref="CommandManager"/>>.
    /// </remarks>
    public sealed class CommandBuilder : IComponentBuilder
    {
        private readonly bool _isNested;

        private static readonly Type c_type = typeof(CommandContext<>);

        /// <inheritdoc />
        public string[] Aliases { get; set; } = [];

        /// <summary>
        ///     Gets the conditions necessary for the command to execute.
        /// </summary>
        public List<IExecuteCondition> Conditions { get; } = [];

        /// <summary>
        ///     Gets the delegate that is executed when the command is invoked.
        /// </summary>
        public Delegate ExecuteDelegate { get; set; } = default!;

        /// <summary>
        ///     Creates a new instance of <see cref="CommandBuilder"/>.
        /// </summary>
        public CommandBuilder()
        {

        }

        /// <summary>
        ///     Creates a new instance of <see cref="CommandBuilder"/> with the specified name, aliases, and delegate.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="aliases">The aliases of the command, excluding the name.</param>
        /// <param name="executeDelegate">The delegate used to execute the command.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="executeDelegate"/> is <see langword="null"/>, or when the aliases are <see langword="null"/>.</exception>
        public CommandBuilder(string name, string[] aliases, Delegate executeDelegate)
        {
            if (executeDelegate == null)
                throw new ArgumentNullException(nameof(executeDelegate));

            if (aliases == null)
                throw new ArgumentNullException(nameof(aliases));

            aliases = new string[] { name }
                .Concat(aliases)
                .Distinct()
                .ToArray();

            Aliases = aliases;
            ExecuteDelegate = executeDelegate;

            Conditions = [];
        }

        internal CommandBuilder(Delegate executeDelegate)
            : this(true)
        {
            ExecuteDelegate = executeDelegate;
        }

        internal CommandBuilder(bool isNested)
        {
            _isNested = isNested;
        }

        /// <summary>
        ///     Replaces the current collection of aliases with the specified aliases. Aliases are used to identify the command in the command execution pipeline.
        /// </summary>
        /// <param name="aliases">The aliases of the command.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder WithAliases(params string[] aliases)
        {
            Aliases = aliases;

            return this;
        }

        /// <summary>
        ///     Replaces the current delegate with the specified delegate. The delegate is executed when the command is invoked.
        /// </summary>
        /// <param name="executionDelegate">The delegate to be executed when the command is invoked.</param>
        /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
        public CommandBuilder WithDelegate(Delegate executionDelegate)
        {
            ExecuteDelegate = executionDelegate;

            return this;
        }

        /// <inheritdoc />
        public ISearchable Build(CommandConfiguration configuration)
        {
            if (ExecuteDelegate is null)
                throw new InvalidOperationException("The command must have a delegate to execute.");

            if (!_isNested && Aliases.Length == 0)
                throw new InvalidOperationException("The command must have at least one alias.");

            foreach (var alias in Aliases)
            {
                if (!configuration.NamingRegex.IsMatch(alias))
                    throw new InvalidOperationException($"The alias of must match the filter provided in the {nameof(CommandConfiguration.NamingRegex)} of the {nameof(CommandConfiguration)}.");
            }

            var param = ExecuteDelegate.Method.GetParameters();

            var hasContext = false;

            if (param.Length > 0 && param[0].ParameterType.IsGenericType && param[0].ParameterType.GetGenericTypeDefinition() == c_type)
                hasContext = true;

            return new CommandInfo(new DelegateInvoker(ExecuteDelegate.Method, ExecuteDelegate.Target, hasContext), Aliases, hasContext, configuration);
        }
    }
}
