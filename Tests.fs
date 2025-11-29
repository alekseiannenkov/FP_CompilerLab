module Tests =  

    #load "Lang.fs"
    #load "Tokenizer.fs"
    #load "Parser.fs"

    open Lang
    open Tokenizer
    open Parser

    let printSuccess name =
        printfn "✅ Test passed: %s" name

    let printFail name msg =
        printfn "❌ Test failed: %s\n   Reason: %s" name msg

    let assertEqual testName actual expected =
        if actual = expected then
            printSuccess testName
        else
            printFail testName (sprintf "Expected:\n%A\nGot:\n%A" expected actual)

    let test1 () =
        let code = "<- inc \\ x -> + x 1"
        let tokens = tokenize code
        let ast = parseExpr tokens
        let expected =
            [ Gets("inc", Lambda("x", Apply(Apply(PredefFunc "+", Var "x"), Int 1))) ]
        assertEqual "Simple lambda definition" ast expected

    let test2 () =
        let code = "$ <- fact \\ n -> if <= n 1 1 * n (fact - n 1)"
        let tokens = tokenize code
        let ast = parseExpr tokens
        let expected =
            [ GetsRec("fact",
                Lambda("n",
                    If(
                        Apply(Apply(PredefFunc "<=", Var "n"), Int 1),
                        Int 1,
                        Apply(
                            Apply(PredefFunc "*", Var "n"),
                            Apply(Var "fact", Apply(Apply(PredefFunc "-", Var "n"), Int 1))
                        )
                    )
                ))
            ]
        assertEqual "Recursive factorial function" ast expected

    let test3 () =
        let code = "\\ x y z -> + x (* y z)"
        let tokens = tokenize code
        let ast = parseExpr tokens
        let expected =
            [ Lambda("x",
                Lambda("y",
                    Lambda("z",
                        Apply(
                            Apply(PredefFunc "+", Var "x"),
                            Apply(
                                Apply(PredefFunc "*", Var "y"),
                                Var "z"
                            )
                        )
                    )
                )) ]
        assertEqual "Multi-arg lambda with nested application" ast expected

    let test4 () =
        let code = "<- x (f [a b c] 5)"
        let tokens = tokenize code
        let ast = parseExpr tokens
        let expected =
            [ Gets("x",
                Apply(
                    Apply(Var "f", List [Var "a"; Var "b"; Var "c"]),
                    Int 5
                )
            ) ]
        assertEqual "Function application with list argument" ast expected

    let runAll () =
        test1()
        test2()
        test3()
        test4()

    runAll()
