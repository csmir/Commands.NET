using Commands.Conversion;
using System.Text.RegularExpressions;

namespace Commands.Builders
{
    /// <summary>
    ///     Represents a builder that creates read-only configuration models for creating executable components.
    /// </summary>
    public interface IConfigurationBuilder
    {
        /// <summary>
        ///     Gets or sets the naming convention of commands and groups being registered into the <see cref="ComponentTree"/>.
        /// </summary>
        public Regex? NamingPattern { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="TypeParser"/>, each containing an operation for <see cref="Type"/> parsing. Parsing of <see langword="object"/>, <see langword="string"/>, implementations of <see langword="enum"/> and implementations of <see cref="IEnumerable{T}"/> are not necessary to implement, as they are handled internally by the <see cref="ComponentTree"/>.
        /// </summary>
        /// <remarks>
        ///     This collection is set by default to the value of <see cref="TypeParser.CreateDefaults"/>. 
        ///     Mutation to existing parsers or adding new parsers can be done through the methods provided by this interface.
        ///     By default, any new parsers added will replace existing parsers with the same <see cref="Type"/>. 
        ///     <br />
        ///     <br />
        ///     The list of types that are converted by the default parsers are:
        ///     <list type="bullet">
        ///         <item>String characters, being: <see langword="char"/>.</item>
        ///         <item>Integers variants, being: <see langword="bool"/>, <see langword="byte"/>, <see langword="sbyte"/>, <see langword="short"/>, <see langword="ushort"/>, <see langword="int"/>, <see langword="uint"/>, <see langword="long"/> and <see langword="ulong"/>.</item>
        ///         <item>Floating point numbers, being: <see langword="float"/>, <see langword="decimal"/> and <see langword="double"/>.</item>
        ///         <item>Commonly used structs, being: <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="TimeSpan"/> and <see cref="Guid"/>.</item>
        ///     </list>
        ///     <i>The parser for <see cref="TimeSpan"/> does not implement <see cref="TryParseParser"/>, instead having extended logic under <see cref="TimeSpanParser"/>.</i>
        /// </remarks>
        public Dictionary<Type, TypeParser> Parsers { get; set; }

        /// <summary>
        ///     Gets or sets a collection of properties that can be used to store additional information applied during the build process.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        ///     Configures an action that will convert a raw argument into the target type, signified by <typeparamref name="TParsable"/>.
        /// </summary>
        /// <typeparam name="TParsable">The type for this parser to target.</typeparam>
        /// <param name="convertAction">The action that is responsible for the parsing process.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> for call-chaining.</returns>
        public IConfigurationBuilder AddParser<TParsable>(Func<CallerContext, IArgument, object?, IServiceProvider, ConvertResult> convertAction);

        /// <summary>
        ///     Configures an asynchronous action that will convert a raw argument into the target type, signified by <typeparamref name="TParsable"/>.
        /// </summary>
        /// <typeparam name="TParsable">The type for this parser to target.</typeparam>
        /// <param name="convertAction">The action that is responsible for the parsing process.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> for call-chaining.</returns>
        public IConfigurationBuilder AddParser<TParsable>(Func<CallerContext, IArgument, object?, IServiceProvider, ValueTask<ConvertResult>> convertAction);

        /// <summary>
        ///     Adds an implementation of <see cref="TypeParser"/> to <see cref="Parsers"/>.
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
