using Commands.Conversion;

namespace Commands.Builders
{
    /// <summary>
    ///     Represents a builder that creates read-only configuration models for creating executable components.
    /// </summary>
    public interface IConfigurationBuilder
    {
        /// <summary>
        ///     Gets or sets the <see cref="TypeParser"/> collection to use when constructing components with this configuration.
        /// </summary>
        /// <remarks>
        ///     This collection is set by default to the value of <see cref="TypeParser.CreateDefaults"/>.
        ///     Mutation to existing parsers or adding new parsers can be done through the methods provided by this interface.
        ///     Any new parsers added will replace existing parsers with the same <see cref="Type"/>. By default, this collection contains parsers for:
        ///     <list type="bullet">
        ///         <item>All BCL types (<see href="https://learn.microsoft.com/en-us/dotnet/standard/class-library-overview#system-namespace"/>).</item>
        ///         <item><see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="TimeSpan"/> and <see cref="Guid"/>.</item>
        ///         <item><see cref="Enum"/> implementations for which no custom parser exists.</item>
        ///     </list>
        ///     <i>Collections implementing <see cref="Array"/> are converted by their respective element types, and not the types themselves.</i>
        /// </remarks>
        public IDictionary<Type, TypeParser> Parsers { get; set; }

        /// <summary>
        ///     Gets or sets a collection of properties that can be used to store additional information applied during the build process.
        /// </summary>
        public IDictionary<object, object> Properties { get; set; }

        /// <summary>
        ///     Configures an action that will convert a raw argument into the target type, signified by <typeparamref name="TParsable"/>.
        /// </summary>
        /// <remarks>
        ///     Any existing <see cref="TypeParser"/> implementing the same <see cref="Type"/> as <typeparamref name="TParsable"/> will be replaced by this operation.
        /// </remarks>
        /// <typeparam name="TParsable">The type for this parser to target.</typeparam>
        /// <param name="convertAction">The action that is responsible for the parsing process.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> for call-chaining.</returns>
        public IConfigurationBuilder AddParser<TParsable>(Func<ICallerContext, IArgument, object?, IServiceProvider, ValueTask<ParseResult>> convertAction);

        /// <summary>
        ///     Adds an implementation of <see cref="TypeParser"/> to <see cref="Parsers"/>, replacing an existing parser with the same <see cref="Type"/>.
        /// </summary>
        /// <param name="parser">The implementation of <see cref="TypeParser"/> to add.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> for call-chaining.</returns>
        public IConfigurationBuilder AddParser(TypeParser parser);

        /// <summary>
        ///     Adds a collection of <see cref="TypeParser"/>'s to the current <see cref="Parsers"/>, replacing any existing parsers with the same <see cref="Type"/>.
        /// </summary>
        /// <param name="parsers">A collection of parsers to add or replace in <see cref="Parsers"/>.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> for call-chaining.</returns>
        public IConfigurationBuilder AddParsers(params IEnumerable<TypeParser> parsers);

        /// <summary>
        ///     Replaces the current collection of type parsers with the specified parsers.
        /// </summary>
        /// <param name="parsers">A collection of parsers to replace the existing parsers in <see cref="Parsers"/>.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> for call-chaining.</returns>
        public IConfigurationBuilder WithParsers(params IEnumerable<TypeParser> parsers);

        /// <summary>
        ///     Builds the current <see cref="IConfigurationBuilder"/> into a new instance of <see cref="ComponentConfiguration"/>.
        /// </summary>
        /// <returns>A newly created instance of <see cref="ComponentConfiguration"/></returns>
        public ComponentConfiguration Build();
    }
}
