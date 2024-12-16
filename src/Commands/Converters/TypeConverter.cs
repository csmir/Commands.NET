using Commands.Reflection;

namespace Commands.Converters
{
    /// <inheritdoc />
    /// <typeparam name="T">The type this <see cref="TypeConverter{T}"/> should convert into.</typeparam>
    public abstract class TypeConverter<T> : TypeConverter
    {
        /// <summary>
        ///     Gets the type that should be converted to.
        /// </summary>
        public override Type Type { get; } = typeof(T);

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> representing a successful evaluation.
        /// </summary>
        /// <param name="value">The value converted from a raw argument into the target type of this converter.</param>
        /// <returns>A <see cref="ConvertResult"/> representing the successful evaluation.</returns>
        public virtual ConvertResult Success(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return base.Success(value);
        }
    }

    /// <summary>
    ///     An abstract type that can be implemented to create custom type conversion from a command query argument.
    /// </summary>
    /// <remarks>
    ///     To register converters for the <see cref="CommandTree"/> to use, add them to the <see cref="CommandTreeBuilder.TypeConverters"/> collection.
    /// </remarks>
    public abstract class TypeConverter
    {
        /// <summary>
        ///     Gets the type that should be converted to. This value determines what command arguments will use this converter.
        /// </summary>
        /// <remarks>
        ///     It is important to ensure this converter actually returns the specified type in <see cref="Success(object)"/>. If this is not the case, a critical exception will occur in runtime when the command is attempted to be executed.
        /// </remarks>
        public abstract Type Type { get; }

        /// <summary>
        ///     Evaluates the known data about the argument to be converted into, as well as the raw value it should convert into a valid invocation parameter.
        /// </summary>
        /// <param name="consumer">Context of the current execution.</param>
        /// <param name="services">The provider used to register modules and inject services.</param>
        /// <param name="argument">Information about the invocation argument this evaluation converts for.</param>
        /// <param name="value">The raw command query argument to convert.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
        public abstract ValueTask<ConvertResult> Evaluate(
            CallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken);

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="exception">The exception that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConvertResult"/> representing the failed evaluation.</returns>
        protected ConvertResult Error(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (exception is ConvertException convertEx)
                return ConvertResult.FromError(convertEx);

            return ConvertResult.FromError(ConvertException.ConvertFailed(Type, exception));
        }

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> representing a failed evaluation.
        /// </summary>
        /// <param name="error">The error that caused the evaluation to fail.</param>
        /// <returns>A <see cref="ConvertResult"/> representing the failed evaluation.</returns>
        protected ConvertResult Error(string error)
        {
            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException(nameof(error));

            return ConvertResult.FromError(new ConvertException(error));
        }

        /// <summary>
        ///     Creates a new <see cref="ConvertResult"/> representing a successful evaluation.
        /// </summary>
        /// <param name="value">The value converted from a raw argument into the target type of this converter.</param>
        /// <returns>A <see cref="ConvertResult"/> representing the successful evaluation.</returns>
        protected ConvertResult Success(object value)
            => ConvertResult.FromSuccess(value);

        /// <summary>
        ///     Creates a dictionary of base value type converters.
        /// </summary>
        /// <remarks>
        ///     The list of types that are converted by these converters are:
        ///     <list type="bullet">
        ///         <item>String characters, being: <see langword="char"/>.</item>
        ///         <item>Integers variants, being: <see langword="bool"/>, <see langword="byte"/>, <see langword="sbyte"/>, <see langword="short"/>, <see langword="ushort"/>, <see langword="int"/>, <see langword="uint"/>, <see langword="long"/> and <see langword="ulong"/>.</item>
        ///         <item>Floating point numbers, being: <see langword="float"/>, <see langword="decimal"/> and <see langword="double"/>.</item>
        ///         <item>Commonly used structs, being: <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="TimeSpan"/> and <see cref="Guid"/>.</item>
        ///     </list>
        ///     <i>The converter <see cref="TimeSpan"/> does not implement the standard <see cref="TimeSpan.TryParse(string, out TimeSpan)"/>, instead having a custom implementation under <see cref="TimeSpanTypeConverter"/>.</i>
        /// </remarks>
        /// <returns>A new <see cref="Dictionary{TKey, TValue}"/> containing a range of <see cref="TypeConverter"/>'s for all types listed above.</returns>
        public static Dictionary<Type, TypeConverter> GetStandardTypeConverters()
        {
            var list = TryParseTypeConverter.CreateBaseConverters();

            list.Add(new TimeSpanTypeConverter());

            return list.ToDictionary(x => x.Type, x => x);
        }
    }
}
