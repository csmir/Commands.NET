using System.Text.RegularExpressions;

namespace Commands.Parsing;

internal sealed partial class TimeSpanParser : TypeParser<TimeSpan>
{
    private static readonly Dictionary<string, Func<string, TimeSpan>> _callback;

    private static readonly Regex _regex = new(@"(\d*)\s*([a-zA-Z]*)\s*(?:and|,)?\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    static TimeSpanParser()
    {
        _callback = new Dictionary<string, Func<string, TimeSpan>>
        {
            ["second"] = Seconds,
            ["seconds"] = Seconds,
            ["sec"] = Seconds,
            ["s"] = Seconds,

            ["minute"] = Minutes,
            ["minutes"] = Minutes,
            ["min"] = Minutes,
            ["m"] = Minutes,

            ["hour"] = Hours,
            ["hours"] = Hours,
            ["h"] = Hours,

            ["day"] = Days,
            ["days"] = Days,
            ["d"] = Days,

            ["week"] = Weeks,
            ["weeks"] = Weeks,
            ["w"] = Weeks,

            ["month"] = Months,
            ["months"] = Months
        };
    }

    public override ValueTask<ParseResult> Parse(
        IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
    {
        var strArg = argument?.ToString();
        if (!TimeSpan.TryParse(strArg, out TimeSpan span))
        {
            strArg = strArg?.ToLower()?.Trim() ?? "";
            MatchCollection matches = _regex.Matches(strArg);
            if (matches.Count != 0)
            {
                foreach (Match match in matches)
                    if (_callback.TryGetValue(match.Groups[2].Value, out var result))
                        span += result(match.Groups[1].Value);
            }
            else
                return Error($"The provided value is no timespan. Got: '{strArg}'. At: '{parameter.Name}'");
        }

        return Success(span);
    }

    private static TimeSpan Seconds(string match)
        => new(0, 0, int.Parse(match));

    private static TimeSpan Minutes(string match)
        => new(0, int.Parse(match), 0);

    private static TimeSpan Hours(string match)
        => new(int.Parse(match), 0, 0);

    private static TimeSpan Days(string match)
        => new(int.Parse(match), 0, 0, 0);

    private static TimeSpan Weeks(string match)
        => new(int.Parse(match) * 7, 0, 0, 0);

    private static TimeSpan Months(string match)
        => new((int)(int.Parse(match) * 30.437), 0, 0, 0);
}
