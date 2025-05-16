namespace Commands.Samples;

[RequireContext<SampleContext>]
public sealed class BasicModule : CommandModule<SampleContext>
{
    [Name("greet")]
    public string Greet(string name)
        => $"Hello, {name}!";

    [Name("whoami")]
    public string WhoAmI()
        => $"You are {Context.Name}.";

    [Name("echo")]
    public string Echo([Remainder] string message)
        => message;

    [Name("coinflip")]
    public string CoinFlip()
        => new Random().Next(0, 2) == 0 ? "Heads" : "Tails";

    [Name("diceroll")]
    public string DiceRoll()
        => new Random().Next(1, 7).ToString();
}