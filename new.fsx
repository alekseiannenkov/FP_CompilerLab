// module Lang

// type Expr =
//     | Int of int
//     | Var of string
//     | Bool of bool
//     | Lambda of string * Expr
//     | Apply of Expr * Expr
//     // | Let of string * Expr * Expr
//     | Gets of string * Expr
//     | GetsRec of string * Expr
//     | If of Expr * Expr * Expr
//     | PredefFunc of string
//     | List of List<Expr>


// let operations = function
//     | "+" -> (function [Int(a);Int(b)] -> Int(a+b))
//     | "-" -> (function [Int(a);Int(b)] -> Int(a-b))
//     | "*" -> (function [Int(a);Int(b)] -> Int(a*b))
//     | "/" -> (function [Int(a);Int(b)] -> Int(a/b))

//     | "==" -> (function [Int(a);Int(b)] -> if a=b then Bool(true) else Bool(false))
//     | "!=" -> (function [Int(a);Int(b)] -> if a<>b then Bool(true) else Bool(false))
//     | ">" -> (function [Int(a);Int(b)] -> if a>b then Bool(true) else Bool(false))
//     | ">=" -> (function [Int(a);Int(b)] -> if a>=b then Bool(true) else Bool(false))
//     | "<" -> (function [Int(a);Int(b)] -> if a<b then Bool(true) else Bool(false))
//     | "<=" -> (function [Int(a);Int(b)] -> if a<=b then Bool(true) else Bool(false))


// // let x = Let("f",Lambda("x",Apply(Apply(PredefFunc("+"),Var("x")),Int(1))),Apply(Var("f"),Int(5)))



// type Token =
//     | TokenInt of int
//     | TokenVar of string
//     | TokenOperation of string
//     | TokenBool of bool
//     | TokenIf
//     | TokenLeftBracket
//     | TokenRightBracket
//     | TokenLambda               // '\'
//     | TokenRightArrow           // '->'
//     | TokenGets                 // '<-'
//     | TokenLBracketList         // '['
//     | TokenRBracketList         // ']'
//     | TokenCurry                // '^'
//     | TokenRecursive            // '$'

// let rec tokenize_number acc line = 
//     match line with
//     | head::tail when System.Char.IsDigit head ->
//         tokenize_number (acc + string head) tail
//     | _ -> 
//         acc, line
    

// let rec tokenize_variable acc line =
//     match line with
//     | head :: tail when System.Char.IsLetterOrDigit head || head = '_' ->
//         tokenize_variable (acc + string head) tail
//     | _ ->
//         acc, line


// let tokenize_equality_operations acc line = 
//     match line with 
//     | head::tail when head = '=' ->
//         acc + string head, tail
//     | _ ->
//         acc, line

// let tokenize (line : string) =
//     let chars' = Seq.toList line

//     let rec loop (chars : List<char>) acc =
//         match chars with
//         | [] ->
//             List.rev acc
//         | ch::tail -> 
//             match ch with
//             | c when System.Char.IsWhiteSpace c ->
//                 loop tail acc

//             // | '(' | ')' ->
//             //     loop tail acc

//             // скобки для application 
//             | '(' ->
//                 loop tail (TokenLeftBracket::acc)
//             | ')' -> 
//                 loop tail (TokenRightBracket::acc)
//             | '\\' ->
//                 loop tail (TokenLambda::acc)
//             | '+' | '*' | '/' ->
//                 loop tail (TokenOperation (string ch)::acc)
//             | '-' ->
//                 match tail with
//                 | '>'::tail1 -> 
//                     loop tail1 (TokenRightArrow::acc)
//                 | _ ->
//                     loop tail (TokenOperation (string ch)::acc)
//             | c when System.Char.IsDigit c ->
//                 let (number, line') =  tokenize_number (string ch) tail
//                 loop line' (TokenInt (int number)::acc)
//             | c when System.Char.IsLetter c ->
//                 let (variable, line') =  tokenize_variable (string ch) tail
//                 let token =
//                     match variable with
//                     | "true" -> TokenBool true
//                     | "false" -> TokenBool false
//                     | "if"    -> TokenIf
//                     | _       -> TokenVar variable
//                 loop line' (token::acc)
//             | '<' ->
//                 match tail with
//                 | '-'::tail' ->
//                     loop tail' (TokenGets :: acc)
//                 | _ ->
//                     let (op, rest) = tokenize_equality_operations (string ch) tail
//                     loop rest (TokenOperation op :: acc)
//             | '=' | '>' | '!' ->
//                 let (operation, line') =  tokenize_equality_operations (string ch) tail
//                 loop line' (TokenOperation operation::acc)
//             | '[' ->
//                 loop tail (TokenLBracketList :: acc)
//             | ']' ->
//                 loop tail (TokenRBracketList :: acc)
//             | '^' ->
//                 loop tail (TokenCurry :: acc)
//             | '$' ->
//                 loop tail (TokenRecursive :: acc)
//             // | ',' ->
//             //     loop tail (TokenComma :: acc)
//             | _ ->
//                 failwithf "Unknown character '%c'" ch
            

