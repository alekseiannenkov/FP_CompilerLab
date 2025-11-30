# Language Specification

## 1. Syntax Overview

Expressions:
```
Int n
Bool b
Var name
Lambda(x, body)
Apply(f, x)
If(cond, t, e)
Let(name, expr, body)
LetRec(...)
List [...]
PredefFunc name
```

### Let-binding

```
<- x expr
```

### Recursive Let-binding

```
$ <- f expr
```

### Lambda

```
\ x -> expr
```

### Application

```
(f x)
(f a b c)
```

### Lists

```
[1 2 3]
[ x y (f z) ]
```

### If-expression

```
if cond a b
```

---

## 2. Built-in Functions

Arithmetic:
```
+ - * /
```

Comparisons:
```
== != < <= > >=
```

List ops:
```
head
tail
isEmpty
length
```

Higher-order:
```
map
filter
fold
mapfold
```
