namespace Commands.Samples

open Commands

type FSharpModule = class 
    inherit CommandModule
        [<Name("echo")>]
        member x.Echo(args: string) = args;

        [<Name("hello")>]
        member x.Hello() = "Hello world";
end