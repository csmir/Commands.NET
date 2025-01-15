open Commands
open System

open ExecutionDependencies

let handler = ResultHandler.Define(Action<LocalCallerContext, IExecuteResult, IServiceProvider>(fun (context: LocalCallerContext) (result: IExecuteResult) (services: IServiceProvider) -> printfn "%A" result))
let command = Command.Define(Func<string>(fun () -> "Hello world"), "hello")

let manager 
    = ComponentManager.Define()
        .Component(command)
        .Handler(handler)
        .ToManager()

manager.TryExecute<LocalCallerContext>(new LocalCallerContext(), "hello")