; Fibonacci
(fun {fibonacci n} {
  select
    { (== n 0) 0 }
    { (== n 1) 1 }
    { otherwise (+ (fibonacci (- n 1)) (fibonacci (- n 2))) }
})


(print "fibonacci 10 is" (fibonacci 10))


