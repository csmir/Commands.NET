# Commands.NET Benchmarks

Benchmarks are ran for every release to ensure the library is performing as expected. 

The following benchmarks are ran on the latest version of the library, using the latest version of the .NET SDK and BenchmarkDotNet.

- **CreateArguments**: Parsing provided string input into processable arguments.
- **FindCommands**: Finding the command to execute based on the provided arguments.
- **RunCommand**: Parsing, Finding and running the command, start to finish.
- **CollectionCreate**: Creating a collection for commands.
- **GroupCreate**: Creating a group for commands.
- **CommandCreate**: Creating a command.

# Intel® Core™ i7-1255U

### Last Benchmark: 1/22/2025

| Method                | Mean       | Error     | StdDev    | Gen0   | Allocated |
|---------------------- |-----------:|----------:|----------:|-------:|----------:|
| CreateArguments       |  50.497 ns | 0.9744 ns | 0.9570 ns | 0.0446 |     280 B |
| FindCommands          |  45.632 ns | 0.4376 ns | 0.3879 ns | 0.0140 |      88 B |
| RunCommand            | 199.856 ns | 2.5021 ns | 2.2180 ns | 0.0815 |     512 B |
| CollectionCreate      |   4.372 ns | 0.0760 ns | 0.0635 ns | 0.0051 |      32 B |
| GroupCreate           |  11.112 ns | 0.0974 ns | 0.0864 ns | 0.0166 |     104 B |
| CommandCreate         | 137.402 ns | 1.4430 ns | 1.2050 ns | 0.0508 |     320 B |

> [!NOTE]
> Allocation for running commands is a combination of the argument creation, and search process. 
> On itself, it allocates 144 bytes for actual execution.

# Intel® Core™ Ultra 7 155H 

### Last Benchmark: 11/27/2025

| Method           | Mean       | Error     | StdDev    | Gen0   | Allocated |
|----------------- |-----------:|----------:|----------:|-------:|----------:|
| CreateArguments  |  42.739 ns | 0.7411 ns | 0.6933 ns | 0.0159 |     200 B |
| FindCommands     |  36.797 ns | 0.7566 ns | 1.0100 ns | 0.0025 |      32 B |
| RunCommand       | 172.082 ns | 3.0553 ns | 2.8579 ns | 0.0248 |     312 B |
| CollectionCreate |   4.652 ns | 0.1170 ns | 0.1094 ns | 0.0025 |      32 B |
| GroupCreate      |  11.211 ns | 0.0869 ns | 0.0813 ns | 0.0083 |     104 B |
| CommandCreate    | 165.108 ns | 1.7455 ns | 1.6327 ns | 0.0370 |     464 B |

> [!NOTE]
> Allocation for running commands is a combination of the argument creation, and search process. 
> On itself, it allocates 112 bytes for actual execution.

*This chart can be reproduced at any time by running this benchmark project.*
