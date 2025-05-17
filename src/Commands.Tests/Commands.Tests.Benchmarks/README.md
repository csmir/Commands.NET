# Commands.NET Benchmarks

Benchmarks are ran for every release to ensure the library is performing as expected. 

The following benchmarks are ran on the latest version of the library, using the latest version of the .NET SDK and BenchmarkDotNet.

### Last Benchmark: 1/22/2025

- **CreateArguments**: Parsing provided string input into processable arguments.
- **FindCommands**: Finding the command to execute based on the provided arguments.
- **RunCommand**: Parsing, Finding and running the command, start to finish.
- **RunCommandNonBlocking**: Parsing, Finding and running the command asynchronously, start to finish.
- **CollectionCreate**: Creating a collection for commands.
- **GroupCreate**: Creating a group for commands.
- **CommandCreate**: Creating a command.

# Intel® Core™ i7-1255U

| Method                | Mean       | Error     | StdDev    | Gen0   | Allocated |
|---------------------- |-----------:|----------:|----------:|-------:|----------:|
| CreateArguments       |  50.497 ns | 0.9744 ns | 0.9570 ns | 0.0446 |     280 B |
| FindCommands          |  45.632 ns | 0.4376 ns | 0.3879 ns | 0.0140 |      88 B |
| RunCommand            | 199.856 ns | 2.5021 ns | 2.2180 ns | 0.0815 |     512 B |
| RunCommandNonBlocking | 207.349 ns | 2.2603 ns | 1.8874 ns | 0.0892 |     560 B |
| CollectionCreate      |   4.372 ns | 0.0760 ns | 0.0635 ns | 0.0051 |      32 B |
| GroupCreate           |  11.112 ns | 0.0974 ns | 0.0864 ns | 0.0166 |     104 B |
| CommandCreate         | 137.402 ns | 1.4430 ns | 1.2050 ns | 0.0508 |     320 B |

> [!NOTE]
> Allocation for running commands is a combination of the argument creation, and search process. 
> On itself, it allocates 144 / 192 bytes for actual execution.

# Intel® Core™ Ultra 7 155H 

> Not yet tested.

*This chart can be reproduced at any time by running this benchmark project.*