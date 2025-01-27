open Commands
open System

let handler = ResultHandler.From(Action<ConsoleContext, IExecuteResult, IServiceProvider>(fun (context: ConsoleContext) (result: IExecuteResult) (services: IServiceProvider) -> printfn "%A" result))
let command = Command.From(Func<string>(fun () -> "Hello world"), "hello")

let manager 
    = ComponentManager.From()
        .Component(command)
        .Handler(handler)
        .Create()

manager.TryExecute<ConsoleContext>(new ConsoleContext("hello")) |> ignore