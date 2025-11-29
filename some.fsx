type Expr =
    | Int of int
    | Var of string
    | Bool of bool
    | Lambda of string * Expr
    | Gets of string * Expr
    | GetsRec of string * Expr
    | If of Expr * Expr * Expr
    | PredefFunc of string
    | List of List<Expr>



let operations = function
    | "+" -> (function [Int(a);Int(b)] -> Int(a+b))
    | "-" -> (function [Int(a);Int(b)] -> Int(a-b))
    | "*" -> (function [Int(a);Int(b)] -> Int(a*b))
    | "/" -> (function [Int(a);Int(b)] -> Int(a/b))

    | "==" -> (function [Int(a);Int(b)] -> if a=b then Bool(true) else Bool(false))
    | "!=" -> (function [Int(a);Int(b)] -> if a<>b then Bool(true) else Bool(false))
    | ">" -> (function [Int(a);Int(b)] -> if a>b then Bool(true) else Bool(false))
    | ">=" -> (function [Int(a);Int(b)] -> if a>=b then Bool(true) else Bool(false))
    | "<" -> (function [Int(a);Int(b)] -> if a<b then Bool(true) else Bool(false))
    | "<=" -> (function [Int(a);Int(b)] -> if a<=b then Bool(true) else Bool(false))


// let x = Let("f",Lambda("x",Apply(Apply(PredefFunc("+"),Var("x")),Int(1))),Apply(Var("f"),Int(5)))



type Token =
    | TokenInt of int
    | TokenVar of string
    | TokenOperation of string
    | TokenBool of bool
    | TokenIf
    | TokenLeftBracket
    | TokenRightBracket
    | TokenLambda               // '\'
    | TokenRightArrow           // '->'
    | TokenGets                 // '<-'
    | TokenLBracketList         // '['
    | TokenRBracketList         // ']'
    | TokenCurry                // '^'
    | TokenRecursive            // '$'

let rec tokenize_number acc line = 
    match line with
    | head::tail when System.Char.IsDigit head ->
        tokenize_number (acc + string head) tail
    | _ -> 
        acc, line
    

let rec tokenize_variable acc line =
    match line with
    | head :: tail when System.Char.IsLetterOrDigit head || head = '_' ->
        tokenize_variable (acc + string head) tail
    | _ ->
        acc, line


let tokenize_equality_operations acc line = 
    match line with 
    | head::tail when head = '=' ->
        acc + string head, tail
    | _ ->
        acc, line

let tokenize (line : string) =
    let chars' = Seq.toList line

    let rec loop (chars : List<char>) acc =
        match chars with
        | [] ->
            List.rev acc
        | ch::tail -> 
            match ch with
            | c when System.Char.IsWhiteSpace c ->
                loop tail acc
            // пока все скобки будем игнорировать - они будут полезны лишь для
            // повышения читаемости
            | '(' | ')' ->
                loop tail acc
            // | '(' ->
            //     loop tail (TokenLeftBracket::acc)
            // | ')' -> 
            //     loop tail (TokenRightBracket::acc)
            | '\\' ->
                loop tail (TokenLambda::acc)
            | '+' | '*' | '/' ->
                loop tail (TokenOperation (string ch)::acc)
            | '-' ->
                match tail with
                | '>'::tail1 -> 
                    loop tail1 (TokenRightArrow::acc)
                | _ ->
                    loop tail (TokenOperation (string ch)::acc)
            | c when System.Char.IsDigit c ->
                let (number, line') =  tokenize_number (string ch) tail
                loop line' (TokenInt (int number)::acc)
            | c when System.Char.IsLetter c ->
                let (variable, line') =  tokenize_variable (string ch) tail
                let token =
                    match variable with
                    | "true" -> TokenBool true
                    | "false" -> TokenBool false
                    | "if"    -> TokenIf
                    | _       -> TokenVar variable
                loop line' (token::acc)
            | '<' ->
                match tail with
                | '-'::tail' ->
                    loop tail' (TokenGets :: acc)
                | _ ->
                    let (op, rest) = tokenize_equality_operations (string ch) tail
                    loop rest (TokenOperation op :: acc)
            | '=' | '>' | '!' ->
                let (operation, line') =  tokenize_equality_operations (string ch) tail
                loop line' (TokenOperation operation::acc)
            | '[' ->
                loop tail (TokenLBracketList :: acc)
            | ']' ->
                loop tail (TokenRBracketList :: acc)
            | '^' ->
                loop tail (TokenCurry :: acc)
            | '$' ->
                loop tail (TokenRecursive :: acc)
            // | ',' ->
            //     loop tail (TokenComma :: acc)
            | _ ->
                failwithf "Unknown character '%c'" ch
            

    loop chars' []


tokenize "x>=(10+4)*4[5,3,   1]"

let parseExpr (tokens: List<Token>) = 
    let rec parseAll (tokens : List<Token>) (acc : List<Expr>) =
        match tokens with
        | [] -> List.rev acc
        | _ ->
            let expr, rest = parse tokens
            parseAll rest (expr :: acc)

    and parse (tokens : List<Token>) =
        match tokens with
        | TokenInt n::tail -> Int n, tail

        | TokenBool b::tail -> Bool b, tail
        
        | TokenVar v::tail -> Var v, tail

        | TokenOperation op :: tail ->
            let arg1, tail1 = parse tail
            let arg2, tail2 = parse tail1
            Apply(Apply(PredefFunc op, arg1), arg2), tail2

        | TokenIf::tail ->
            let cond, tail1 = parse tail
            let thenExpr, tail2 = parse tail1
            let elseExpr, tail3 = parse tail2
            If(cond, thenExpr, elseExpr), tail3

        | TokenGets::TokenVar v::tail ->
            let boundExpr, tail1 = parse tail
            Gets(v, boundExpr), tail1

        | TokenLambda :: TokenVar v :: TokenRightArrow :: tail ->
            let body, tail1 = parse tail
            Lambda(v, body), tail1

        | TokenLBracketList::tail ->
            let rec parseList tokens acc =
                match tokens with
                | TokenRBracketList :: tail' -> List.rev acc, tail'
                | _ ->
                    let arg, tail1 = parse tokens
                    parseList tail1 (arg :: acc)
            let args, tail1 = parseList tail []
            List(args), tail1

        | TokenCurry::TokenVar func::tail ->


        | TokenRecursive::


            
    // let expr, tail = parse tokens
    // match tail with 
    //     | [] -> expr
    //     | _ -> failwith "Unexpected tokens after expression"
    parseAll tokens []


let tokens = tokenize "<- fact \ x ->
                                                if 
                                                    <= x 1
                                                    1
                                                    * x (fact - x 1)"
let ast = parseExpr tokens
printfn "%A" ast


let myList = tokenize "<- x [5 4 6]"
let list_ast = parseExpr myList
printfn "%A" myList