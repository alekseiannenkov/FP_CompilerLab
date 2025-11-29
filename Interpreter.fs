module Interpreter =    

    // #load "Parser.fs"
    // #load "Tokenizer.fs"
    // #load "Lang.fs"

    open Parser
    open Tokenizer
    open Lang


    let operations = function
        | "+" -> (function [Int(a);Int(b)] -> Int(a+b))
        | "-" -> (function [Int(a);Int(b)] -> Int(a-b))
        | "*" -> (function [Int(a);Int(b)] -> Int(a*b))
        | "/" -> (function [Int(a);Int(b)] -> Int(a/b))

        | "==" -> (function [Int(a);Int(b)] -> Bool(a = b))
        | "!=" -> (function [Int(a);Int(b)] -> Bool(a <> b))
        | ">"  -> (function [Int(a);Int(b)] -> Bool(a > b))
        | ">=" -> (function [Int(a);Int(b)] -> Bool(a >= b))
        | "<"  -> (function [Int(a);Int(b)] -> Bool(a < b))
        | "<=" -> (function [Int(a);Int(b)] -> Bool(a <= b))

    
    let rec eval (expr: Expr) (env: Env) : Expr * Env = 
        printfn "Eval %A in %A" expr env
        match expr with
        | Int x -> Int x, env

        | Bool x -> Bool x, env

        | Var x -> Map.find x env, env

        | PredefFunc name ->
            Partial(name, 2, []), env


        | Apply (expr1, expr2) -> 
            let func, env1 = eval expr1 env
            let arg, env2 = eval expr2 env1
            apply func arg env2

        | If (cond, expr1, expr2) ->
            let res, env1  = eval cond env
            match res with 
            | Bool true -> eval expr1 env1
            | Bool false -> eval expr2 env1
            
        | Lambda (var, body) ->
            Closure(expr, env), env

        | Gets (var, expr1) ->
            let res, env1 = eval expr1 env
            Int 0, Map.add var res env1

        | GetsRec(var, exp1) -> 
            let closure = RClosure(exp1, env, var)
            Int 0, Map.add var closure env

        | List exprs ->
        let rec evalList acc env = function
            | [] -> List(List.rev acc), env
            | x::xs -> 
                let v, env' = eval x env
                evalList (v::acc) env' xs
        evalList [] env exprs


    and apply func arg env =
        printfn "App (%A) (%A)" func arg |> ignore 
        match func with
        | Closure (Lambda (var, body), closureEnv) ->
            eval body (Map.add var arg closureEnv)

        | RClosure (Lambda (var, body), closureEnv, name) ->
            let env' = closureEnv |> Map.add var arg |> Map.add name func
            eval body env'

        | Partial(name, remaining, args) ->
            if remaining = 1 then
                let allArgs = List.rev (arg :: args)
                operations name allArgs, env
            else
                Partial(name, remaining - 1, arg :: args), env

    let interpret (exprs: Expr list) (env: Env) : Expr * Env =
        // пробегаемся по всем выражениям, последовательно обновляя окружение
        exprs
        |> List.fold (fun (_lastResult, env) expr ->
            eval expr env          // eval : Expr -> Env -> Expr * Env
        ) (Int 0, env)             // стартовое значение: "результат 0", исходное окружение


        