module Interpreter =    

    open Parser
    open Tokenizer
    open Lang

    let predefArity name =
        match name with
        | "+" | "-" | "*" | "/" 
        | "==" | "!=" | ">" | ">=" | "<" | "<=" -> 2

        | "head" | "tail" | "isEmpty" | "length" -> 1
        | "map" | "filter" -> 2
        | "fold" | "mapfold" -> 3

        | _ -> failwithf "Unknown predef function %s" name

    let rec eval (expr: Expr) (env: Env) : Expr * Env = 
        // printfn "Eval %A in %A" expr env
        match expr with
        | Int x -> Int x, env

        | Bool x -> Bool x, env

        | Var x -> Map.find x env, env

        | PredefFunc name ->
            Partial(name, predefArity name, []), env

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
        // printfn "App (%A) (%A)" func arg |> ignore 
        match func with
        | Closure (Lambda (var, body), closureEnv) ->
            eval body (Map.add var arg closureEnv)

        | RClosure (Lambda (var, body), closureEnv, name) ->
            let env' = closureEnv |> Map.add var arg |> Map.add name func
            eval body env'

        | Partial(name, remaining, args) ->
            if remaining = 1 then
                let allArgs = List.rev (arg :: args)
                operations name allArgs env
            else
                Partial(name, remaining - 1, arg :: args), env

    and operations name args env : Expr * Env =
        match name, args with
        | "+",  [Int a; Int b] -> Int (a + b), env
        | "-",  [Int a; Int b] -> Int (a - b), env
        | "*",  [Int a; Int b] -> Int (a * b), env
        | "/",  [Int a; Int b] -> Int (a / b), env

        | "==", [Int a; Int b] -> Bool (a = b), env
        | "!=", [Int a; Int b] -> Bool (a <> b), env
        | ">",  [Int a; Int b] -> Bool (a > b), env
        | ">=", [Int a; Int b] -> Bool (a >= b), env
        | "<",  [Int a; Int b] -> Bool (a < b), env
        | "<=", [Int a; Int b] -> Bool (a <= b), env

        | "head",   [List (x :: _)] -> x, env
        | "head",   [List []] -> failwith "head: empty list"

        | "tail",   [List (_ :: xs)] -> List xs, env
        | "tail",   [List []] -> failwith "tail: empty list"

        | "isEmpty",[List xs] -> Bool (List.isEmpty xs), env
        | "length", [List xs] -> Int (List.length xs), env

        | "map", [func; List xs] ->
            let rec loop xs env acc =
                match xs with
                | [] -> List (List.rev acc), env
                | v :: vs ->
                    let v', env1 = apply func v env
                    loop vs env1 (v' :: acc)
            loop xs env []

        | "filter", [func; List xs] ->
            let rec loop xs env acc =
                match xs with
                | [] -> List (List.rev acc), env
                | v :: vs ->
                    let cond, env1 = apply func v env
                    match cond with
                    | Bool true  -> loop vs env1 (v :: acc)
                    | Bool false -> loop vs env1 acc
            loop xs env []

        | "fold", [func; init; List xs] ->
            let rec loop xs acc env =
                match xs with
                | [] -> acc, env
                | v :: vs ->
                    // f is a carried function of 2 arguments
                    let fApplied, env1 = apply func acc env
                    let acc',     env2 = apply fApplied v env1
                    loop vs acc' env2
            loop xs init env

        // result of mapfold is List [ List mapped; finalState ]
        | "mapfold", [func; initState; List xs] ->
            let rec loop xs state env accMapped =
                match xs with
                | [] -> List [ List (List.rev accMapped); state ], env
                | v :: vs ->
                    let fApplied, env1 = apply func state env
                    let res,      env2 = apply fApplied v env1
                    match res with
                    | List [mapped; newState] ->
                        loop vs newState env2 (mapped :: accMapped)
                    | _ ->
                        failwith "mapfold: function must return [mapped; newState]"
            loop xs initState env []

        | _ ->
            failwithf "Unknown operation %s with args %A" name args


    let interpret (exprs: Expr list) (env: Env) : Expr * Env =
        // run through all the expressions, consistently updating the environment
        exprs
        |> List.fold (fun (_lastResult, env) expr ->
            eval expr env          // eval : Expr -> Env -> Expr * Env
        ) (Int 0, env)             // initial state: "result 0", initial environment


        