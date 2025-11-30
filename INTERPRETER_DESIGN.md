# Interpreter Design

## Architecture

```
Code → Tokenizer → Parser → AST → Interpreter (eval/apply) → Value
```

### Core Concepts

- `Expr` — all AST nodes
- `Environment` = Map<string, Expr>
- `Closure` = (lambda, env)
- `RClosure` = for recursion
- `Partial` = curried built-in function

---

## eval

Evaluates expressions:
- resolves variables
- evaluates lists
- evaluates conditionals
- produces closures for lambdas

## apply

Handles:
- closure invocation
- recursive closure invocation
- partial built-in ops
- full application of built-ins

---

## Built-in Ops

Unified under:

```
operations name args env
```

Includes arithmetic, comparisons, list ops, HOFs.

---