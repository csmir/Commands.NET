using Commands.Helpers;
using System.Text;

namespace Commands.Parsing
{
    /// <summary>
    ///     A thread-safe argument parser, implementing <see cref="string"/> as the raw value.
    /// </summary>
    /// <remarks>
    ///     As edge cases are discovered in the parser logic, the parser guidelines may change, and command input might improve, or degrade based on different usecases.
    /// </remarks>
    public static class StringParser
    {
        const char quote = '"';
        const char whitespace = ' ';

        /// <summary>
        ///     Parses a <see cref="string"/> into an array of command arguments.
        /// </summary>
        /// <remarks>
        ///     This parser sets the following guidelines:
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
        public static object[] Parse(string? toParse)
        {
            // return empty range on empty object.
            if (string.IsNullOrWhiteSpace(toParse))
            {
                return [];
            }

            var arr = Array.Empty<string>();
            var sb = new StringBuilder(0, toParse.Length);
            var quoted = false;

            // adds SB content to array & resets.
            void AddReset()
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
                    if (toParse[i] is quote)
                    {
                        // add discovered until now, skipping quote itself.
                        AddReset();

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
                if (toParse[i] is quote)
                {
                    // check end of loop, skipping add.
                    if (i + 1 == toParse.Length)
                    {
                        break;
                    }

                    // add all before quote.
                    AddReset();

                    // set startquote discovery to true.
                    quoted = true;

                    continue;
                }

                // check for whitespace.
                if (toParse[i] is whitespace)
                {
                    // add all before whitespace, skip whitespace itself.
                    AddReset();

                    continue;
                }

                // nomatch for above, add character to current range.
                sb.Append(toParse[i]);
            }

            // if loop ended, do final add.
            AddReset();

            return arr;
        }
    }
}
