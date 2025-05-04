Commands.NET implements an API specifically for testing, allowing the user to evaluate whether commands will succeed or fail under certain scenarios. 
This is useful for ensuring that commands are functioning as expected, and for debugging issues with commands.

- [Creating Tests](#creating-tests)
- [Testing Commands](#testing-commands)

## Creating Tests

Tests are defined in one of two ways: By creating a new `Test`, or by marking `Test` on modular command methods.

### TestProvider

Alongside other components within Commands.NET, a creation pattern is used to create a new `Test`. This pattern is used to ensure that the test is correctly initialized.
```cs
var provider = Test.From(...).ToTest();
```

### Test Attribute

Tests can be defined by marking a method with the `Test` attribute. This attribute is used to define the test, and will be used by the `TestCollection` to execute the test.

```cs
// Because an argument too many is provided here, the test will fail on Match, which is what the test expects to fail at, so it will complete succesfully.
[Name("testcommand")]
[Test(ShouldEvaluateTo = TestResultType.MatchFailure, Arguments = "arguments")]
public void TestMethod()
{

}
```

## Testing Commands

Collections of commands can be tested individually using the `TestProvider`. 
When making use of the `ComponentProvider`, some clever use of LINQ will allow us to make providers for every known command.

```cs
var tests = components.GetCommands().Select(x => TestProvider.From(x).ToProvider());
```

> [!IMPORTANT]
> When creating a test provider in this way, all `TestAttribute` definitions present on the command's execution delegate will be included. 
> Additionally, extra tests can be defined using its own respective properties.

This will create a new instance of `TestProvider` for every command, which contains available tests. 
The provider can be tested against and awaited. All known tests will be ran in sequence, and the results returned as an enumerable of `TestResult`:

```cs
var results = await provider.Test((input) => new ConsoleCallerContext(input));
```

When this method completes, all tests have been executed for the specified command. 
To verify whether the tests succeeded, the results contain a `Success` boolean. Again, some LINQ will help us get the values we want.

```cs
if (results.Any(x => !x.Success))
{
	// A test failed for the command.
}
```

> [!NOTE]
> To ensure that the context is not shared between tests, the `TestProvider` will create a new instance of `ICallerContext` using the provided delegate for every test.