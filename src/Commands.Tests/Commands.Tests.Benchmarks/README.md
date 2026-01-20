# Commands.NET Benchmarks

Benchmarks are ran for every release to ensure the library is performing as expected. 

The following benchmarks are ran on the latest version of the library, using the latest version of the .NET SDK and BenchmarkDotNet.

- **CreateArguments**: Parsing provided string input into processable arguments.
- **FindCommands**: Finding the command to execute based on the provided arguments.
- **RunCommand**: Parsing, Finding and running the command, start to finish.
- **CollectionCreate**: Creating a collection for commands.
- **GroupCreate**: Creating a group for commands.
- **CommandCreate**: Creating a command.

# Intel® Core™ Ultra 7 155H 

### Last Benchmark: 11/27/2025

| Method           | Mean       | Error     | StdDev    | Gen0   | Allocated |
|----------------- |-----------:|----------:|----------:|-------:|----------:|
| CreateArguments  |  32.457 ns | 0.6247 ns | 0.5844 ns | 0.0159 |     200 B |
| FindCommands     |  15.107 ns | 0.1320 ns | 0.1170 ns | 0.0025 |      32 B |
| RunCommand       | 116.836 ns | 0.9194 ns | 0.8600 ns | 0.0248 |     312 B |
| CollectionCreate |   4.082 ns | 0.0578 ns | 0.0541 ns | 0.0025 |      32 B |
| GroupCreate      |   9.616 ns | 0.0657 ns | 0.0583 ns | 0.0083 |     104 B |
| CommandCreate    | 109.819 ns | 0.8869 ns | 0.7406 ns | 0.0312 |     392 B |

> [!NOTE]
> Allocation for running commands is a combination of the argument creation, and search process. 
> On itself, it allocates 112 bytes for actual execution.

*This chart can be reproduced at any time by running this benchmark project.*
