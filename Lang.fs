module Lang =

    type Expr =
        | Int of int
        | Var of string
        | Bool of bool
        | Lambda of string * Expr
        | Apply of Expr * Expr
        | Gets of string * Expr
        | GetsRec of string * Expr
        | If of Expr * Expr * Expr
        | PredefFunc of string
        | List of List<Expr>
        | Closure of Expr * Env
        | RClosure of Expr * Env * string
        | Partial of string * int * Expr list 
    and Env = Map<string, Expr>
