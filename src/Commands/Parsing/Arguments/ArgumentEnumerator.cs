using System.Diagnostics.CodeAnalysis;

namespace Commands.Parsing
{
    /// <summary>
    ///     Contains a set of arguments for the command pipeline. This class is not intended to be implemented by end-users. 
    ///     By using either <see cref="CommandTree.Execute{T}(T, IEnumerable{KeyValuePair{string, object?}}, CommandOptions?)"/> or <see cref="CommandTree.Execute{T}(T, IEnumerable{object?}, CommandOptions?)"/> you can use named or unnamed command entry.
    /// </summary>
    /// <remarks>
    ///     Searching for commands supports only unnamed arguments. Named arguments are used for command parameter population.
    /// </remarks>
    public struct ArgumentEnumerator
    {
        const char u0020 = ' ';

        private int _size;
        private int _indexUnnamed = 0;

        private readonly object[] _unnamedArgs;
        private readonly Dictionary<string, object?> _namedArgs;

        /// <summary>
        ///     Gets the length of the argument set. This is represented by a sum of named and unnamed arguments, reducing it by the search range of the resulted command by calling <see cref="SetSize(int)"/>.
        /// </summary>
        public readonly int Length
            => _size;

        /// <summary>
        ///     Creates a new instance of the <see cref="ArgumentEnumerator"/> class with a set of named arguments.
        /// </summary>
        /// <param name="args">The range of named arguments to enumerate in this set.</param>
        /// <param name="comparer">The comparer to evaluate keys in the inner named dictionary.</param>
        public ArgumentEnumerator(IEnumerable<KeyValuePair<string, object?>> args, StringComparer comparer)
        {
            _namedArgs = new(comparer);

            var unnamedFill = new List<string>();

            foreach (var (key, value) in args)
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key), "The key of an argument KeyValuePair cannot be null.");

                if (value == null)
                    unnamedFill.Add(key);
                else
                    _namedArgs[key] = value;
            }

            _unnamedArgs = [.. unnamedFill];
            _size = _unnamedArgs.Length + _namedArgs.Count;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="ArgumentEnumerator"/> class with a set of unnamed arguments.
        /// </summary>
        /// <param name="args">The range of unnamed arguments to enumerate in this set.</param>
        public ArgumentEnumerator(IEnumerable<object> args)
        {
            _namedArgs = [];

            _unnamedArgs = args.ToArray();
            _size = _unnamedArgs.Length;
        }

        /// <summary>
        ///     Makes an attempt to retrieve the next argument in the set. If a named argument is found, it will be removed from the set and returned. 
        ///     If an unnamed argument is found, it will be returned and the currently observed index will be incremented to return the next unnamed argument on the next try.
        /// </summary>
        /// <remarks>
        ///     This method compares the parameter name to the named arguments known to the set at the point of execution and determines the result based on the <see cref="StringComparer"/> set in <see cref="CommandOptions.MatchComparer"/>.
        /// </remarks>
        /// <param name="parameterName">The name of the command parameter that this set attempts to match to.</param>
        /// <param name="value">The value returned when an item is discovered in the set.</param>
        /// <returns><see langword="true"/> when an item was discovered in the set, otherwise <see langword="false"/>.</returns>
        public bool TryNext(string parameterName, out object? value)
        {
            if (_namedArgs.TryGetValue(parameterName, out value))
            {
                return true;
            }

            if (_indexUnnamed >= _unnamedArgs.Length)
                return false;

            value = _unnamedArgs[_indexUnnamed++];

            return true;
        }

        /// <summary>
        ///     Makes an attempt to retrieve the next argument in the set, exclusively browsing unnamed arguments to match in search operations.
        /// </summary>
        /// <param name="searchHeight">The next incrementation that the search operation should attempt to match in the command set.</param>
        /// <param name="value">The value returned when an item is discovered in the set.</param>
        /// <returns><see langword="true"/> when an item was discovered in the set, otherwise <see langword="false"/>.</returns>
        public readonly bool TryNext(int searchHeight, [NotNullWhen(true)] out string? value)
        {
            if (searchHeight < _unnamedArgs.Length && _unnamedArgs[searchHeight] is string str)
            {
                value = str;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        ///     Sets the size of the set, reducing the length by the search height that ended up discovering the command.
        /// </summary>
        /// <param name="searchHeight">The final incrementation that the search operation returned the discovered result with.</param>
        public void SetSize(int searchHeight)
        {
            _indexUnnamed = searchHeight;
            _size = _unnamedArgs.Length - searchHeight;
        }

        /// <summary>
        ///     Joins the remaining unnamed arguments in the set into a single string.
        /// </summary>
        /// <returns>A joined string containing all remaining arguments in this enumerator.</returns>
        public readonly string JoinRemaining()
        {
            return string.Join(u0020, _unnamedArgs[_indexUnnamed..]);
        }

        /// <summary>
        ///     Takes the remaining unnamed arguments in the set into an array which is used by Collector arguments.
        /// </summary>
        /// <returns>An array of objects that represent the remaining arguments of this enumerator.</returns>
        public readonly object[] TakeRemaining()
        {
            return _unnamedArgs[_indexUnnamed..];
        }
    }
}
