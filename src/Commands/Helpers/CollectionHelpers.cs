using System.Collections;

namespace Commands.Helpers
{
    internal static class CollectionHelpers
    {
        public static IEnumerable<T> CastWhere<T>(this IEnumerable input)
        {
            foreach (var @in in input)
                if (@in is T @out)
                    yield return @out;
        }

        public static T SelectFirstOrDefault<T>(this IEnumerable input, T defaultValue = default)
        {
            foreach (var @in in input)
                if (@in is T @out)
                    return @out;

            return defaultValue;
        }

        public static bool Contains<T>(this IEnumerable input, bool allowMultipleMatches)
            where T : Attribute
        {
            var found = false;
            foreach (var entry in input)
            {
                if (entry is T)
                {
                    if (!allowMultipleMatches)
                    {
                        if (!found)
                            found = true;
                        else
                            return false;
                    }
                    else
                    {
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }

        public static void AddTo<T>(this T value, ref T[] range)
        {
            var oLen = range.Length;
            Array.Resize(ref range, oLen + 1);

            range[oLen] = value;
        }
    }
}
