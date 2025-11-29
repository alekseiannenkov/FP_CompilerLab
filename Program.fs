
module InterpreterTests =

    // #load "Lang.fs"
    // #load "Tokenizer.fs"
    // #load "Parser.fs"
    // #load "Intepreter.fs"

    open Lang
    open Tokenizer
    open Parser
    open Interpreter

    let text = "$ <- fact \\ n -> 
                if <= n 1 
                1 
                * n (fact - n 1)
                <- x (fact 5)
                x"

    // let text = "<- fact \\ x -> x + 1
    //             (fact 2)"
    Seq.toList text
    let tokens = Tokenizer.tokenize(text)
    let parsed = Parser.parseExpr(tokens)
    let result, env = Interpreter.interpret parsed Map.empty
    result
