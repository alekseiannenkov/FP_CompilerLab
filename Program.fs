module Program =

    open Lang
    open Tokenizer
    open Parser
    open Interpreter
    open InterpreterTests

    runAll()

    printfn "Press Enter to exit"
    System.Console.ReadLine() |> ignore

    0
