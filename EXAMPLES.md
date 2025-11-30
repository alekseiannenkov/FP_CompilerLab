# Example Programs

## Factorial

```
$ <- fact \ n ->
    if <= n 1
        1
        * n (fact (- n 1))

(fact 5)
```

## Tail-rec Fibonacci

```
$ <- fibTR \ state ->
    if <= (head state) 0
        (head (tail state))
        (fibTR [
            - (head state) 1
            (head (tail (tail state)))
            + (head (tail state)) (head (tail (tail state)))
        ])
```

## Map

```
(map \ x -> + x 1 [1 2 3])
```

## Mapfold prefix sums

```
(mapfold \ s x -> [ + s x + s x ] 0 [1 2 3])
```
