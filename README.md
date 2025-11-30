# Custom Functional Programming Language

This project implements a custom **functional programming language** created as part of the *Functional Programming* course at HSE.  
The assignment required inventing a language, implementing its interpreter, extending it with functional features, documenting the work and writing tests.

---

# Key Features

- First-class functions
- Lambda expressions (`\ x -> expr`)
- Lexical closures
- Recursive functions (`$ <- f ...`)
- Tail recursion
- Immutable values
- Let-bindings (`<- x expr`)
- If-expressions
- Lists with `[1 2 3]` syntax
- Built-in list functions:
  - `head`, `tail`, `isEmpty`, `length`
- Higher-order functions:
  - `map`, `filter`, `fold`, `mapfold`
- Curried built-in operators (`+ - * / == != > >= < <=`)
- Full interpreter test suite
- Many example programs

---

# Project Structure

```
Lang.fs               — AST definitions and types
Tokenizer.fs          — Tokenizer
Parser.fs             — Parser
Interpreter.fs        — Interpreter
InterpreterTests.fs   — tests
Program.fs            — entry point
README.md
LANGUAGE_SPEC.md
INTERPRETER_DESIGN.md
TESTING.md
EXAMPLES.md
LLM_USAGE.md
```

---

# Language Overview

```plain
$ <- fact \ n ->
    if <= n 1
        1
        * n (fact (- n 1))

<- x (fact 5)
x
```

Output: `120`

More examples inside EXAMPLES.md.

---
