module InterpreterTests =

    #load "Lang.fs"
    #load "Tokenizer.fs"
    #load "Parser.fs"
    #load "Intepreter.fs"

    open Lang
    open Tokenizer
    open Parser
    open Interpreter

    let testEvaluate code expected =
        let tokens = tokenize code
        let exprs = parseExpr tokens
        match exprs with
        | [expr] ->
            let result, _ = eval expr Map.empty
            if result = expected then
                printfn $"✅ Passed: {code}"
            else
                printfn $"❌ Failed: {code}\nExpected: {expected}\nGot: {result}"
        | _ -> printfn $"❌ Failed to parse: {code}"


    // ----------------- Тесты --------------------

    let runTests () =
        // Тест 1: Арифметика
        testEvaluate "+ 2 3" (Int 5)

        // Тест 2: Присваивание и использование
        let code2 = "<- x 10"
        let expr2 = parseExpr (tokenize code2)
        let _, env2 = eval (List.head expr2) Map.empty
        let yExpr = Apply(Apply(PredefFunc "+", Var "x"), Int 5)
        let res2, _ = eval yExpr env2
        if res2 = Int 15 then
            printfn "✅ Passed: assignment and reuse"
        else
            printfn $"❌ Failed reuse: expected 15, got {res2}"

        // Тест 3: Лямбда и применение
        testEvaluate "(\\ x -> + x 1) 4" (Int 5)

        // Тест 4: Рекурсивный факториал
        testEvaluate "$ <- fact \\ x -> if <= x 1 1 * x (fact - x 1)" |> ignore
        testEvaluate "(fact 5)" (Int 120)

        // Тест 5: Условный if
        testEvaluate "if == 3 3 42 0" (Int 42)
        testEvaluate "if == 3 4 42 0" (Int 0)
