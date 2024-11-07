using Commands.Helpers;
using System.ComponentModel;
using System.Text;

namespace Commands.Parsing
{
    /// <summary>
    ///     A thread-safe argument parser, implementing <see cref="string"/> as the raw value.
    /// </summary>
    /// <remarks>
    ///     <b>This class does not adhere to semantic versioning. </b>
    ///     As edge cases are discovered in the parser logic, the parser guidelines may change, and command input might improve or degrade based on different usecases.
    /// </remarks>
    public static partial class StringParser
    {
        const char u0022 = '"';
        const char u0020 = ' ';
        const char u002D = '-';

        /// <summary>
        ///     This method is deprecated and will be removed in a future version. Use <see cref="ParseKeyCollection(string)"/> instead.
        /// </summary>
        /// <param name="toParse"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by ParseKeyCollection(string).")]
        public static object[] Parse(string? toParse)
        {
            if (string.IsNullOrWhiteSpace(toParse))
            {
                return [];
            }

            return ParseKeyCollection(toParse).Select(x => (object)x).ToArray();
        }

        /// <summary>
        ///     Parses a <see cref="string"/> into a collection of command arguments. This collection is a key-value pair, where the key is the argument name and the value is the argument value. 
        ///     When arguments have no value, the name is the value instead.
        /// </summary>
        /// <remarks>
        ///     This implementation sets the following guidelines:
        ///     <list type="number">
        ///         <item>
        ///             <b>Flags</b> are used as argument names. When a flag is prefixed with one hyphen (-), it will be considered a standalone argument. 
        ///             When a flag is prefixed with two hyphens (--), it will be considered an argument name for the next argument, unless if the following argument is another flag.
        ///         </item>
        ///         <item>
        ///             <b>Whitespace</b> announcements will wrap the previous argument and start a new one, unless if the previous argument is an argument name. In this case, it will include the name and then start a new argument.
        ///         </item>
        ///         <item>
        ///             <b>Quotations</b> at the start of an argument will begin argument concatenation. This concatenation will collect all following arguments until an end-quote is found. 
        ///             This end-quote is only considered to be an end-quote, if it is actually the lowest level quote in all following arguments.    
        ///         </item>
        ///         <item>
        ///             <b>Unnamed</b> arguments will be added to the collection as a key, with the value being null.
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <param name="toParse">The input string to parse.</param>
        /// <returns>
        ///     A collection of key-value pairs as command arguments.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object?>> ParseKeyValueCollection(string toParse)
        {
            if (string.IsNullOrWhiteSpace(toParse))
            {
                ThrowHelpers.ThrowInvalidArgument(toParse);
            }

            // Reserved for joining arguments.
            var openState = 0;
            var concatenating = false;
            var concatenation = new List<string>();

            // Reserved for named arguments.
            string? name = null;

            foreach (var argument in toParse.Split())
            {
                if (concatenating)
                {
                    if (argument.StartsWith(u0022))
                    {
                        openState++;

                        concatenation.Add(argument);

                        if (argument.Length > 1)
                        {
                            if (argument[1..].Contains(u0022))
                            {
                                openState--;
                            }
                        }

                        continue;
                    }

                    if (argument.EndsWith(u0022))
                    {
                        if (openState is 0)
                        {
                            concatenating = false;

                            concatenation.Add(argument);

                            if (name is null)
                                yield return new(string.Join(u0020, concatenation), null);
                            else
                            {
                                yield return new(name, string.Join(u0020, concatenation));

                                name = null;
                            }

                            concatenation.Clear();
                        }
                        else
                        {
                            openState--;

                            concatenation.Add(argument);
                        }

                        continue;
                    }

                    concatenation.Add(argument);
                }
                else
                {
                    if (argument.StartsWith(u002D))
                    {
                        if (argument.Length > 1)
                        {
                            if (argument[1] is u002D)
                            {
                                if (name is not null)
                                    yield return new(name, null);

                                name = argument[2..];

                                continue;
                            }
                        }

                        yield return new(argument[1..], null);

                        continue;
                    }

                    if (argument.StartsWith(u0022) && !argument.EndsWith(u0022))
                    {
                        concatenating = true;

                        concatenation.Add(argument);

                        continue;
                    }

                    if (name is null)
                        yield return new(argument, null);
                    else
                    {
                        yield return new(name, argument);

                        name = null;
                    }
                }
            }
        }

        /// <summary>
        ///     Parses a <see cref="string"/> into a collection of command arguments.
        /// </summary>
        /// <remarks>
        ///     This implementation sets the following guidelines:
        ///     <list type="number">
        ///         <item>
        ///             <b>Whitespace</b> announcements will wrap the previous argument and build a new one.
        ///         </item>
        ///         <item>
        ///             <b>Quotations</b> will wrap the previous argument and build a new one.
        ///         </item>
        ///         <item>
        ///             <b>Quoted</b> arguments will start when a start-quote is discovered, and consider all following whitespace as part of the previous argument. 
        ///             This argument will only be wrapped when an end-quote is announced.
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <param name="toParse">The input string to parse.</param>
        /// <returns>
        ///     A collection of command arguments.
        /// </returns>
        public static IEnumerable<string> ParseKeyCollection(string toParse)
        {
            // return empty range on empty object.
            if (string.IsNullOrWhiteSpace(toParse))
            {
                ThrowHelpers.ThrowInvalidArgument(toParse);
            }

            var arr = Array.Empty<string>();
            var sb = new StringBuilder(0, toParse.Length);
            var quoted = false;

            // adds SB content to array & resets.
            void Reset()
            {
                // if anything exists, otherwise skip.
                if (sb.Length > 0)
                {
                    sb.ToString().AddTo(ref arr);

                    // clear for next range.
                    sb.Clear();
                }
            }

            // enter loop for string inner char[]
            for (int i = 0; i < toParse.Length; i++)
            {
                // if startquote found, skip space check & continue until next occurrence of quote.
                if (quoted)
                {
                    // next quote occurrence.
                    if (toParse[i] is u0022)
                    {
                        // add discovered until now, skipping quote itself.
                        Reset();

                        // set quoted to false, quoted range is handled.
                        quoted = false;

                        // next loop step.
                        continue;
                    }

                    // add char in quote range.
                    sb.Append(toParse[i]);

                    // dont allow the checks below this statement, next loop step.
                    continue;
                }

                // check for startquote.
                if (toParse[i] is u0022)
                {
                    // check end of loop, skipping add.
                    if (i + 1 == toParse.Length)
                    {
                        break;
                    }

                    // add all before quote.
                    Reset();

                    // set startquote discovery to true.
                    quoted = true;

                    continue;
                }

                // check for whitespace.
                if (char.IsWhiteSpace(toParse[i]))
                {
                    // add all before whitespace, skip whitespace itself.
                    Reset();

                    continue;
                }

                // nomatch for above, add character to current range.
                sb.Append(toParse[i]);
            }

            // if loop ended, do final add.
            Reset();

            return arr;
        }
    }
}
