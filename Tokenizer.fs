module Tokenizer = 

    type Token =
        | TokenInt of int
        | TokenVar of string
        | TokenOperation of string
        | TokenBool of bool
        | TokenIf                   // 'if'
        | TokenLeftBracket          // '('
        | TokenRightBracket         // ')'
        | TokenLambda               // '\'
        | TokenRightArrow           // '->'
        | TokenGets                 // '<-'
        | TokenLBracketList         // '['
        | TokenRBracketList         // ']'
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
                | '(' ->
                    loop tail (TokenLeftBracket::acc)
                | ')' -> 
                    loop tail (TokenRightBracket::acc)
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
                | '$' ->
                    loop tail (TokenRecursive :: acc)
                | _ ->
                    failwithf "Unknown character '%c'" ch
                

        loop chars' []