//     loop chars' []

// let rec buildNestedLambda (args: string list) (body: Expr) : Expr =
//     match args with
//     | [] -> body
//     | hd::tl -> Lambda(hd, buildNestedLambda tl body)

// let parseExpr (tokens: List<Token>) = 
//     let rec parseAll (tokens : List<Token>) (acc : List<Expr>) =
//         match tokens with
//         | [] -> List.rev acc
//         | _ ->
//             let expr, rest = parse tokens
//             parseAll rest (expr :: acc)

//     and parse (tokens : List<Token>) =
//         match tokens with
//         | TokenInt n::tail -> Int n, tail

//         | TokenBool b::tail -> Bool b, tail
        
//         | TokenVar v::tail -> Var v, tail

//         | TokenOperation op :: tail ->
//             let arg1, tail1 = parse tail
//             let arg2, tail2 = parse tail1
//             Apply(Apply(PredefFunc op, arg1), arg2), tail2

//         | TokenIf::tail ->
//             let cond, tail1 = parse tail
//             let thenExpr, tail2 = parse tail1
//             let elseExpr, tail3 = parse tail2
//             If(cond, thenExpr, elseExpr), tail3

//         | TokenGets::TokenVar v::tail ->
//             let boundExpr, tail1 = parse tail
//             Gets(v, boundExpr), tail1


//         // Generated by ChatGPT
//         | TokenLambda :: tail ->
//             let rec gatherVars tokens acc =
//                 match tokens with
//                 | TokenVar v :: rest -> gatherVars rest (v :: acc)
//                 | TokenRightArrow :: rest -> List.rev acc, rest
//                 | _ -> failwith "Invalid lambda parameter list"

//             let vars, rest = gatherVars tail []
//             let body, remaining = parse rest

//             // Превращаем список параметров в вложенные лямбды
//             let lam = List.foldBack (fun v acc -> Lambda(v, acc)) vars body
//             lam, remaining
//         // until here


//         // Generated by ChatGPT
//         | TokenRecursive :: TokenGets :: TokenVar name :: TokenLambda :: tail ->
//             // Соберём список аргументов до ->
//             let rec collectArgs toks acc =
//                 match toks with
//                 | TokenVar arg :: rest -> collectArgs rest (arg::acc)
//                 | TokenRightArrow :: rest -> List.rev acc, rest
//                 | _ -> failwith "Invalid function argument list in recursive definition"

//             let args, afterArrow = collectArgs tail []
//             let body, rest = parse afterArrow
//             GetsRec(name, buildNestedLambda args body), rest
//         // until here


//         // Generated by ChatGPT
//         | TokenLeftBracket :: tail ->
//             // Разбираем все выражения внутри скобок
//             let rec parseExprs tokens acc =
//                 match tokens with
//                 | TokenRightBracket :: rest -> List.rev acc, rest
//                 | [] -> failwith "Unclosed parenthesis"
//                 | _ ->
//                     let expr, rest = parse tokens
//                     parseExprs rest (expr :: acc)

//             let exprs, rest = parseExprs tail []

//             match exprs with
//             | [] -> failwith "Empty application"
//             | f :: args ->
//                 let app = List.fold (fun acc arg -> Apply(acc, arg)) f args
//                 app, rest
//         // until here


//         | TokenLBracketList::tail ->
//             let rec parseList tokens acc =
//                 match tokens with
//                 | TokenRBracketList :: tail' -> List.rev acc, tail'
//                 | _ ->
//                     let arg, tail1 = parse tokens
//                     parseList tail1 (arg :: acc)
//             let args, tail1 = parseList tail []
//             List(args), tail1
            
//     parseAll tokens []


// let tokens = tokenize "$ <- fact \ x y z ->
//                                                 if 
//                                                     <= x 1
//                                                     1
//                                                     * x (fact - x 1)"
// let ast = parseExpr tokens
// printfn "%A" ast


// let myList = tokenize "<- x (f [a b c] 5)"
// let list_ast = parseExpr myList
// printfn "%A" list_ast

// let myLambda = tokenize "<- add \ x y -> + x y"
// let lambdaAst = parseExpr myLambda
// printfn "%A" lambdaAst