;; Funtions
;;

; A function to create functions using an easier syntax
(def {fun} (\ {args body} {
  def (head args) (\ (tail args) body)
}))

; Unpack list and curry alias
; Allows us to call a function taking multiple arguments with a single list
; we can also think of functions themselves as existing in curried and uncurried forms
; curry + {5 6 7} => join (list +) l => eval {+ 5 6 7} => 18
; def {curry-add} (curry +)
; curry-add {5 6 7} 
(fun {unpack function qexpr} {
  eval (join (list function) qexpr)
})
(def {curry} unpack)

; Pack  and uncurry alias
; The opposite of currying - allows us to call a function accepting list with multiple args
; uncurry head 5 6 7 => head {5 6 7} => {5}
(fun {pack function & xs} {
  function xs
})
(def {uncurry} pack)

; Open a new scope - defines a lamba function with a caller defined body (\ {_} b) which is then invoked with nil.
; Caller can work inside this body, including making assignments to the local scope *without* cluttering the global scope.
; This is particularly useful when combined with let.
; let {= {x} 77} => (), x => Undefined symbol
(fun {let b} {
  ((\ {_} b) nil)
})

; Perform Several things in Sequence then return the last one
; do (print "hello") (print "goodbye") (+ 1 2 3)
; This is particularly useful when combined with let.
; let {do (= {x} 123) (= {y} 456) (+ x y)} => 579
; x => Undefined symbol
(fun {do & l} {
  if (== l nil)
    {nil}
    {last l}
})