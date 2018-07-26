
;; List functions
;

; Prepend the value 'item' to the q-expression list'
; cons 1 {2 3} => {1 2 3}
(fun {cons item qexpr} {
  join (list item) qexpr
})

; Return all of list but last element.
; init {1 2 3} => {1 2}
(fun {init qexpr} {
  if (== (tail qexpr) nil)
    {nil}
    {join (head qexpr) (init (tail qexpr))}
})

; Recusively determine the length of a list
; len {1 2 3} => 3
(fun {len qexpr} {
  if (== qexpr {})
    {0}
    {+ 1 (len (tail qexpr))}
})

; Recursively reverse a list
; rev {1 2 3} => {3 2 1}
(fun {rev qexpr} {
  if(== qexpr {})
    {{}}
    {join (rev (tail qexpr)) (head qexpr)}
})

; Return the first item in a list
; first {1 2 3} => 1
(fun {first qexpr} {
  (eval (head qexpr))
})

(fun {second qexpr} { eval (head (tail qexpr)) })

; Recursively return the nth (zero based) item in a list
; nth 1 {1 2 3} => 2
(fun {nth n qexpr} {
  if(== n 0)
    {first qexpr}
    {nth (- n 1) (tail qexpr)} 
})

; Return last item in a list
; last {1 2 3} => 3
(fun {last qexpr} {
  nth (- (len qexpr) 1) qexpr
})

; Recusively determine if an element is in a list
; member 3 {1 2 3} => 1
; member 4 {1 2 3} => 0
(fun {member e qexpr} {
  if(== qexpr nil)
    {false}
    {if (== e (first qexpr))
      {true}
      {member e (tail qexpr)}}
})

; Deterimine the (last) index of an element in a list
; lastindexof 2 {1 2 3 2} => 3
(fun {lastindexof e qexpr}{
  if(== qexpr nil) 
    {- 1}
    {if (== e (last qexpr))
      {- (len qexpr) 1}
      {lastindexof e (init qexpr)}}
})

; Determine the first index of an element in a list
;indexof 2 {1 2 3 2} => 1
(fun {indexof e qexpr} {
  do 
    (= {r} (lastindexof e (rev qexpr)))
    (if (== r (- 1))
      {- 1}
      { - (len qexpr) 1 r})
})

; Apply a function to list
; map print {"hello" "goodbye"} => "hello" "goodbye" {() ()}
(fun {map function qexpr} {
  if (== qexpr nil)
    {nil}
    {join (list (function (first qexpr))) (map function (tail qexpr))}
})

; Apply filter to list
; fun {is-even n} {== (% n 2) 0}
; filter is-even {1 2 3 4 5 6 7 8} => {2 4 6 8}
(fun {filter predicate qexpr} {
  if (== qexpr nil)
    {nil}
    {join (
      if (predicate (first qexpr)) 
        {head qexpr} 
        {nil}) 
      (filter predicate (tail qexpr))}
})

; Fold left aka reduce
; fun {plus a b} {do (print a b) (+ a b)}
; foldl plus 0 {1 2 3 4} => 10
(fun {foldl function base qexpr} {
  if (== qexpr nil) 
    {base}
    {foldl function (function base (first qexpr)) (tail qexpr)}
})

; Fold right
; foldr plus 0 {1 2 3 4}

(fun {foldr function base qexpr} {
  if (== qexpr nil) 
    {base}
    {function (first qexpr) (foldr function base (tail qexpr))}
})

; Fold examples
; sum {1 2 3}
(fun {sum qexpr} {
  foldl + 0 qexpr
})
(fun {product qexpr} {
  foldl * 1 qexpr
})

; Take N items
; take 5 {1 2 3 4 5 6 7 8 9 10} => {1 2 3 4 5}
(fun {take n qexpr} {
  if (== n 0)
    {nil}
    {join (head qexpr) (take (- n 1) (tail qexpr))}
})

; Drop N items
; drop 5 {1 2 3 4 5 6 7 8 9 10} => {6 7 8 9 10}
(fun {drop n qexpr} {
  if (== n 0)
    {qexpr}
    {drop (- n 1) (tail qexpr)}
})

; Split at N
; split 5 {1 2 3 4 5 6 7 8 9 10} => {1 2 3 4 5} {6 7 8 9 10}}
(fun {split n qexpr} {
  list (take n qexpr) (drop n qexpr)
})

; Zip two lists together into a list of pairs
; zip {"a" "b"} {1 2} => {{"a" 1} {"b" 2}}
(fun {zip qexpr1 qexpr2} {
  if (or (== qexpr1 nil) (== qexpr2 nil))
    {nil}
    {join (list (join (head qexpr1) (head qexpr2))) (zip (tail qexpr1) (tail qexpr2))}
})

; Unzip a list of pairs into two lists
; unzip {{"a" 1} {"b" 2}} => {{"a" "b"} {1 2}}
(fun {unzip qexpr} {
  if (== qexpr nil)
    {{nil nil}}
    {do
      (= {x} (first qexpr))
      (= {xs} (unzip (tail qexpr)))
      (list (join (head x) (first xs)) (join (tail x) (nth 1 xs)))
    }
})

; Find element in list of pairs
; lookup "b" (zip {"a" "b" "c"} {1 2 3})
(fun {lookup x qexpr} {
  if (== qexpr nil)
    {error "No Element Found"}
    {do
      (= {key} (first (first qexpr)))
      (= {val} (nth 1 (first qexpr)))
      (if (== key x) 
        {val} 
        {lookup x (tail qexpr)})
    }
})
