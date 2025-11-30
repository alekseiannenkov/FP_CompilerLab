module InterpreterTests =

    open Lang
    open Tokenizer
    open Parser
    open Interpreter

    let printSuccess name =
        printfn "✅ Interpreter test passed: %s" name

    let printFail name msg =
        printfn "❌ Interpreter test failed: %s\n   Reason: %s" name msg

    let assertEqualValue testName (actual: Expr) (expected: Expr) =
        if actual = expected then
            printSuccess testName
        else
            printFail testName (sprintf "Expected:\n%A\nGot:\n%A" expected actual)

    // Runs the program: gets code as a string input, tokenizes it, parses and interprets.
    // Returns (result, environment)
    let runProgram (code: string) : Expr * Env =
        let tokens = tokenize code
        let ast = parseExpr tokens
        Interpreter.interpret ast Map.empty

    // ------------------ TESTS ------------------

    // 1. Trivial arithmetics
    let testSimpleArith () =
        let code = "+ 40 2"
        let result, _ = runProgram code
        assertEqualValue "simple addition" result (Int 42)

    // 2. let-assignment and usage of a variable
    let testLetBinding () =
        let code = "<- x 10 
                    x"
        let result, _ = runProgram code
        assertEqualValue "let binding" result (Int 10)

    // 3. if-test
    let testIf () =
        let codeTrue  = "if == 1 1  
                            10  
                            20"
        let codeFalse = "if == 1 2  
                            10  
                            20"

        let r1, _ = runProgram codeTrue
        let r2, _ = runProgram codeFalse

        assertEqualValue "if true branch"  r1 (Int 10)
        assertEqualValue "if false branch" r2 (Int 20)

    // 4. Initialization and application of a simple function: inc x = x + 1
    let testSimpleFunction () =
        let code = "<- inc \\ x -> + x 1 
                    (inc 2)"
        let result, _ = runProgram code
        assertEqualValue "simple function inc" result (Int 3)

    // 5. Closure: addN n = (\ x -> x + n)
    let testClosure () =
        let code = "
            <- addN \\ n -> 
                        \\ x -> + x n
            <- add5 (addN 5)
            (add5 10)
        "
        let result, _ = runProgram code
        assertEqualValue "closure addN/add5" result (Int 15)

    // 6. Recursive factorial
    let testRecFact () =
        let code = "
            $ <- fact \\ n ->
                if <= n 1
                    1
                    * n (fact - n 1)
            <- x (fact 5)
            x
        "
        let result, _ = runProgram code
        assertEqualValue "recursive factorial fact 5" result (Int 120)

    // 7. List
    let testListLiteral () =
        let code = "[1 2 3]"
        let result, _ = runProgram code
        let expected = List [Int 1; Int 2; Int 3]
        assertEqualValue "list literal [1 2 3]" result expected

    // 8. head / tail / isEmpty / length
    let testListBuiltinSimple () =
        let codeHead     = "(head [1 2 3])"
        let codeTail     = "(tail [1 2 3])"
        let codeEmpty    = "(isEmpty [ ])"
        let codeNonEmpty = "(isEmpty [1])"
        let codeLength   = "(length [10 20 30 40])"

        let rHead, _     = runProgram codeHead
        let rTail, _     = runProgram codeTail
        let rEmpty, _    = runProgram codeEmpty
        let rNonEmpty, _ = runProgram codeNonEmpty
        let rLength, _   = runProgram codeLength

        assertEqualValue "head [1 2 3]"    rHead (Int 1)
        assertEqualValue "tail [1 2 3]"    rTail (List [Int 2; Int 3])
        assertEqualValue "isEmpty []"      rEmpty (Bool true)
        assertEqualValue "isEmpty [1]"     rNonEmpty (Bool false)
        assertEqualValue "length [10..40]" rLength (Int 4)


    // 9. map: (\x -> x + 1) for [1 2 3] -> [2 3 4]
    let testMap () =
        let code = "(map \\ x -> + x 1 [1 2 3])"
        let result, _ = runProgram code
        let expected = List [Int 2; Int 3; Int 4]
        assertEqualValue "map (+1) [1 2 3]" result expected

    // 10. filter: leave numbers >1 from [1 2 3 4] -> [2 3 4]
    let testFilter () =
        let code = "(filter \\ x -> > x 1 [1 2 3 4])"
        let result, _ = runProgram code
        let expected = List [Int 2; Int 3; Int 4]
        assertEqualValue "filter (>1) [1 2 3 4]" result expected

    // 11. fold: sum of list [1 2 3 4] -> 10
    // fold (\acc x -> acc + x) 0 [1 2 3 4]
    let testFold () =
        let code = "(fold \\ acc x -> + acc x 0 [1 2 3 4])"
        let result, _ = runProgram code
        let expected = Int 10
        assertEqualValue "fold sum [1 2 3 4]" result expected

    // 12. mapfold:
    //   f state x = [x + 1; state + x]
    //   init state 0, list [1 2 3]
    //   mapped = [2 3 4], finalState = 6
    //   result: [ [2 3 4] 6 ]
    let testMapFold () =
        let code = "
            (mapfold \\ s x -> [ + x 1 + s x ] 0 [1 2 3])
        "
        let result, _ = runProgram code
        let expected =
            List [
                List [Int 2; Int 3; Int 4]; // mapped
                Int 6                       // final state = 1 + 2 + 3
            ]
        assertEqualValue "mapfold example" result expected

    // 13. Tail recursion factorial (using state = [n acc])
    let testTailRecFact () =
        let code = "
            $ <- factTR \\ state ->
                if <= (head state) 1
                    (head (tail state))
                    (factTR [ - (head state) 1  * (head (tail state)) (head state) ])
            <- fact \\ n -> (factTR [ n 1 ])
            (fact 5)
        "
        let result, _ = runProgram code
        assertEqualValue "tail-recursive factorial fact 5" result (Int 120)

    // 14. Fold factorial: 5! = 120
    let testFactWithFold () =
        let code = "
            (fold \\ acc x -> * acc x 1 [1 2 3 4 5])
        "
        let result, _ = runProgram code
        assertEqualValue "factorial via fold [1..5]" result (Int 120)

    // 15. Tail recursion Fibonacci using state [n a b]
    let testFib () =
        let code = "
            $ <- fibTR \\ state ->
                if <= (head state) 0
                    (head (tail state))
                    (fibTR
                        [ - (head state) 1
                        (head (tail (tail state)))
                        + (head (tail state)) (head (tail (tail state))) ])
            <- fib \\ n -> (fibTR [ n 0 1 ])
            (fib 6)
        "
        let result, _ = runProgram code
        assertEqualValue "recursive fibonacci fib 6" result (Int 8)



    // 16. Sum of squares using map + fold
    let testSumOfSquares () =
        let code = "
            <- sqs (map \\ x -> * x x [1 2 3 4])
            (fold \\ acc x -> + acc x 0 sqs)
        "
        let result, _ = runProgram code
        assertEqualValue "sum of squares [1..4]" result (Int 30)

    // 17. Prefix sums using mapfold
    let testPrefixSumsWithMapFold () =
        let code = "
            (mapfold \\ s x -> [ + s x + s x ] 0 [1 2 3])
        "
        let result, _ = runProgram code
        let expected =
            List [
                List [Int 1; Int 3; Int 6];
                Int 6
            ]
        assertEqualValue "prefix sums via mapfold" result expected


    // 18. First 10 of Fibonacci numbers using tail recursion fibTR
    let testFibFirst10 () =
        let code = "
            $ <- fibTR \\ state ->
                if <= (head state) 0
                    (head (tail state))
                    (fibTR
                        [ - (head state) 1
                        (head (tail (tail state)))
                        + (head (tail state)) (head (tail (tail state))) ])

            [ (fibTR [ 0 0 1 ])
            (fibTR [ 1 0 1 ])
            (fibTR [ 2 0 1 ])
            (fibTR [ 3 0 1 ])
            (fibTR [ 4 0 1 ])
            (fibTR [ 5 0 1 ])
            (fibTR [ 6 0 1 ])
            (fibTR [ 7 0 1 ])
            (fibTR [ 8 0 1 ])
            (fibTR [ 9 0 1 ]) ]
        "

        let result, _ = runProgram code

        let expected =
            List [
                Int 0   // fib 0
                Int 1   // fib 1
                Int 1   // fib 2
                Int 2   // fib 3
                Int 3   // fib 4
                Int 5   // fib 5
                Int 8   // fib 6
                Int 13  // fib 7
                Int 21  // fib 8
                Int 34  // fib 9
            ]

        assertEqualValue "first 10 fibonacci numbers via fibTR" result expected





    // Run all tests
    let runAll () =
        testSimpleArith()
        testLetBinding()
        testIf()
        testSimpleFunction()
        testClosure()
        testRecFact()
        testListLiteral()
        testListBuiltinSimple()
        testMap()
        testFilter()
        testFold()
        testMapFold()
        testTailRecFact()
        testFactWithFold()
        testFib()
        testSumOfSquares()
        testPrefixSumsWithMapFold()
        testFibFirst10()

