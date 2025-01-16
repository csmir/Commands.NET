open Commands
open System

open ExecutionDependencies

let handler = ResultHandler.From(Action<LocalCallerContext, IExecuteResult, IServiceProvider>(fun (context: LocalCallerContext) (result: IExecuteResult) (services: IServiceProvider) -> printfn "%A" result))
let command = Command.From(Func<string>(fun () -> "Hello world"), "hello")

let manager 
    = ComponentManager.From()
        .Component(command)
        .Handler(handler)
        .ToManager()

manager.TryExecute<LocalCallerContext>(new LocalCallerContext(), "hello")