module ExecutionDependencies

open Commands
open System

type public LocalCallerContext() = 
    interface ICallerContext with 
        member this.Respond (message: obj): unit = printfn "%s" (message.ToString())

type public ResultProcessor() =
    static member Process(): Action<ICallerContext, IExecuteResult, IServiceProvider> 
        = Action<ICallerContext, IExecuteResult, IServiceProvider>(fun (context: ICallerContext) (result: IExecuteResult) (services: IServiceProvider) -> printfn "%A" result)